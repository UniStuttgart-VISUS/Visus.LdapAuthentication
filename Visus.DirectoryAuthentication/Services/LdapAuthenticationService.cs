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
using System.Security.AccessControl;
using System.Security.Claims;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Extensions;
using Visus.Ldap;
using Visus.Ldap.Claims;
using Visus.Ldap.Extensions;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Services {

    /// <summary>
    /// Implements an <see cref="ILdapAuthenticationService{TUser}"/> using
    /// <see cref="System.DirectoryServices.Protocols"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of the user object to be returned on
    /// login.</typeparam>
    /// <typeparam name="TGroup">The type used to represent the groups the user
    /// might be member of.</typeparam>
    public sealed class LdapAuthenticationService<TUser, TGroup>
            : ILdapAuthenticationService<TUser>
            where TUser : class, new()
            where TGroup : class, new() {

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
                ILdapMapper<SearchResultEntry, TUser, TGroup> mapper,
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

        #region Public methods
        /// <inheritdoc />
        public ClaimsPrincipal? LoginPrincipal(string username,
                string password,
                string? authenticationType,
                string? nameType,
                string? roleType,
                ClaimFilter? filter) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var user = this.LoginUser(username, password);

            // TODO: use direct mapper instead.
            if (user != null) {
                var claims = this._claimsBuilder.GetClaims(user, filter);
                var identity = new ClaimsIdentity(claims,
                    authenticationType, nameType, roleType);
                return new ClaimsPrincipal(identity);
            } else {
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<ClaimsPrincipal?> LoginPrincipalAsync(string username,
                string password,
                string? authenticationType,
                string? nameType,
                string? roleType,
                ClaimFilter? filter) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var user = await this.LoginUserAsync(username, password);

            // TODO: use direct mapper instead.
            if (user != null) {
                var claims = this._claimsBuilder.GetClaims(user, filter);
                var identity = new ClaimsIdentity(claims,
                    authenticationType, nameType, roleType);
                return new ClaimsPrincipal(identity);
            } else {
                return null;
            }
        }

        /// <inheritdoc />
        public TUser? LoginUser(string username, string password) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._connectionService.Connect(
                username ?? string.Empty,
                password ?? string.Empty);

            var retval = new TUser();

            foreach (var b in this._options.SearchBases) {
                var req = this.GetRequest(username ?? string.Empty, b);
                var res = connection.SendRequest(req, this._options);
                if (res is SearchResponse s && s.Any()) {
                    this._mapper.MapUser(s.Entries[0], retval);
                    var groups = s.Entries[0].GetGroups(connection,
                        this._mapper,
                        this._options);
                    this._mapper.SetGroups(retval, groups);
                    return retval;
                }
            }

            // Not found ad this point.
            this._logger.LogError(Properties.Resources.ErrorUserNotFound,
                username);
            return null;
        }

        /// <inheritdoc />
        public async Task<TUser?> LoginUserAsync(string username,
                string password) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._connectionService.Connect(
                username ?? string.Empty,
                password ?? string.Empty);

            var retval = new TUser();

            foreach (var b in this._options.SearchBases) {
                var req = this.GetRequest(username ?? string.Empty, b);
                var res = await connection.SendRequestAsync(req, this._options)
                    .ConfigureAwait(false);

                if (res is SearchResponse s && s.Any()) {
                    this._mapper.MapUser(s.Entries[0], retval);
                    var groups = s.Entries[0].GetGroups(connection,
                        this._mapper,
                        this._options);
                    this._mapper.SetGroups(retval, groups);
                    return retval;
                }
            }

            // Not found at this point, although authentication succeeded.
            {
                var msg = Properties.Resources.ErrorUserNotFound;
                msg = string.Format(msg, username);
                throw new KeyNotFoundException(msg);
            }
        }
        #endregion

        #region Private methods
        private SearchRequest GetRequest(string username,
                string searchBase,
                SearchScope scope) {
            Debug.Assert(searchBase != null);
            Debug.Assert(this._options.Mapping != null);
            var filter = string.Format(this._options.Mapping.UserFilter,
                username.EscapeLdapFilterExpression());
            var attributes = this._mapper.RequiredUserAttributes
                .Append(this._options.Mapping.PrimaryGroupAttribute)
                .Append(this._options.Mapping.GroupsAttribute)
                .ToArray();
            var retval = new SearchRequest(searchBase,
                filter,
                scope,
                attributes);
            this._logger.LogDebug("Requesting {filter} in search base {base} "
                    + "with search scope {scope}.",
                    filter, searchBase, scope);
            return retval;
        }

        private SearchRequest GetRequest(string username,
                KeyValuePair<string, SearchScope> searchBase)
            => this.GetRequest(username, searchBase.Key,
                searchBase.Value);
        #endregion

        #region Private fields
        private readonly IClaimsBuilder<TUser, TGroup> _claimsBuilder;
        private readonly ILdapConnectionService _connectionService;
        private readonly ILogger _logger;
        private readonly LdapOptions _options;
        private readonly ILdapMapper<SearchResultEntry, TUser, TGroup> _mapper;
        #endregion
    }
}
