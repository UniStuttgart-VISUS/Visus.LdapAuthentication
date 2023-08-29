// <copyright file="LdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2023 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using System;
using System.DirectoryServices.Protocols;
using System.Linq;


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
            : ILdapAuthenticationService where TUser : ILdapUser, new() {

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

        public object GetUserByIdentity(string existingUserIdentity) {
            throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// Performs an LDAP bind using the specified credentials and retrieves
        /// the LDAP entry with the account name <paramref name="username"/> in
        /// case the bind succeeds.
        /// </summary>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The user object in case of a successful login.</returns>
        public ILdapUser Login(string username, string password) {
            using var connection = this._options.Connect(username, password,
                this._logger);

            var retval = new TUser();
            var groupAttribs = this._options.Mapping.RequiredGroupAttributes;
            var filter = string.Format(this._options.Mapping.UserFilter,
                username);
            var request = new SearchRequest(this._options.SearchBase,
                filter,
                this._options.GetSearchScope(),
                retval.RequiredAttributes.Concat(groupAttribs).ToArray());

            var result = (this._options.Timeout > TimeSpan.Zero)
                ? connection.SendRequest(request, this._options.Timeout)
                : connection.SendRequest(request);

            if ((result is SearchResponse s) && s.Any()) {
                retval.Assign(s.Entries[0], connection, this._options);
                return retval;

            } else {
                this._logger.LogError(Properties.Resources.ErrorUserNotFound,
                    username);
                return null;
            }
        }

        #region Private fields
        private readonly ILogger _logger;
        private readonly ILdapOptions _options;
        #endregion
    }
}
