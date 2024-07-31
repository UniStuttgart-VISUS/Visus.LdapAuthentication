// <copyright file="LdapPasswordHasher.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;
using System.Text;
using Visus.DirectoryAuthentication;
using Visus.Ldap;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// A custom <see cref="IPasswordHasher{TUser}"/>, which accounts for the
    /// fact that we use LDAP bind to authenticate rather than reading the
    /// password hash from the directory.
    /// </summary>
    /// <remarks>
    /// Although some directories might store password hashes (eg IDMU used to
    /// store Unix hashes in AD), accessing these requires extremely high
    /// privileges which we do not want to give to web applications. Therefore,
    /// this hasher implements a hack in that it authenticates the user using
    /// an LDAP bind and if this bind succeeds, we compute a transient hash
    /// from the user itself, which is then handed out to the identity API for
    /// comparing it against the user.
    /// </remarks>
    /// <typeparam name="TUser"></typeparam>
    internal sealed class LdapPasswordHasher<TUser> : IPasswordHasher<TUser>
            where TUser : class, ILdapIdentityUser {

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="authService">The authentication service that provides
        /// access to the directory.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="authService"/> is <c>null</c>.</exception>
        public LdapPasswordHasher(
                ILdapAuthenticationService<TUser> authService) {
            this._authService = authService
                ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// Authenticates the user against the directory and if that succeeded,
        /// returns <see cref="HashUser(TUser)"/> of the user object we obtained
        /// from the directory.
        /// </summary>
        /// <param name="user">The user to authenticate using
        /// <see cref="ILdapUser.AccountName"/>.</param>
        /// <param name="password">The password used to authenticate the user.
        /// </param>
        /// <returns>A hash of the user</returns>
        public string HashPassword(TUser user, string password) {
            throw new NotImplementedException();
            //_ = user ?? throw new ArgumentNullException(nameof(user));
            //var u = this._authService.Login(user.AccountName, password);

            //if (u == null) {
            //    // If we could not bind to the directory, return an invalid
            //    // hash.
            //    return null;
            //} else {
            //    // If we successfully authenticated and found the user, we
            //    // hash the user object we retrieved in order to later compre
            //    // this hash with the one created by LdapUserStore.
            //    return this.HashUser(u);
            //}
        }

        /// <summary>
        /// Gets a hash of the user which is the reference we expect to get from
        /// the middleware.
        /// </summary>
        /// <param name="user">The user to hash.</param>
        /// <returns>A hash of the user.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="user"/>
        /// is <c>null</c>.</exception>
        public string HashUser(TUser user) {
            _ = user ?? throw new ArgumentNullException(nameof(user));

            var sb = new StringBuilder();
            //sb.AppendLine(user.Identity);
            //foreach (var c in user.Claims) {
            //    sb.AppendLine(c.Type + c.Value);
            //}

            using (var sha = SHA256.Create()) {
                var value = sb.ToString();
                var hash = sha.ComputeHash(Encoding.Unicode.GetBytes(value));
                return Convert.ToBase64String(hash);
            }
        }

        /// <inheritdoc />
        public PasswordVerificationResult VerifyHashedPassword(TUser user,
                string hashedPassword, string providedPassword) {
            // TODO: We should have some kind of nonce here to prevent replays.
            return ((hashedPassword == providedPassword)
                && (providedPassword == this.HashUser(user)))
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }

        #region Private fields
        private readonly ILdapAuthenticationService<TUser> _authService;
        #endregion
    }
}
