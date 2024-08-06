// <copyright file="LdapUserManager.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Visus.Ldap;
using Visus.Ldap.Mapping;


namespace Visus.Identity.Managers {

    /// <summary>
    /// A specialisation of the default <see cref="UserManager{TUser}"/>, which
    /// overrides the authentication method to use LDAP.
    /// </summary>
    /// <typeparam name="TUser">The class representing a user in memory.
    /// </typeparam>
    internal class LdapUserManager<TUser>(IUserStore<TUser> store,
            IOptions<IdentityOptions> options,
            IPasswordHasher<TUser> passwordHasher,
            IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators,
            ILookupNormalizer keyNormaliser,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILdapAuthenticationService<TUser> authenticationService,
            ILdapAttributeMap<TUser> userMap,
            ILogger<LdapUserManager<TUser>> logger)
        : UserManager<TUser>(store,
            options,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormaliser,
            errors,
            services,
            logger)
            where TUser :class {

        /// <summary>
        /// Retrieves the account name from <paramref name="user"/>, tries to
        /// bing this account with the given <paramref name="password"/> and
        /// answers the success of this operation.
        /// </summary>
        /// <param name="user">The user to logon.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns><c>true</c> if the login to the LDAP server succeeded,
        /// <c>false</c> otherwise.</returns>
        public override async Task<bool> CheckPasswordAsync(TUser user,
                string password) {
            try {
                // Get the account name from the attribute map.
                var name = this._userMap.AccountNameProperty?.GetValue(user)
                    as string;

                // If we do not have a name here, try the user store next
                if (name == null) {
                    this.Logger.LogWarning("Could not obtain user account name "
                        + "of {User} from the LDAP attribute map. Falling back "
                        + "to using the user store.", user);
                    name = await this.GetUserNameAsync(user)
                        .ConfigureAwait(false);
                }

                if (name == null) {
                    this.Logger.LogError("Failed to obtain a user name of "
                        + "{User}. Please make sure that your user object "
                        + "has been properly mapped to LDAP attributes and "
                        + "that the AccountNameProperty has been set.", user);
                    return false;
                }

                var entry = await this._authenticationService
                    .LoginUserAsync(name, password, this.CancellationToken)
                    .ConfigureAwait(false);
                return (entry != null);
            } catch (Exception ex) {
                this.Logger.LogError(ex, "An unexpected error occurred when "
                    + "trying to validate the login {User} against the LDAP "
                    + "directory.", user);
                return false;
            }
        }

        #region Private fields
        private readonly ILdapAuthenticationService<TUser> _authenticationService
            = authenticationService;
        private readonly ILdapAttributeMap<TUser> _userMap = userMap;
        #endregion
    }
}
