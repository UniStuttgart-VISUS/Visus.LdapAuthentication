// <copyright file="LdapUserStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// An ASP.NET Core Identity store that retrieves its information from LDAP.
    /// </summary>
    /// <typeparam name="TUser">The type of the user object to be retrieved
    /// from the directory.</typeparam>
    public class LdapUserStore<TUser> : IQueryableUserStore<TUser>,
            IUserClaimStore<TUser>,
            IUserEmailStore<TUser>,
            IUserLockoutStore<TUser>,
            IUserPasswordStore<TUser>,
            IUserPhoneNumberStore<TUser>
            where TUser : class, ILdapIdentityUser {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="authService"></param>
        /// <param name="searchService"></param>
        public LdapUserStore(ILdapAuthenticationService<TUser> authService,
                ILdapSearchService<TUser> searchService) {
            this._authService = authService
                ?? throw new ArgumentNullException(nameof(authService));
            this._searchService = searchService
                ?? throw new ArgumentNullException(nameof(searchService));

            this._hasher = new LdapPasswordHasher<TUser>(this._authService);

            {
                var prop = typeof(TUser).GetProperty(
                    nameof(ILdapIdentityUser.AccountName));
                //prop.Get
                //TODO: need to know schema here
                this._accountAttribute = "sAMAccountName";
            }
        }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public IQueryable<TUser> Users
            => this._searchService.GetUsers().AsQueryable();
        #endregion

        public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<IdentityResult> CreateAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose() {
            if (this._searchService != null) {
                this._searchService.Dispose();
                this._searchService = null;
            }
        }

        public Task<TUser> FindByEmailAsync(string normalizedEmail,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the user based on the <see cref="ILdapUser.Identity"/>
        /// attribute of the specified user type.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<TUser> FindByIdAsync(string userId,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            this.CheckNotDisposed();
            return this._searchService.GetUserByIdentityAsync(userId);
        }

        /// <summary>
        /// Gets the user based on the <see cref="ILdapUser.AccountName"/>
        /// attribute of the specified user type.
        /// </summary>
        /// <param name="normalisedUserName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TUser> FindByNameAsync(string normalisedUserName,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            _ = normalisedUserName
                ?? throw new ArgumentNullException(normalisedUserName);
            this.CheckNotDisposed();

            return (await this._searchService.GetUsersAsync(
                $"({this._accountAttribute}={normalisedUserName})"))
                .SingleOrDefault();
        }

        public Task<int> GetAccessFailedCountAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<IList<Claim>> GetClaimsAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<string> GetEmailAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            this.CheckNotDisposed();

            if (!string.IsNullOrWhiteSpace(user.EmailAddress)) {
                return user.EmailAddress;

            } else {
                var u = await this._searchService.GetUserByIdentityAsync(
                    user.Identity);
                return u.EmailAddress;
            }
        }

        /// <summary>
        /// Answer whether the e-mail address has been confirmed, which is
        /// always <c>true</c> for an LDAP directory that is managed by an
        /// administrator and has no self service features.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns><c>true</c>, unconditionally.</returns>
        public Task<bool> GetEmailConfirmedAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(true);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedEmailAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(this._hasher.HashUser(user));
        }

        public Task<string> GetPhoneNumberAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<IList<TUser>> GetUsersForClaimAsync(Claim claim,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<bool> HasPasswordAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task RemoveClaimsAsync(TUser user,
                IEnumerable<Claim> claims,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task ReplaceClaimAsync(TUser user,
                Claim claim,
                Claim newClaim,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task ResetAccessFailedCountAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task SetEmailAsync(TUser user,
                string email,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task SetEmailConfirmedAsync(TUser user,
                bool confirmed,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task SetLockoutEnabledAsync(TUser user,
                bool enabled,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task SetLockoutEndDateAsync(TUser user,
                DateTimeOffset? lockoutEnd,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task SetNormalizedEmailAsync(TUser user,
                string normalizedEmail,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(TUser user,
                string normalizedName,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task SetPasswordHashAsync(TUser user,
                string passwordHash,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task SetPhoneNumberAsync(TUser user,
                string phoneNumber,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user,
                bool confirmed,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(TUser user,
                string userName,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        #region Private methods
        private void CheckNotDisposed() {
            if (this._searchService == null) {
                throw new ObjectDisposedException(nameof(LdapUserStore<TUser>));
            }
        }
        #endregion

        #region Private fields
        private readonly string _accountAttribute;
        private readonly ILdapAuthenticationService<TUser> _authService;
        private readonly LdapPasswordHasher<TUser> _hasher;
        private ILdapSearchService<TUser> _searchService;
        #endregion
    }
}
