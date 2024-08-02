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


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// A specialisation of the default <see cref="UserManager{TUser}"/>, which
    /// overrides the authentication method to use LDAP.
    /// </summary>
    /// <typeparam name="TUser">The class representing a user in memory.
    /// </typeparam>
    internal class LdapUserManager<TUser>(IUserStore<TUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher,
            IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators,
            ILookupNormalizer keyNormaliser,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILdapAuthenticationService<TUser> authenticationService,
            ILogger<LdapUserManager<TUser>> logger)
        : UserManager<TUser>(store,
              optionsAccessor,
              passwordHasher,
              userValidators,
              passwordValidators,
              keyNormaliser,
              errors,
              services,
              logger)
            where TUser :class {

        public override async Task<bool> CheckPasswordAsync(TUser user,
                string password) {
            try {
                var name = await this.GetUserNameAsync(user)
                    .ConfigureAwait(false);
                var entry = await this._authenticationService.LoginUserAsync(
                    name!, password, this.CancellationToken)
                    .ConfigureAwait(false);
                return (entry != null);
            } catch {
                return false;
            }
        }

        #region Private fields
        private readonly ILdapAuthenticationService<TUser> _authenticationService
            = authenticationService;
        #endregion
    }
}
