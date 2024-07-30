// <copyright file="LdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using System;
using System.Linq;
using Visus.Ldap;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;
using Visus.LdapAuthentication.Configuration;


namespace Visus.LdapAuthentication.Services {

    /// <summary>
    /// Implements an <see cref="IAuthenticationService"/> using Novell's LDAP
    /// library.
    /// </summary>
    /// <typeparam name="TUser">The type of the user object to be returned on
    /// login. This is typically something derived from
    /// <see cref="LdapUserBase"/> rather than a custom implementation of
    /// <see cref="ILdapUser"/>.</typeparam>
    public sealed class LdapAuthenticationService<TUser, TGroup>
            : ILdapAuthenticationService<TUser>
            where TUser : class, new ()
            where TGroup : class, new () {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="connectionService"></param>
        /// <param name="mapper"></param>
        /// <param name="claimsBuilder"></param>
        /// <param name="logger">A logger for presisting important messages like
        /// login failures.</param>
        /// <exception cref="ArgumentNullException">If any of the parameters is
        /// <c>null</c>.</exception>
        public LdapAuthenticationService(IOptions<LdapOptions> options,
                ILdapConnectionService connectionService,
                ILdapMapper<LdapEntry, TUser, TGroup> mapper,
                IClaimsBuilder<TUser, TGroup> claimsBuilder,
                ILogger<LdapAuthenticationService<TUser, TGroup>> logger) {
            this._claimsBuilder = claimsBuilder
                ?? throw new ArgumentNullException(nameof(claimsBuilder));
            this._connectionService = connectionService
                ?? throw new ArgumentNullException(nameof(connectionService));
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
            this._mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
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
        public TUser? LoginUser(string username, string password) {
            using var connection = _options.Connect(username, password,
                _logger);

            var retval = new TUser();
            var groupAttribs = _options.Mapping.RequiredGroupAttributes;
            var filter = string.Format(_options.Mapping.UserFilter,
                username);

            foreach (var b in _options.SearchBases) {
                var result = connection.Search(
                    b,
                    filter,
                    retval.RequiredAttributes.Concat(groupAttribs).ToArray(),
                    false);

                if (result.HasMore()) {
                    var entry = result.NextEntry(_logger);
                    if (entry != null) {
                        _mapper.Assign(retval, entry, connection,
                            _logger);
                        return retval;
                    }
                }
            }

            _logger.LogError(Properties.Resources.ErrorUserNotFound,
                username);
            return null;
        }

        #region Private fields
        private readonly IClaimsBuilder<TUser, TGroup> _claimsBuilder;
        private readonly ILdapConnectionService _connectionService;
        private readonly ILogger _logger;
        private readonly LdapOptions _options;
        private readonly ILdapMapper<LdapEntry, TUser, TGroup> _mapper;
        #endregion
    }
}
