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

    //AccountLockoutTime,LastBadPasswordAttempt,BadPwdCount,LockedOut

    /// <summary>
    /// An ASP.NET Core Identity user store that is backed by an LDAP server.
    /// </summary>
    /// <remarks>
    /// The LDAP user store does support authentication and retrieval of
    /// properties and claims of a user, but it does not support modifying
    /// any directory content.
    /// </remarks>
    /// <typeparam name="TUser"></typeparam>
    public class UserStore<TUser>
            : IUserClaimStore<TUser>,
            IUserEmailStore<TUser>,
            IUserLockoutStore<TUser>,
            IPasswordHasher<TUser>,
            IUserPasswordStore<TUser>,
            IUserPhoneNumberStore<TUser>,
            IQueryableUserStore<TUser>
            where TUser : class, ILdapUser {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="ldapAuth">The authentication service that should be
        /// used to verify a user's password.</param>
        /// <param name="ldapSearch">The search service that should be used to
        /// retrieve properties about the user.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="ldapAuth"/> is <c>null</c>, or if
        /// <paramref name="ldapSearch"/> is <c>null</c>.</exception>
        public UserStore(ILdapAuthenticationService ldapAuth,
                ILdapSearchService<TUser> ldapSearch) {
            this._ldapAuth = ldapAuth
                ?? throw new ArgumentNullException(nameof(ldapAuth));
            this._ldapSearch = ldapSearch
                ?? throw new ArgumentNullException(nameof(ldapSearch));
        }
        #endregion

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

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IdentityResult> CreateAsync(TUser user,
                CancellationToken cancellationToken) {
            return Failure(nameof(NotImplementedException),
                Properties.Resources.ErrorNotSupported);
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IdentityResult> DeleteAsync(TUser user,
                CancellationToken cancellationToken) {
            return Failure(nameof(NotImplementedException),
                Properties.Resources.ErrorNotSupported);
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

        /// <inheritdoc />
        public Task<bool> HasPasswordAsync(TUser user,
                CancellationToken cancellationToken) {
            return Task.FromResult(true);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="claims"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task RemoveClaimsAsync(TUser user,
                IEnumerable<Claim> claims,
                CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="claim"></param>
        /// <param name="newClaim"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ReplaceClaimAsync(TUser user,
                Claim claim,
                Claim newClaim,
                CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task ResetAccessFailedCountAsync(TUser user,
                CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="email"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetEmailAsync(TUser user, string email,
                CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetEmailConfirmedAsync(TUser user,
                bool confirmed,
                CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetLockoutEnabledAsync(TUser user,
                bool enabled,
                CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="lockoutEnd"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetLockoutEndDateAsync(TUser user,
                DateTimeOffset? lockoutEnd,
                CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="normalizedEmail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetNormalizedEmailAsync(TUser user,
                string normalizedEmail,
                CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="normalizedName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetNormalizedUserNameAsync(TUser user,
                string normalizedName,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="passwordHash"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetPasswordHashAsync(TUser user,
                string passwordHash,
                CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetPhoneNumberAsync(TUser user,
                string phoneNumber,
                CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetPhoneNumberConfirmedAsync(TUser user,
                bool confirmed,
                CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetUserNameAsync(TUser user,
                string userName,
                CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IdentityResult> UpdateAsync(TUser user,
                CancellationToken cancellationToken) {
            return Failure(nameof(NotImplementedException),
                Properties.Resources.ErrorNotSupported);
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user,
                string hashedPassword,
                string providedPassword) {
            throw new NotImplementedException();
        }
        #endregion

        #region Private class methods
        private static Task<IdentityResult> Failure(string code,
                string description) {
            var error = new IdentityError() {
                Code = code,
                Description = description
            };

            return Task.FromResult(IdentityResult.Failed(error));
        }
        #endregion

        #region Private fields
        private readonly ILdapAuthenticationService _ldapAuth;
        private readonly ILdapSearchService<TUser> _ldapSearch;
        #endregion
    }
}
