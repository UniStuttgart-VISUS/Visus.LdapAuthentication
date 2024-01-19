// <copyright file="LdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using System;
using System.Linq;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Implements an <see cref="IAuthenticationService"/> using Novell's LDAP
    /// library.
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

        //public object GetUserByIdentity(string existingUserIdentity) {
        //    throw new NotImplementedException();
        //}
        #endregion

        /// <inheritdoc />
        public TUser Login(string username, string password) {
            using var connection = this._options.Connect(username, password,
                this._logger);

            var retval = new TUser();
            var groupAttribs = this._options.Mapping.RequiredGroupAttributes;
            var filter = string.Format(this._options.Mapping.UserFilter,
                username);

            foreach (var b in this._options.SearchBases) {
                var result = connection.Search(
                    b,
                    filter,
                    retval.RequiredAttributes.Concat(groupAttribs).ToArray(),
                    false);

                if (result.HasMore()) {
                    var entry = result.NextEntry(this._logger);
                    if (entry != null) {
                        retval.Assign(entry, connection, this._options);
                        return retval;
                    }
                }
            }

            this._logger.LogError(Properties.Resources.ErrorUserNotFound,
                username);
            return null;
        }

        /// <inheritdoc />
        ILdapUser ILdapAuthenticationService.Login(string username,
                string password)
            => this.Login(username, password);

        #region Private fields
        private readonly ILogger _logger;
        private readonly ILdapOptions _options;
        #endregion
    }
}
