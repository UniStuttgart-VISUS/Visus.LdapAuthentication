// <copyright file="UserStoreBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
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
using Visus.LdapAuthentication;


namespace Visus.LdapIdentity {

    public abstract class UserStoreBase<TUser>
            : IUserClaimStore<TUser>,
            IUserEmailStore<TUser>,
            IUserLockoutStore<TUser>,
            IPasswordHasher<TUser>,
            IUserPasswordStore<TUser>,
            IUserPhoneNumberStore<TUser>,
            IQueryableUserStore<TUser>
            where TUser : class, ILdapUser {

        #region Public properties
        /// <inheritdoc />
        public IQueryable<TUser> Users
            => this._ldapSearch.GetUsers().AsQueryable();
        #endregion

        #region Public methods
        public Task AddClaimsAsync(TUser user,
                IEnumerable<Claim> claims,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> CreateAsync(TUser user,
                CancellationToken cancellationToken) {
            //return IdentityResult.Failed()
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public void Dispose() { }

        public Task<TUser> FindByEmailAsync(string normalizedEmail,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the <see cref="TUser"/> from the unique ID that is stored
        /// in <see cref="TUser.Identity"/>.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<TUser> FindByIdAsync(string userId,
                CancellationToken cancellationToken) {
            _ = userId ?? throw new ArgumentNullException(nameof(userId));
            return Task.FromResult(this._ldapSearch.GetUserByIdentity(userId));
        }

        /// <summary>
        /// Gets the <see cref="TUser"/> for the user name that is
        /// requested from the user at login.
        /// </summary>
        /// <param name="normalizedUserName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<TUser> FindByNameAsync(string normalizedUserName,
                CancellationToken cancellationToken) {
            _ = normalizedUserName
                ?? throw new ArgumentNullException(nameof(normalizedUserName));
            throw new NotImplementedException();
        }

        //AccountLockoutTime,LastBadPasswordAttempt,BadPwdCount,LockedOut

        public Task<int> GetAccessFailedCountAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<IList<Claim>> GetClaimsAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<string> GetEmailAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedEmailAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<string> GetPhoneNumberAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<IList<TUser>> GetUsersForClaimAsync(Claim claim,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public string HashPassword(TUser user, string password) {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            _ = password ?? throw new ArgumentNullException(nameof(password));
            var result = this._ldapAuth.Login(user.AccountName, password);
            return result?.AccountName;
        }

        public Task<bool> HasPasswordAsync(TUser user,
                CancellationToken cancellationToken) {
            return Task.FromResult(true);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task RemoveClaimsAsync(TUser user,
                IEnumerable<Claim> claims,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task ReplaceClaimAsync(TUser user,
                Claim claim,
                Claim newClaim,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task ResetAccessFailedCountAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task SetEmailAsync(TUser user, string email,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task SetEmailConfirmedAsync(TUser user,
                bool confirmed,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task SetLockoutEnabledAsync(TUser user,
                bool enabled,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task SetLockoutEndDateAsync(TUser user,
                DateTimeOffset? lockoutEnd,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task SetNormalizedEmailAsync(TUser user,
                string normalizedEmail,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(TUser user,
                string normalizedName,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task SetPasswordHashAsync(TUser user,
                string passwordHash,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task SetPhoneNumberAsync(TUser user,
                string phoneNumber,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user,
                bool confirmed,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(TUser user,
                string userName,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user,
                string hashedPassword,
                string providedPassword) {
            throw new NotImplementedException();
        }
        #endregion

        #region Private fields
        private readonly ILdapAuthenticationService _ldapAuth;
        private readonly ILdapSearchService<TUser> _ldapSearch;
        #endregion
    }
}
