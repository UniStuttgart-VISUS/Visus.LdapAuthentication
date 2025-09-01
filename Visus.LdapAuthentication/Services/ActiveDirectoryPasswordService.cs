// <copyright file="ActiveDirectoryPasswordService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2025 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Visus.LdapAuthentication.Configuration;
using Visus.LdapAuthentication.Extensions;
using Visus.LdapAuthentication.Properties;
using Visus.Ldap;
using Visus.Ldap.Extensions;
using Novell.Directory.Ldap;


namespace Visus.LdapAuthentication.Services {

    /// <summary>
    /// Implements <see cref="ILdapPasswordService"/> for an Active Directory
    /// server, including Identity Management for UNIX (IDMU).
    /// </summary>
    public sealed class ActiveDirectoryPasswordService : ILdapPasswordService {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP options configuring the behaviour of
        /// the server.</param>
        /// <param name="connectionService">A connection service that allows for
        /// connecting using an administrative account and as user.</param>
        /// <param name="logger">A logger for the service.</param>
        /// <exception cref="ArgumentNullException">If any of the parameters is
        /// <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If the
        /// <paramref name="options"/> do not hold sufficient information for
        /// the service to perform its task.</exception>
        public ActiveDirectoryPasswordService(IOptions<LdapOptions> options,
                ILdapConnectionService connectionService,
                ILogger<ActiveDirectoryPasswordService> logger) {
            this._connectionService = connectionService
                ?? throw new ArgumentNullException(nameof(connectionService));
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));

            var mapping = this._options.Mapping ?? throw new ArgumentException(
                Resources.ErrorNoMapping, nameof(options));
            _ = mapping.PasswordAttribute ?? throw new ArgumentNullException(
                Resources.ErrorNoPasswordAttribute, nameof(options));
            this._attributes = [ mapping.DistinguishedNameAttribute,
                mapping.PasswordAttribute ];
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public void ChangePassword(string userName,
                string oldPassword,
                string newPassword) {
            var connection = this.GetConnection(userName, oldPassword);
            var entry = this.GetEntry(connection, userName);
            var mods = this.GetModifications(entry, oldPassword, newPassword);
            connection.Modify(entry.Dn, mods);
        }

        /// <inheritdoc />
        public async Task ChangePasswordAsync(string userName,
                string oldPassword,
                string newPassword) {
            var connection = this.GetConnection(userName, oldPassword);
            var entry = await this.GetEntryAsync(connection, userName);
            var mods = this.GetModifications(entry, oldPassword, newPassword);
            connection.Modify(entry.Dn, mods);
        }

        /// <inheritdoc />
        public void ChangePassword(string userName,
                string newPassword) {
            var connection = this.GetConnection(null, null);
            var entry = this.GetEntry(connection, userName);
            var mods = this.GetModifications(entry, null, newPassword);
            connection.Modify(entry.Dn, mods);
        }

        /// <inheritdoc />
        public async Task ChangePasswordAsync(string userName,
                string newPassword) {
            var connection = this.GetConnection(null, null);
            var entry = await this.GetEntryAsync(connection, userName);
            var mods = this.GetModifications(entry, null, newPassword);
            connection.Modify(entry.Dn, mods);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Get an LDAP connection as <paramref name="userName"/> or as the
        /// configured service account.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private LdapConnection GetConnection(string? userName, string? password)
            => this._connectionService.Connect(userName, password);

        /// <summary>
        /// Loads an entry matching the given user name.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private LdapEntry GetEntry(LdapConnection connection,
                string userName) {
            Debug.Assert(connection != null);
            var mapping = this._options.Mapping!;
            var filter = this._options.Mapping!.UserFilter;
            filter = string.Format(filter, userName);

            foreach (var b in this._options.SearchBases) {
                this._logger.LogTrace("Searching for \"{Filter}\" in "
                    + "\"{SearchBase}\".", filter, b);
                var entries = connection.Search(filter,
                    this._attributes,
                    this._options);

                if (entries.Any()) {
                    return entries.First();
                }
            }

            throw new ArgumentException(
                string.Format(Resources.ErrorEntryNotFound, userName),
                nameof(userName));
        }

        /// <summary>
        /// Loads an entry matching the given user name.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task<LdapEntry> GetEntryAsync(
                LdapConnection connection, string userName) {
            Debug.Assert(connection != null);
            var mapping = this._options.Mapping!;
            var filter = this._options.Mapping!.UserFilter;
            filter = string.Format(filter, userName);

            foreach (var b in this._options.SearchBases) {
                this._logger.LogTrace("Searching for \"{Filter}\" in "
                    + "\"{SearchBase}\".", filter, b);
                var entries = await connection.SearchAsync(filter,
                    this._attributes,
                    this._options);

                if (entries.Any()) {
                    return entries.First();
                }
            }

            throw new ArgumentException(
                string.Format(Resources.ErrorEntryNotFound, userName),
                nameof(userName));
        }

        /// <summary>
        /// Gets a modification request for the password.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        private LdapModification[] GetModifications(LdapEntry entry,
                string? oldPassword,
                string newPassword) {
            Debug.Assert(entry != null);
            Debug.Assert(newPassword != null);
            Debug.Assert(this._options.Mapping != null);
            Debug.Assert(this._options.Mapping.PasswordAttribute != null);

            if (oldPassword != null) {
                this._logger.LogInformation("{DN} is attempting to change its "
                    + "password via an add and a delete modification.",
                    entry.Dn);
                var att = new LdapAttribute(
                    this._options.Mapping.PasswordAttribute,
                    newPassword.ToActiveDirectoryPassword());
                var add = new LdapModification(LdapModification.Add, att);

                att = new LdapAttribute(
                    this._options.Mapping.PasswordAttribute,
                    oldPassword.ToActiveDirectoryPassword());
                var del = new LdapModification(LdapModification.Delete, att);
                return [ add, del ];

            } else {
                this._logger.LogInformation("{Account} is attemtping to change "
                    + "the password of {DN} is changing its password via a "
                    + "replace operation.", this._options.User, entry.Dn);
                var att = new LdapAttribute(
                    this._options.Mapping.PasswordAttribute,
                    newPassword.ToActiveDirectoryPassword());
                var mod = new LdapModification(LdapModification.Replace, att);
                return [ mod ];
            }
        }
        #endregion

        #region Private fields
        private readonly string[] _attributes;
        private readonly ILdapConnectionService _connectionService;
        private readonly ILogger _logger;
        private readonly LdapOptions _options;
        #endregion
    }
}
