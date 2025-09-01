// <copyright file="ActiveDirectoryPasswordService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2025 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Extensions;
using Visus.DirectoryAuthentication.Properties;
using Visus.Ldap;
using Visus.Ldap.Extensions;


namespace Visus.DirectoryAuthentication.Services {

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
            var req = this.GetModification(entry, oldPassword, newPassword);
            var res = connection.SendRequest(req);
            // TODO: should we propagate the server error message here?
        }

        /// <inheritdoc />
        public async Task ChangePasswordAsync(string userName,
                string oldPassword,
                string newPassword) {
            var connection = this.GetConnection(userName, oldPassword);
            var entry = await this.GetEntryAsync(connection, userName);
            var req = this.GetModification(entry, oldPassword, newPassword);
            var res = await connection.SendRequestAsync(req);
            // TODO: should we propagate the server error message here?
        }

        /// <inheritdoc />
        public void ChangePassword(string userName,
                string newPassword) {
            var connection = this.GetConnection(null, null);
            var entry = this.GetEntry(connection, userName);
            var req = this.GetModification(entry, null, newPassword);
            var res = connection.SendRequest(req);
            // TODO: should we propagate the server error message here?
        }

        /// <inheritdoc />
        public async Task ChangePasswordAsync(string userName,
                string newPassword) {
            var connection = this.GetConnection(null, null);
            var entry = await this.GetEntryAsync(connection, userName);
            var req = this.GetModification(entry, null, newPassword);
            var res = await connection.SendRequestAsync(req);
            // TODO: should we propagate the server error message here?
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
        private SearchResultEntry GetEntry(LdapConnection connection,
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
        private async Task<SearchResultEntry> GetEntryAsync(
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
        private ModifyRequest GetModification(SearchResultEntry entry,
                string? oldPassword,
                string newPassword) {
            Debug.Assert(entry != null);
            Debug.Assert(newPassword != null);
            Debug.Assert(this._options.Mapping != null);
            Debug.Assert(this._options.Mapping.PasswordAttribute != null);

            var retval = new ModifyRequest(entry.DistinguishedName);

            if (oldPassword != null) {
                this._logger.LogInformation("{DN} is attempting to change its "
                    + "password via an add and a delete modification.",
                    entry.DistinguishedName);
                var add = new DirectoryAttributeModification {
                    Operation = DirectoryAttributeOperation.Add,
                    Name = this._options.Mapping.PasswordAttribute,
                };
                add.Add(newPassword.ToActiveDirectoryPassword());

                var del = new DirectoryAttributeModification {
                    Operation = DirectoryAttributeOperation.Delete,
                    Name = this._options.Mapping.PasswordAttribute,
                };
                del.Add(oldPassword.ToActiveDirectoryPassword());

                retval.Modifications.Add(add);
                retval.Modifications.Add(del);

            } else {
                this._logger.LogInformation("{Account} is attemtping to change "
                    + "the password of {DN} is changing its password via a "
                    + "replace operation.", this._options.User,
                    entry.DistinguishedName);
                var mod = new DirectoryAttributeModification {
                    Operation = DirectoryAttributeOperation.Replace,
                    Name = this._options.Mapping.PasswordAttribute,
                };
                mod.Add(newPassword.ToActiveDirectoryPassword());
                retval.Modifications.Add(mod);
            }

            return retval;
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
