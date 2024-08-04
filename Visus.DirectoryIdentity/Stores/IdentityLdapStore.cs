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
    /// <typeparam name="TKey">The type used for the primary key for the
    /// user.</typeparam>
    public class IdentityLdapStore<TKey>
            : IQueryableUserStore<IdentityUser<TKey>>,
            IUserClaimStore<IdentityUser<TKey>>,
            IUserEmailStore<IdentityUser<TKey>>,
            IUserLockoutStore<IdentityUser<TKey>>,
            IUserPhoneNumberStore<IdentityUser<TKey>>
            where TKey : IEquatable<TKey> {

        #region Public constructors
        public IdentityLdapStore(
                ILdapSearchService<IdentityUser<TKey>, IdentityRole<TKey>> searchService,
                IClaimsBuilder<IdentityUser<TKey>, IdentityRole<TKey>> claimsBuilder,
                ILdapAttributeMap<IdentityUser<TKey>> userMap,
                IGroupClaimsMap groupClaims,
                ILogger<IdentityLdapStore<TKey>> logger) {
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
        public IQueryable<IdentityUser<TKey>> Users
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }

        /// <inheritdoc />
        public void Dispose() {
            this._searchService?.Dispose();
        }

        /// <inheritdoc />
        public async Task<IdentityUser<TKey>?> FindByEmailAsync(
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
        public Task<IdentityUser<TKey>?> FindByIdAsync(
                string userID,
                CancellationToken cancellationToken) {
            return this._searchService.GetUserByIdentityAsync(userID);
        }

        /// <inheritdoc />
        public Task<IdentityUser<TKey>?> FindByNameAsync(
                string normalisedUserName,
                CancellationToken cancellationToken) {
            return this._searchService.GetUserByAccountNameAsync(
                normalisedUserName);
        }

        /// <inheritdoc />
        public Task<int> GetAccessFailedCountAsync(
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <inheritdoc />
        public Task<IList<Claim>> GetClaimsAsync(
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            var retval = this._claimsBuilder.GetClaims(user);
            throw new NotImplementedException("TODO: How do we find the groups?");
        }

        /// <inheritdoc />
        public Task<string?> GetEmailAsync(
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.Email);
        }

        /// <inheritdoc />
        public Task<bool> GetEmailConfirmedAsync(
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            // Note: we do not allow for editing, so any e-mail address is
            // always considered to be confirmed.
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> GetLockoutEnabledAsync(
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.LockoutEnabled);
        }

        /// <inheritdoc />
        public Task<DateTimeOffset?> GetLockoutEndDateAsync(
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.LockoutEnd);
        }

        /// <inheritdoc />
        public Task<string?> GetNormalizedEmailAsync(
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.Email?.ToLowerInvariant());
        }

        /// <inheritdoc />
        public Task<string?> GetNormalizedUserNameAsync(
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.UserName?.ToLowerInvariant());
        }

        /// <inheritdoc />
        public Task<string?> GetPhoneNumberAsync(
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.PhoneNumber);
        }

        /// <inheritdoc />
        public Task<bool> GetPhoneNumberConfirmedAsync(
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            // Note: we do not allow for editing, so any phone number is always
            // considered to be confirmed.
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<string> GetUserIdAsync(
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.Id.ToString()!);
        }

        /// <inheritdoc />
        public Task<string?> GetUserNameAsync(
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user?.UserName);
        }

        /// <inheritdoc />
        public Task<IList<IdentityUser<TKey>>> GetUsersForClaimAsync(
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
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
                IdentityUser<TKey> user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }
        #endregion

        #region Private fields
        private readonly LdapAttributeAttribute _emailAttribute;
        private readonly IClaimsBuilder<IdentityUser<TKey>,
            IdentityRole<TKey>> _claimsBuilder;
        private readonly IGroupClaimsMap _groupClaims;
        private readonly ILogger _logger;
        private readonly ILdapSearchService<IdentityUser<TKey>,
            IdentityRole<TKey>> _searchService;
        private readonly ILdapAttributeMap<IdentityUser<TKey>> _userMap;
        #endregion
    }
}
