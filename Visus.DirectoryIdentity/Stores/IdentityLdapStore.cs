// <copyright file="LdapUserStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
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
    /// users.</typeparam>
    /// <typeparam name="TRoleKey">The type used for the primray key for the
    /// roles.</typeparam>
    public class IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>
            : LdapStore<TUser, TRole>,
            IUserEmailStore<TUser>,
            IUserLockoutStore<TUser>,
            IUserPhoneNumberStore<TUser>
            where TUser : IdentityUser<TUserKey>, new()
            where TRole : IdentityRole<TRoleKey>, new()
            where TUserKey : IEquatable<TUserKey>
            where TRoleKey : IEquatable<TRoleKey> {

        #region Public constructors
        public IdentityLdapStore(
                ILdapSearchService<TUser, TRole> searchService,
                ILdapAttributeMap<TUser> userMap,
                ILdapAttributeMap<TRole> roleMap,
                IClaimsBuilder<TUser, TRole> claimsBuilder,
                IUserClaimsMap userClaims,
                IGroupClaimsMap roleClaims,
                ILogger<IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>> logger)
            : base(searchService,
                  userMap,
                  roleMap,
                  claimsBuilder,
                  userClaims,
                  roleClaims,
                  logger) {
            Debug.Assert(this.UserMap != null);

            {
                var prop = typeof(IdentityUser).GetProperty(
                    nameof(IdentityUser.Email));
                Debug.Assert(prop != null);
                this._emailAttribute = this.UserMap[prop];
            }
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public async Task<TUser?> FindByEmailAsync(
                string normalisedEmail,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(normalisedEmail,
                nameof(normalisedEmail));
            if (this._emailAttribute == null) {
                return null;
            }

            var filter = $"({this._emailAttribute.Name}={normalisedEmail})";
            var results = await this.SearchService
                .GetUsersAsync(filter, cancellationToken)
                .ConfigureAwait(false);
            return results.SingleOrDefault();
        }

        /// <inheritdoc />
        public Task<int> GetAccessFailedCountAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <inheritdoc />
        public Task<string?> GetEmailAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.Email);
        }

        /// <inheritdoc />
        public Task<bool> GetEmailConfirmedAsync(
                TUser user,
                CancellationToken cancellationToken) {
            // Note: we do not allow for editing, so any e-mail address is
            // always considered to be confirmed.
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> GetLockoutEnabledAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.LockoutEnabled);
        }

        /// <inheritdoc />
        public Task<DateTimeOffset?> GetLockoutEndDateAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.LockoutEnd);
        }

        /// <inheritdoc />
        public Task<string?> GetNormalizedEmailAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.Email?.ToLowerInvariant());
        }

        /// <inheritdoc />
        public override Task<string?> GetNormalizedUserNameAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.UserName?.ToLowerInvariant());
        }

        /// <inheritdoc />
        public Task<string?> GetPhoneNumberAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.PhoneNumber);
        }

        /// <inheritdoc />
        public Task<bool> GetPhoneNumberConfirmedAsync(
                TUser user,
                CancellationToken cancellationToken) {
            // Note: we do not allow for editing, so any phone number is always
            // considered to be confirmed.
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public override Task<string> GetUserIdAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user.Id.ToString()!);
        }

        /// <inheritdoc />
        public override Task<string?> GetUserNameAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(user?.UserName);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<int> IncrementAccessFailedCountAsync(
                TUser user,
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
                TUser user,
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
                TUser user,
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
                TUser user,
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
                TUser user,
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
                TUser user,
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
                TUser user,
                string? normalisedEmail,
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
                TUser user,
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
                TUser user,
                bool confirmed,
                CancellationToken cancellationToken) {
            throw new NotImplementedException(Resources.ErrorReadOnlyStore);
        }
        #endregion

        #region Private fields
        private readonly LdapAttributeAttribute? _emailAttribute;
        #endregion
    }
}
