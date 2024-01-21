// <copyright file="LdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Threading.Tasks;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Implements an <see cref="IAuthenticationService"/> using
    /// <see cref="System.DirectoryServices.Protocols"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of the user object to be returned on
    /// login. This is typically something derived from
    /// <see cref="LdapUserBase"/> rather than a custom implementation of
    /// <see cref="ILdapUser"/>.</typeparam>
    public sealed class LdapAuthenticationService<TUser>
            : ILdapAuthenticationService<TUser>
            where TUser : class, ILdapUser, new() {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP options that specify how to connect
        /// to the directory server.</param>
        /// <param name="logger">A logger for writing important messages.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapAuthenticationService(ILdapOptions options,
                ILogger<LdapAuthenticationService<TUser>> logger) {
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._options = options
                ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP options that specify how to connect
        /// to the directory server.</param>
        /// <param name="logger">A logger for writing important messages.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapAuthenticationService(IOptions<LdapOptions> options,
                ILogger<LdapAuthenticationService<TUser>> logger) {
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public TUser Login(string username, string password) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._options.Connect(username ?? string.Empty,
                password ?? string.Empty, this._logger);

            var retval = new TUser();

            foreach (var b in this._options.SearchBase) {
                var req = this.GetRequest(retval, username, b);
                var res = connection.SendRequest(req, this._options);
                if ((res is SearchResponse s) && s.Any()) {
                    retval.Assign(s.Entries[0], connection, this._options);
                    return retval;
                }
            }

            // Not found ad this point.
            this._logger.LogError(Properties.Resources.ErrorUserNotFound,
                username);
            return null;
        }

        /// <inheritdoc />
        ILdapUser ILdapAuthenticationService.Login(string username,
                string password)
            => this.Login(username, password);

        /// <inheritdoc />
        public async Task<TUser> LoginAsync(string username,
                string password) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._options.Connect(username ?? string.Empty,
                    password ?? string.Empty, this._logger);

            var retval = new TUser();

            foreach (var b in this._options.SearchBase) {
                var req = this.GetRequest(retval, username, b);
                var res = await connection.SendRequestAsync(req, this._options);

                if ((res is SearchResponse s) && s.Any()) {
                    retval.Assign(s.Entries[0], connection, this._options);
                    return retval;
                }
            }

            // Not found ad this point.
            this._logger.LogError(Properties.Resources.ErrorUserNotFound,
                username);
            return null;
        }

        /// <inheritdoc />
        async Task<ILdapUser> ILdapAuthenticationService.LoginAsync(
                string username, string password)
            => await this.LoginAsync(username, password);
        #endregion

        #region Private methods
        private SearchRequest GetRequest(TUser user, string username,
                string searchBase, SearchScope scope) {
            Debug.Assert(searchBase != null);
            var groupAttribs = this._options.Mapping.RequiredGroupAttributes;
            var filter = string.Format(this._options.Mapping.UserFilter,
                username);
            var retval = new SearchRequest(searchBase,
                filter,
                scope,
                user.RequiredAttributes.Concat(groupAttribs).ToArray());
            return retval;
        }

        private SearchRequest GetRequest(TUser user, string username,
                KeyValuePair<string, SearchScope> searchBase)
            => this.GetRequest(user, username, searchBase.Key,
                searchBase.Value);
        #endregion

        #region Private fields
        private readonly ILogger _logger;
        private readonly ILdapOptions _options;
        #endregion
    }
}
