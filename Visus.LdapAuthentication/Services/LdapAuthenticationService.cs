// <copyright file="LdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Visus.Ldap;
using Visus.Ldap.Claims;
using Visus.Ldap.Extensions;
using Visus.Ldap.Mapping;
using Visus.LdapAuthentication.Claims;
using Visus.LdapAuthentication.Configuration;
using Visus.LdapAuthentication.Extensions;
using Visus.LdapAuthentication.Properties;


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
                IClaimsMapper<LdapEntry> claimsMapper,
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
                var res = connection.Search(b,
                    this.GetUserFilter(username),
                    this._userClaimAttributes);
                var user = res.NextEntry(this._logger);

                if (user != null) {
                    var primary = user.GetPrimaryGroup(connection,
                        this._groupClaimAttributes,
                        this._options);
                    var groups = user.GetGroups(connection,
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
                ClaimFilter? filter) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._connectionService.Connect(
                username ?? string.Empty,
                password ?? string.Empty);

            foreach (var b in this._options.SearchBases) {
                var res = await connection.SearchAsync(b,
                    this.GetUserFilter(username),
                    this._userClaimAttributes,
                    this._options.PollingInterval)
                    .ConfigureAwait(false);

                if (res.Any()) {
                    var user = res.First();
                    var primary = user.GetPrimaryGroup(connection,
                        this._groupClaimAttributes,
                        this._options);
                    var groups = user.GetGroups(connection,
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

            var filter = this.GetUserFilter(username);
            var retval = new TUser();

            foreach (var b in this._options.SearchBases) {
                var res = connection.Search(b, filter, this._userAttributes);
                var entry = res.NextEntry(this._logger);
                if (entry != null) {
                    this._mapper.MapUser(entry, retval);
                    var groups = entry.GetGroups(connection,
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
                string password) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._connectionService.Connect(
                username ?? string.Empty,
                password ?? string.Empty);

            var filter = this.GetUserFilter(username);
            var retval = new TUser();

            foreach (var b in this._options.SearchBases) {
                var res = await connection.SearchAsync(b, filter,
                    this._userAttributes, this._options.PollingInterval)
                    .ConfigureAwait(false);
                var entry = res.FirstOrDefault();
                if (entry != null) {
                    this._mapper.MapUser(entry, retval);
                    var groups = entry.GetGroups(connection,
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
                ClaimFilter? filter) {
            var user = await this.LoginUserAsync(username, password);
            var claims = this._claimsBuilder.GetClaims(user, filter)
                .Distinct(ClaimEqualityComparer.Instance);
            return (user, claims);
        }
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
        private readonly IClaimsMapper<LdapEntry> _claimsMapper;
        private readonly ILdapConnectionService _connectionService;
        private readonly string[] _groupClaimAttributes;
        private readonly ILogger _logger;
        private readonly LdapOptions _options;
        private readonly ILdapMapper<LdapEntry, TUser, TGroup> _mapper;
        private readonly string[] _userAttributes;
        private readonly string[] _userClaimAttributes;
        #endregion
    }
}
