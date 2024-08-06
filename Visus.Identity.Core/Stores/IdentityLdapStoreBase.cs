// <copyright file="LdapUserStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Visus.Ldap;
using Visus.Ldap.Claims;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;


namespace Visus.Identity.Stores {

    /// <summary>
    /// The implementation of an LDAP user store using the default
    /// <see cref="IdentityUser"/>.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TUserKey">The type used for the primary key for the
    /// users.</typeparam>
    /// <typeparam name="TRole"></typeparam>
    /// <typeparam name="TRoleKey">The type used for the primray key for the
    /// roles.</typeparam>
    /// <typeparam name="TSearchScope">The type used to specify the search
    /// scipe in the directory.</typeparam>
    public class IdentityLdapStoreBase<TUser, TUserKey,
            TRole, TRoleKey, TSearchScope, TOptions>
        : LdapStoreBase<TUser, TRole, TSearchScope, TOptions>,
            IUserEmailStore<TUser>,
            IUserLockoutStore<TUser>,
            IUserPhoneNumberStore<TUser>
            where TUser : IdentityUser<TUserKey>, new()
            where TRole : IdentityRole<TRoleKey>, new()
            where TUserKey : IEquatable<TUserKey>
            where TRoleKey : IEquatable<TRoleKey>
            where TSearchScope : struct, Enum
            where TOptions : LdapOptionsBase {

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
            return Task.FromResult(user.UserName);
        }

        /// <inheritdoc />
        public Task<int> IncrementAccessFailedCountAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            return Task.FromResult(++user.AccessFailedCount);
        }

        /// <inheritdoc />
        public Task ResetAccessFailedCountAsync(
                TUser user,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetEmailAsync(
                TUser user,
                string? email,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            user.Email = email;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetEmailConfirmedAsync(
                TUser user,
                bool confirmed,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetLockoutEnabledAsync(
                TUser user,
                bool enabled,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetLockoutEndDateAsync(
                TUser user,
                DateTimeOffset? lockoutEnd,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            user.LockoutEnd = lockoutEnd;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetNormalizedEmailAsync(
                TUser user,
                string? normalisedEmail,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            user.NormalizedEmail = normalisedEmail;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetPhoneNumberAsync(
                TUser user,
                string? phoneNumber,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            user.PhoneNumber = phoneNumber;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task SetPhoneNumberConfirmedAsync(
                TUser user,
                bool confirmed,
                CancellationToken cancellationToken) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));
            user.PhoneNumberConfirmed = confirmed;
            return Task.CompletedTask;
        }
        #endregion

        #region Protected constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="searchService"></param>
        /// <param name="ldapOptions"></param>
        /// <param name="userMap"></param>
        /// <param name="roleMap"></param>
        /// <param name="claimsBuilder"></param>
        /// <param name="userClaims"></param>
        /// <param name="roleClaims"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException">If any of the parameters
        /// is <c>null</c>.</exception>
        protected IdentityLdapStoreBase(
                ILdapSearchServiceBase<TUser, TRole, TSearchScope> searchService,
                IOptions<TOptions> ldapOptions,
                ILdapAttributeMap<TUser> userMap,
                ILdapAttributeMap<TRole> roleMap,
                IClaimsBuilder<TUser, TRole> claimsBuilder,
                IUserClaimsMap userClaims,
                IGroupClaimsMap roleClaims,
                ILogger logger)
            : base(searchService,
                  ldapOptions,
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

        #region Private fields
        private readonly LdapAttributeAttribute? _emailAttribute;
        #endregion
    }
}
