// <copyright file="LdapUserStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication;
using Visus.DirectoryIdentity.Properties;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryIdentity.Stores {

    /// <summary>
    /// The implementation of an LDAP user store using the default
    /// <see cref="IdentityUser"/>.
    /// </summary>
    /// <typeparam name="TUserKey">The type used for the primary key for the
    /// user.</typeparam>
    public class IdentityLdapStore<TUserKey, TRoleKey>
            : LdapStore<IdentityUser<TUserKey>, IdentityRole<TRoleKey>>,
            IUserClaimStore<IdentityUser<TUserKey>>,
            IUserEmailStore<IdentityUser<TUserKey>>,
            IUserLockoutStore<IdentityUser<TUserKey>>,
            IUserPhoneNumberStore<IdentityUser<TUserKey>>
            where TUserKey : IEquatable<TUserKey>
            where TRoleKey : IEquatable<TRoleKey> {

        #region Public constructors
        public IdentityLdapStore(
                ILdapSearchService<IdentityUser<TUserKey>,
                    IdentityRole<TRoleKey>> searchService,
                IClaimsBuilder<IdentityUser<TUserKey>,
                    IdentityRole<TRoleKey>> claimsBuilder,
                ILdapAttributeMap<IdentityUser<TUserKey>> userMap,
                IGroupClaimsMap groupClaims,
                ILogger<IdentityLdapStore<TUserKey, TRoleKey>> logger) {
            this._claimsBuilder = claimsBuilder
                ?? throw new ArgumentNullException(nameof(claimsBuilder));
            this._groupClaims = groupClaims
                ?? throw new ArgumentNullException(nameof(groupClaims));
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._searchService = searchService
                ?? throw new ArgumentNullException(nameof(searchService));
            this._userMap = userMap
                ?? throw new ArgumentNullException(nameof(userMap));

            {
                var prop = typeof(IdentityUser).GetProperty(
                    nameof(IdentityUser.Email));
                Debug.Assert(prop != null);
                this._emailAttribute = this._userMap[prop]!;
                Debug.Assert(this._emailAttribute != null);
            }
        }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public IQueryable<IdentityUser<TUserKey>> Users
            => this._searchService.GetUsers().AsQueryable();
        #endregion

        #region Public methods
        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="claims"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task AddClaimsAsync(
                IdentityUser<TUserKey> user,
                IEnumerable<Claim> claims,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<IdentityResult> CreateAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<IdentityResult> DeleteAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <inheritdoc />
        public void Dispose() {
            this._searchService?.Dispose();
        }

        /// <inheritdoc />
        public async Task<IdentityUser<TUserKey>?> FindByEmailAsync(
                string normalisedEmail,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(normalisedEmail,
                nameof(normalisedEmail));
            var filter = $"({this._emailAttribute.Name}={normalisedEmail})";
            var results = await this._searchService.GetUsersAsync(filter,
                cancellationToken);
            return results.SingleOrDefault();
        }

        /// <inheritdoc />
        public Task<IdentityUser<TUserKey>?> FindByIdAsync(
                string userID,
                CancellationToken cancellationToken) {
            return this._searchService.GetUserByIdentityAsync(userID);
        }

        /// <inheritdoc />
        public Task<IdentityUser<TUserKey>?> FindByNameAsync(
                string normalisedUserName,
                CancellationToken cancellationToken) {
            return this._searchService.GetUserByAccountNameAsync(
                normalisedUserName);
        }

        /// <inheritdoc />
        public Task<int> GetAccessFailedCountAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <inheritdoc />
        public Task<IList<Claim>> GetClaimsAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            var retval = this._claimsBuilder.GetClaims(user);
            throw new NotImplementedException("TODO: How do we find the groups?");
        }

        /// <inheritdoc />
        public Task<string?> GetEmailAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.Email);
        }

        /// <inheritdoc />
        public Task<bool> GetEmailConfirmedAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            // Note: we do not allow for editing, so any e-mail address is
            // always considered to be confirmed.
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> GetLockoutEnabledAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.LockoutEnabled);
        }

        /// <inheritdoc />
        public Task<DateTimeOffset?> GetLockoutEndDateAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.LockoutEnd);
        }

        /// <inheritdoc />
        public Task<string?> GetNormalizedEmailAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.Email?.ToLowerInvariant());
        }

        /// <inheritdoc />
        public Task<string?> GetNormalizedUserNameAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.UserName?.ToLowerInvariant());
        }

        /// <inheritdoc />
        public Task<string?> GetPhoneNumberAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.PhoneNumber);
        }

        /// <inheritdoc />
        public Task<bool> GetPhoneNumberConfirmedAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            // Note: we do not allow for editing, so any phone number is always
            // considered to be confirmed.
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<string> GetUserIdAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.Id.ToString()!);
        }

        /// <inheritdoc />
        public Task<string?> GetUserNameAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user?.UserName);
        }

        /// <inheritdoc />
        public Task<IList<IdentityUser<TUserKey>>> GetUsersForClaimAsync(
                Claim claim,
                CancellationToken cancellationToken) {
            throw new NotImplementedException("TODO");
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<int> IncrementAccessFailedCountAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="claims"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task RemoveClaimsAsync(
                IdentityUser<TUserKey> user,
                IEnumerable<Claim> claims,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="claim"></param>
        /// <param name="newClaim"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task ReplaceClaimAsync(
                IdentityUser<TUserKey> user,
                Claim claim,
                Claim newClaim,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task ResetAccessFailedCountAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="email"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task SetEmailAsync(
                IdentityUser<TUserKey> user,
                string? email,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task SetEmailConfirmedAsync(
                IdentityUser<TUserKey> user,
                bool confirmed,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task SetLockoutEnabledAsync(
                IdentityUser<TUserKey> user,
                bool enabled,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="lockoutEnd"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task SetLockoutEndDateAsync(
                IdentityUser<TUserKey> user,
                DateTimeOffset? lockoutEnd,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="normalisedEmail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task SetNormalizedEmailAsync(
                IdentityUser<TUserKey> user,
                string? normalisedEmail,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="normalisedName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task SetNormalizedUserNameAsync(
                IdentityUser<TUserKey> user,
                string? normalisedName,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task SetPhoneNumberAsync(
                IdentityUser<TUserKey> user,
                string? phoneNumber,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task SetPhoneNumberConfirmedAsync(
                IdentityUser<TUserKey> user,
                bool confirmed,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task SetUserNameAsync(
                IdentityUser<TUserKey> user,
                string? userName,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<IdentityResult> UpdateAsync(
                IdentityUser<TUserKey> user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }
        #endregion

        #region Private fields
        private readonly LdapAttributeAttribute _emailAttribute;
        private readonly IClaimsBuilder<IdentityUser<TUserKey>,
            IdentityRole<TRoleKey>> _claimsBuilder;
        private readonly IGroupClaimsMap _groupClaims;
        private readonly ILogger _logger;
        private readonly ILdapSearchService<IdentityUser<TUserKey>,
            IdentityRole<TRoleKey>> _searchService;
        private readonly ILdapAttributeMap<IdentityUser<TUserKey>> _userMap;
        #endregion
    }
}
