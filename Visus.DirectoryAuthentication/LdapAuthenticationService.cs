// <copyright file="LdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2023 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using System;
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

        //public object GetUserByIdentity(string existingUserIdentity) {
        //    throw new NotImplementedException();
        //}
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
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._options.Connect(username ?? string.Empty,
                password ?? string.Empty, this._logger);

            var retval = new TUser();
            var request = this.GetRequest(retval, username);

            var result = connection.SendRequest(request, this._options);

            if ((result is SearchResponse s) && s.Any()) {
                retval.Assign(s.Entries[0], connection, this._options);
                return retval;

            } else {
                this._logger.LogError(Properties.Resources.ErrorUserNotFound,
                    username);
                return null;
            }
        }

        /// <summary>
        /// Asynchronously performs <see cref="Login(string, string)"/>.
        /// </summary>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The user object in case of a successful login.</returns>
        public Task<ILdapUser> LoginAsync(string username, string password) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._options.Connect(username ?? string.Empty,
                    password ?? string.Empty, this._logger);

            var retval = new TUser();
            var request = this.GetRequest(retval, username);

            return connection.SendRequestAsync(request, this._options)
                .ContinueWith<ILdapUser>(r => {
                    if ((r.Result is SearchResponse s) && s.Any()) {
                        retval.Assign(s.Entries[0], connection, this._options);
                        return retval;

                    } else {
                        this._logger.LogError(Properties.Resources.ErrorUserNotFound,
                            username);
                        return null;
                    }
                });
        }

        #region Private methods
        private SearchRequest GetRequest(TUser user, string username) {
            var groupAttribs = this._options.Mapping.RequiredGroupAttributes;
            var filter = string.Format(this._options.Mapping.UserFilter,
                username);
            var retval = new SearchRequest(this._options.SearchBase,
                filter,
                this._options.GetSearchScope(),
                user.RequiredAttributes.Concat(groupAttribs).ToArray());
            return retval;
        }
        #endregion

        #region Private fields
        private readonly ILogger _logger;
        private readonly ILdapOptions _options;
        #endregion
    }
}
