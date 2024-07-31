// <copyright file="LdapUserStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication;
using Visus.DirectoryAuthentication.Configuration;
using Visus.Ldap;
using Visus.Ldap.Mapping;


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
        /// <param name="ldapOptions"></param>
        public LdapUserStore(ILdapAuthenticationService<TUser> authService,
                ILdapSearchService<TUser, LdapGroup> searchService,
                IOptions<LdapOptions> ldapOptions) {
            this._authService = authService
                ?? throw new ArgumentNullException(nameof(authService));
            this._searchService = searchService
                ?? throw new ArgumentNullException(nameof(searchService));
            var schema = ldapOptions?.Value?.Schema
                ?? throw new ArgumentNullException(nameof(ldapOptions));

            this._hasher = new LdapPasswordHasher<TUser>(this._authService);

            //{
            //    var prop = typeof(TUser).GetProperty(
            //        nameof(LdapIdentityUser.AccountName));
            //    var att = LdapAttributeAttribute.GetLdapAttribute(prop, schema);

            //    if (att == null) {
            //        var msg = Properties.Resources.ErrorNoLdapAttribute;
            //        msg = string.Format(msg, prop.Name, schema);
            //        throw new ArgumentException(msg, nameof(ldapOptions));
            //    }

            //    this._accountAttribute = att.Name;
            //}

            //{
            //    var prop = typeof(TUser).GetProperty(
            //        nameof(ILdapIdentityUser.EmailAddress));
            //    var att = LdapAttributeAttribute.GetLdapAttribute(prop, schema);

            //    if (att == null) {
            //        var msg = Properties.Resources.ErrorNoLdapAttribute;
            //        msg = string.Format(msg, prop.Name, schema);
            //        throw new ArgumentException(msg, nameof(ldapOptions));
            //    }

            //    this._emailAttribute = att.Name;
            //}
        }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public IQueryable<TUser> Users
            => this._searchService.GetUsers().AsQueryable();
        #endregion

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="claims"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<IdentityResult> CreateAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<IdentityResult> DeleteAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose() {
            if (this._searchService != null) {
                this._searchService.Dispose();
                this._searchService = null;
            }
        }

        /// <summary>
        /// Retrieves a user account by the <see cref="ILdapUser.EmailAddress"/>
        /// attribute.
        /// </summary>
        /// <param name="normalisedEmail">The normalised e-mail address to
        /// search.</param>
        /// <param name="cancellationToken">A cancellation token to abort the
        /// operation.</param>
        /// <returns>The user object matching the address or <c>null</c> if no
        /// such address could be found or if it was not unique.</returns>
        public async Task<TUser> FindByEmailAsync(string normalisedEmail,
                CancellationToken cancellationToken) {
            _ = normalisedEmail
                ?? throw new ArgumentNullException(normalisedEmail);
            this.CheckNotDisposed();

            cancellationToken.ThrowIfCancellationRequested();
            return (await this._searchService.GetUsersAsync(
                $"({this._emailAttribute}={normalisedEmail})")
                .ConfigureAwait(false))
                .SingleOrDefault();
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
            this.CheckNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
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
            _ = normalisedUserName
                ?? throw new ArgumentNullException(normalisedUserName);
            this.CheckNotDisposed();
            throw new NotImplementedException();

            //cancellationToken.ThrowIfCancellationRequested();
            //return (await this._searchService.GetUsersAsync(
            //    $"({this._accountAttribute}={normalisedUserName})")
            //    .ConfigureAwait(false))
            //    .SingleOrDefault();
        }

        /// <summary>
        /// Gets the number of failed access attempts.
        /// </summary>
        /// <remarks>
        /// The method will only make a call to the directory if the property
        /// of <paramref name="user"/> is zero. Otherwise, it will return
        /// <see cref="ILdapIdentityUser.AccessFailedCount"/> without querying
        /// the directory.
        /// </remarks>
        /// <param name="user">The user to retrieve the value for.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns>The number of failed access attempts.</returns>
        public async Task<int> GetAccessFailedCountAsync(TUser user,
                CancellationToken cancellationToken) {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            this.CheckNotDisposed();

            if (user.AccessFailedCount > 0) {
                return user.AccessFailedCount;

            } else {
                //cancellationToken.ThrowIfCancellationRequested();
                //var u = await this._searchService.GetUserByIdentityAsync(
                //    user.Identity).ConfigureAwait(false);
                //return u.AccessFailedCount;
                throw new NotImplementedException();
            }
        }

        public Task<IList<Claim>> GetClaimsAsync(TUser user,
                CancellationToken cancellationToken) {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the e-mail address of the specified user.
        /// </summary>
        /// <remarks>
        /// The method will only make a call to the directory if the property
        /// of <paramref name="user"/> is not set. Otherwise, it will return
        /// <see cref="ILdapUser.EmailAddress"/> without querying the directory.
        /// </remarks>
        /// <param name="user">The user to retrieve the value for.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns>The e-mail address of the user.</returns>
        public async Task<string> GetEmailAsync(TUser user,
                CancellationToken cancellationToken) {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            this.CheckNotDisposed();

            //if (!string.IsNullOrWhiteSpace(user.EmailAddress)) {
            //    return user.EmailAddress;

            //} else {
            //    cancellationToken.ThrowIfCancellationRequested();
            //    var u = await this._searchService.GetUserByIdentityAsync(
            //        user.Identity).ConfigureAwait(false);
            //    return u.EmailAddress;
            //}
            throw new NotImplementedException();
        }

        /// <summary>
        /// Answer whether the e-mail address has been confirmed, which is
        /// always <c>true</c> for an LDAP directory that is managed by an
        /// administrator and has no self service features.
        /// </summary>
        /// <param name="user">The user to get the value for.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns><c>true</c>, unconditionally.</returns>
        public Task<bool> GetEmailConfirmedAsync(TUser user,
                CancellationToken cancellationToken) {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task<bool> GetLockoutEnabledAsync(TUser user,
                CancellationToken cancellationToken) {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            this.CheckNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            // Note: we always query that, because it would be bad if we
            // returned a stale value from 'user' here.
            //var u = await this._searchService.GetUserByIdentityAsync(
            //    user.Identity).ConfigureAwait(false);
            //return u.IsLockoutEnabled;
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user,
                CancellationToken cancellationToken) {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            this.CheckNotDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            // Note: we always query that, because it would be bad if we
            // returned a stale value from 'user' here.
            //var u = await this._searchService.GetUserByIdentityAsync(
            //    user.Identity).ConfigureAwait(false);
            //return u.LockoutTime;
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<string> GetNormalizedEmailAsync(TUser user,
                CancellationToken cancellationToken) {
            return this.GetEmailAsync(user, cancellationToken);
        }

        /// <inheritdoc />
        public Task<string> GetNormalizedUserNameAsync(TUser user,
                CancellationToken cancellationToken) {
            return this.GetUserNameAsync(user, cancellationToken);
        }

        /// <summary>
        /// Gets the password hash of the user.
        /// </summary>
        /// <remarks>
        /// This method actually computes a hash of <paramref name="user"/>
        /// itself, because we do not have access to the password hashes in
        /// the directory. See <see cref="PasswordHasher{TUser}"/> for how we
        /// work around this problem.
        /// </remarks>
        /// <param name="user">The user to get the value for.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns>A hash for the user.</returns>
        public Task<string> GetPasswordHashAsync(TUser user,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(this._hasher.HashUser(user));
        }

        /// <summary>
        /// Gets the phone number of the specified user.
        /// </summary>
        /// <remarks>
        /// The method will only make a call to the directory if the property
        /// of <paramref name="user"/> is not set. Otherwise, it will return
        /// <see cref="ILdapIdentityUser.PhoneNumber"/> without querying the
        /// directory.
        /// </remarks>
        /// <param name="user">The user to retrieve the value for.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns>The phone number of the user.</returns>
        public async Task<string> GetPhoneNumberAsync(TUser user,
                CancellationToken cancellationToken) {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            this.CheckNotDisposed();

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber)) {
                return user.PhoneNumber;

            } else {
                cancellationToken.ThrowIfCancellationRequested();
                //var u = await this._searchService.GetUserByIdentityAsync(
                //    user.Identity).ConfigureAwait(false);
                //return u.PhoneNumber;
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user,
                CancellationToken cancellationToken) {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<string> GetUserIdAsync(TUser user,
                CancellationToken cancellationToken) {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            //return Task.FromResult(user.Identity);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the <see cref="ILdapUser.AccountName"/> for the given
        /// <paramref name="user"/>.
        /// </summary>
        /// <remarks>
        /// The method will only make a call to the directory if the property
        /// of <paramref name="user"/> is not set. Otherwise, it will return
        /// <see cref="ILdapUser.AccountName"/> without querying the directory.
        /// </remarks>
        /// <param name="user">The user to retrieve the value for.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns>The name of the user account.</returns>
        public async Task<string> GetUserNameAsync(TUser user,
                CancellationToken cancellationToken) {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            this.CheckNotDisposed();

            //if (!string.IsNullOrWhiteSpace(user.AccountName)) {
            //    return user.AccountName;

            //} else {
            //    cancellationToken.ThrowIfCancellationRequested();
            //    var u = await this._searchService.GetUserByIdentityAsync(
            //        user.Identity).ConfigureAwait(false);
            //    return u.AccountName;
            //}
            throw new NotImplementedException();
        }

        public Task<IList<TUser>> GetUsersForClaimAsync(Claim claim,
                CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets whether the given user has a password, which is always the
        /// case for LDAP accounts.
        /// </summary>
        /// <param name="user">The user to get the value for.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the opertion.</param>
        /// <returns><c>true</c>, unconditionally.</returns>
        public Task<bool> HasPasswordAsync(TUser user,
                CancellationToken cancellationToken) {
            return Task.FromResult(true);
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<int> IncrementAccessFailedCountAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="claims"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task RemoveClaimsAsync(TUser user,
                IEnumerable<Claim> claims,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="claim"></param>
        /// <param name="newClaim"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task ReplaceClaimAsync(TUser user,
                Claim claim,
                Claim newClaim,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task ResetAccessFailedCountAsync(TUser user,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="email"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetEmailAsync(TUser user,
                string email,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetEmailConfirmedAsync(TUser user,
                bool confirmed,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetLockoutEnabledAsync(TUser user,
                bool enabled,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="lockoutEnd"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetLockoutEndDateAsync(TUser user,
                DateTimeOffset? lockoutEnd,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="normalizedEmail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetNormalizedEmailAsync(TUser user,
                string normalizedEmail,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="normalizedName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetNormalizedUserNameAsync(TUser user,
                string normalizedName,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="passwordHash"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetPasswordHashAsync(TUser user,
                string passwordHash,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetPhoneNumberAsync(TUser user,
                string phoneNumber,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetPhoneNumberConfirmedAsync(TUser user,
                bool confirmed,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetUserNameAsync(TUser user,
                string userName,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<IdentityResult> UpdateAsync(TUser user,
                CancellationToken cancellationToken) {
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
        private readonly string _emailAttribute;
        private readonly LdapPasswordHasher<TUser> _hasher;
        private ILdapSearchService<TUser, LdapGroup> _searchService;
        #endregion
    }
}
