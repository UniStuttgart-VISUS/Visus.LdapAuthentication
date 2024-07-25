// <copyright file="LdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Extensions;


namespace Visus.DirectoryAuthentication.Services
{

    /// <summary>
    /// Implements an <see cref="IAuthenticationService"/> using
    /// <see cref="System.DirectoryServices.Protocols"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of the user object to be returned on
    /// login. This is typically something derived from
    /// <see cref="LdapUserBase"/> to avoid implementing a custom mapper.
    /// </typeparam>
    public sealed class LdapAuthenticationService<TUser, TGroup>
            : ILdapAuthenticationService<TUser>
            where TUser : class, new()
            where TGroup : class, new() {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="connectionService">The connection service providing the
        /// LDAP connections along with the options.</param>
        /// <param name="mapper">The <see cref="ILdapMapper{TUser, TGroup}"/> to
        /// provide mapping between LDAP attributes and properties of
        /// <typeparamref name="TUser"/> and <typeparamref name="TGroup"/>, 
        /// respectively.</param>
        /// <param name="claimsBuilder">A helper that creates
        /// <see cref="Claim"/>s from a user object.</param>
        /// <param name="logger">A logger for presisting important messages like
        /// login failures.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapAuthenticationService(
                ILdapConnectionService connectionService,
                ILdapMapper<TUser, TGroup> mapper,
                IClaimsBuilder<TUser, TGroup> claimsBuilder,
                ILogger<LdapAuthenticationService<TUser, TGroup>> logger) {
            _claimsBuilder = claimsBuilder
                ?? throw new ArgumentNullException(nameof(claimsBuilder));
            _connectionService = connectionService
                ?? throw new ArgumentNullException(nameof(connectionService));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public TUser Login(string username, string password) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = _connectionService.Connect(
                username ?? string.Empty,
                password ?? string.Empty);
            _logger.LogDebug("Connected to {server} with authentication "
                    + "type {authType} and protocol version {version}. "
                    + "Retrieving user claims is next ...",
                    connection.SessionOptions.HostName,
                    connection.SessionOptions.ProtocolVersion,
                    connection.AuthType);

            var retval = new TUser();

            foreach (var b in Options.SearchBases) {
                var req = GetRequest(username, b);
                var res = connection.SendRequest(req, Options);
                if (res is SearchResponse s && s.Any()) {
                    _logger.LogDebug("Successfully retrieved {entry}.",
                        s.Entries[0].DistinguishedName);
                    _mapper.Assign(s.Entries[0], connection, retval);
                    _claimsBuilder.AddClaims(retval);
                    return retval;
                }
            }

            // Not found ad this point.
            _logger.LogError(Properties.Resources.ErrorUserNotFound,
                username);
            return null;
        }

        /// <inheritdoc />
        public async Task<TUser> LoginAsync(string username,
                string password) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = _connectionService.Connect(
                username ?? string.Empty,
                password ?? string.Empty);

            var retval = new TUser();

            foreach (var b in Options.SearchBases) {
                var req = GetRequest(username, b);
                var res = await connection.SendRequestAsync(req, Options)
                    .ConfigureAwait(false);

                if (res is SearchResponse s && s.Any()) {
                    _mapper.Assign(s.Entries[0], connection, retval);
                    _claimsBuilder.AddClaims(retval);
                    return retval;
                }
            }

            // Not found ad this point.
            _logger.LogError(Properties.Resources.ErrorUserNotFound,
                username);
            return null;
        }
        #endregion

        #region Private properties
        private LdapOptions Options => _connectionService.Options;
        #endregion

        #region Private methods
        private SearchRequest GetRequest(string username,
                string searchBase,
                SearchScope scope) {
            Debug.Assert(searchBase != null);
            var filter = string.Format(Options.Mapping.UserFilter,
                username);
            var retval = new SearchRequest(searchBase,
                filter,
                scope,
                _mapper.RequiredUserAttributes.ToArray());
            _logger.LogDebug("Requesting {filter} in search base {base} "
                    + "with search scope {scope}.",
                    filter, searchBase, scope);
            return retval;
        }

        private SearchRequest GetRequest(string username,
                KeyValuePair<string, SearchScope> searchBase)
            => GetRequest(username, searchBase.Key,
                searchBase.Value);
        #endregion

        #region Private fields
        private readonly IClaimsBuilder<TUser, TGroup> _claimsBuilder;
        private readonly ILdapConnectionService _connectionService;
        private readonly ILogger _logger;
        private readonly ILdapMapper<TUser, TGroup> _mapper;
        #endregion
    }
}
