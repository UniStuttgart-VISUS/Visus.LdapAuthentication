// <copyright file="LdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        /// <param name="mapper">The <see cref="ILdapUserMapper{TUser}"/> to
        /// provide mapping between LDAP attributes and properties of
        /// <typeparamref name="TUser"/>.</param>
        /// <param name="options">The LDAP options that specify how to connect
        /// to the directory server.</param>
        /// <param name="logger">A logger for writing important messages.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapAuthenticationService(ILdapUserMapper<TUser> mapper,
                IOptions<LdapOptions> options,
                ILogger<LdapAuthenticationService<TUser>> logger) {
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
            this._options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
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
                        this._mapper.Assign(retval, entry, connection,
                            this._logger);
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
        private readonly ILdapUserMapper<TUser> _mapper;
        private readonly IOptions _options;
        #endregion
    }
}
