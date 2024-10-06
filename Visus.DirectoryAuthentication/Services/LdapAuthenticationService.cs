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
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Extensions;
using Visus.DirectoryAuthentication.Properties;
using Visus.Ldap;
using Visus.Ldap.Claims;
using Visus.Ldap.Extensions;
using Visus.Ldap.Mapping;
using Visus.Ldap.Services;


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
        /// <param name="claimsMapper"></param>
        /// <param name="logger">A logger for presisting important messages like
        /// login failures.</param>
        /// <exception cref="ArgumentNullException">If any of the parameters is
        /// <c>null</c>.</exception>
        public LdapAuthenticationService(IOptions<LdapOptions> options,
                ILdapConnectionService connectionService,
                ILdapMapper<SearchResultEntry, TUser, TGroup> mapper,
                IClaimsBuilder<TUser, TGroup> claimsBuilder,
                IClaimsMapper<SearchResultEntry> claimsMapper,
                ILogger<LdapAuthenticationService<TUser, TGroup>> logger) {
            this._claimsBuilder = claimsBuilder
                ?? throw new ArgumentNullException(nameof(claimsBuilder));
            this._claimsMapper = claimsMapper
                ?? throw new ArgumentNullException(nameof(claimsMapper));
            this._connectionService = connectionService
                ?? throw new ArgumentNullException(nameof(connectionService));
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
            this._mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));

            Debug.Assert(this._options.Mapping != null);
            this._groupClaimAttributes = this._claimsMapper.RequiredGroupAttributes
                .Append(this._options.Mapping.PrimaryGroupAttribute)
                .Append(this._options.Mapping.GroupsAttribute)
                .ToArray();
            this._userAttributes = this._mapper.RequiredUserAttributes
                .Append(this._options.Mapping.PrimaryGroupAttribute)
                .Append(this._options.Mapping.GroupsAttribute)
                .ToArray();
            this._userClaimAttributes = this._claimsMapper.RequiredUserAttributes
                .Append(this._options.Mapping.PrimaryGroupAttribute)
                .Append(this._options.Mapping.GroupsAttribute)
                .ToArray();
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public ClaimsPrincipal LoginPrincipal(string username,
                string password,
                string? authenticationType,
                string? nameType,
                string? roleType,
                ClaimFilter? filter) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._connectionService.Connect(
                username ?? string.Empty,
                password ?? string.Empty);

            foreach (var b in this._options.SearchBases) {
                var req = new SearchRequest(b.Key,
                    this.GetUserFilter(username),
                    b.Value,
                    this._userClaimAttributes);
                var res = connection.SendRequest(req, this._options);

                if (res is SearchResponse s && s.Any()) {
                    var user = s.Entries[0];
                    var primary = user.GetPrimaryGroup(connection,
                        this.Cache,
                        this._groupClaimAttributes,
                        this._options);
                    // TODO: this is not recursive!
                    var groups = user.GetGroups(connection,
                        this.Cache,
                        this._groupClaimAttributes,
                        this._options);

                    var claims = this._claimsMapper.GetClaims(user,
                        primary,
                        groups,
                        filter).Distinct(ClaimEqualityComparer.Instance);
                    var identity = new ClaimsIdentity(claims,
                        authenticationType ?? this.GetType().Name,
                        nameType,
                        roleType);
                    return new ClaimsPrincipal(identity);
                }
            }

            // Not found ad this point.
            this._logger.LogError(Resources.ErrorUserNotFoundDetailed,
                username);
            throw new KeyNotFoundException(Resources.ErrorUserNotFound);
        }

        /// <inheritdoc />
        public async Task<ClaimsPrincipal> LoginPrincipalAsync(string username,
                string password,
                string? authenticationType,
                string? nameType,
                string? roleType,
                ClaimFilter? filter,
                CancellationToken cancellationToken) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._connectionService.Connect(
                username ?? string.Empty,
                password ?? string.Empty);

            foreach (var b in this._options.SearchBases) {
                cancellationToken.ThrowIfCancellationRequested();
                var req = new SearchRequest(b.Key,
                    this.GetUserFilter(username),
                    b.Value,
                    this._userClaimAttributes);
                var res = await connection.SendRequestAsync(req, this._options)
                    .ConfigureAwait(false);

                if (res is SearchResponse s && s.Any()) {
                    var user = s.Entries[0];
                    var primary = user.GetPrimaryGroup(connection,
                        this.Cache,
                        this._groupClaimAttributes,
                        this._options);
                    // TODO: this is not recursive!
                    var groups = user.GetGroups(connection,
                        this.Cache,
                        this._groupClaimAttributes,
                        this._options);

                    var claims = this._claimsMapper.GetClaims(user,
                        primary,
                        groups,
                        filter).Distinct(ClaimEqualityComparer.Instance);
                    var identity = new ClaimsIdentity(claims,
                        authenticationType ?? this.GetType().Name,
                        nameType,
                        roleType);
                    return new ClaimsPrincipal(identity);
                }
            }

            // Not found ad this point.
            this._logger.LogError(Resources.ErrorUserNotFoundDetailed,
                username);
            throw new KeyNotFoundException(Resources.ErrorUserNotFound);
        }

        /// <inheritdoc />
        public TUser LoginUser(string username, string password) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._connectionService.Connect(
                username ?? string.Empty,
                password ?? string.Empty);

            var retval = new TUser();

            foreach (var b in this._options.SearchBases) {
                var req = new SearchRequest(b.Key,
                    this.GetUserFilter(username),
                    b.Value,
                    this._userAttributes);
                var res = connection.SendRequest(req, this._options);
                if (res is SearchResponse s && s.Any()) {
                    this._mapper.MapUser(s.Entries[0], retval);
                    var groups = s.Entries[0].GetGroups(connection,
                        this.Cache,
                        this._mapper,
                        this._options);
                    this._mapper.SetGroups(retval, groups);
                    return retval;
                }
            }

            // Not found ad this point.
            this._logger.LogError(Resources.ErrorUserNotFoundDetailed,
                username);
            throw new KeyNotFoundException(Resources.ErrorUserNotFound);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (TUser, IEnumerable<Claim>) LoginUser(string username,
                string password, ClaimFilter? filter) {
            var user = this.LoginUser(username, password);
            var claims = this._claimsBuilder.GetClaims(user, filter)
                .Distinct(ClaimEqualityComparer.Instance);
            return (user, claims);
        }

        /// <inheritdoc />
        public async Task<TUser> LoginUserAsync(string username,
                string password, CancellationToken cancellationToken) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._connectionService.Connect(
                username ?? string.Empty,
                password ?? string.Empty);

            var retval = new TUser();

            foreach (var b in this._options.SearchBases) {
                cancellationToken.ThrowIfCancellationRequested();
                var req = new SearchRequest(b.Key,
                    this.GetUserFilter(username),
                    b.Value,
                    this._userAttributes);
                var res = await connection
                    .SendRequestAsync(req, this._options)
                    .ConfigureAwait(false);

                if (res is SearchResponse s && s.Any()) {
                    this._mapper.MapUser(s.Entries[0], retval);
                    var groups = s.Entries[0].GetGroups(connection,
                        this.Cache,
                        this._mapper,
                        this._options);
                    this._mapper.SetGroups(retval, groups);
                    return retval;
                }
            }

            // Not found at this point, although authentication succeeded.
            this._logger.LogError(Resources.ErrorUserNotFoundDetailed,
                username);
            throw new KeyNotFoundException(Resources.ErrorUserNotFound);
        }

        /// <inheritdoc />
        public async Task<(TUser, IEnumerable<Claim>)> LoginUserAsync(
                string username,
                string password,
                ClaimFilter? filter,
                CancellationToken cancellationToken) {
            var user = await this.LoginUserAsync(username, password,
                cancellationToken);
            var claims = this._claimsBuilder.GetClaims(user, filter)
                .Distinct(ClaimEqualityComparer.Instance);
            return (user, claims);
        }
        #endregion

        #region Private properties
        private NoCacheService<SearchResultEntry> Cache
            => NoCacheService<SearchResultEntry>.Default;
        #endregion

        #region Private methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetUserFilter(string? username) {
            Debug.Assert(this._options.Mapping != null);
            return string.Format(this._options.Mapping.UserFilter,
                (username ?? string.Empty).EscapeLdapFilterExpression());
        }
        #endregion

        #region Private fields
        private readonly IClaimsBuilder<TUser, TGroup> _claimsBuilder;
        private readonly IClaimsMapper<SearchResultEntry> _claimsMapper;
        private readonly ILdapConnectionService _connectionService;
        private readonly string[] _groupClaimAttributes;
        private readonly ILogger _logger;
        private readonly LdapOptions _options;
        private readonly ILdapMapper<SearchResultEntry, TUser, TGroup> _mapper;
        private readonly string[] _userAttributes;
        private readonly string[] _userClaimAttributes;
        #endregion
    }
}
