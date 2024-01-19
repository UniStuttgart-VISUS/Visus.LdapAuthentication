// <copyright file="UserStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
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


namespace Visus.LdapIdentity {

    //public abstract class UserStore<TUser, TKey, TUserClaim, TUserLogin, TUserToken>
    //        : UserStoreBase<TUser, TKey, TUserClaim, TUserLogin, TUserToken>
    //        where TUser : IdentityUser<TKey>
    //        where TKey : IEquatable<TKey>
    //        where TUserClaim : IdentityUserClaim<TKey>, new()
    //        where TUserLogin : IdentityUserLogin<TKey>, new()
    //        where TUserToken : IdentityUserToken<TKey>, new() { }

    /* IUserLoginStore<TUser>
     * IUserStore<TUser>
     * IUserClaimStore<TUser>
     * IUserPasswordStore<TUser>
     * IUserSecurityStampStore<TUser>
     * IUserEmailStore<TUser>
     * IUserLockoutStore<TUser>
     * IUserPhoneNumberStore<TUser>
     * IQueryableUserStore<TUser>
     * IUserTwoFactorStore<TUser>
     * IUserAuthenticationTokenStore<TUser>
     * IUserAuthenticatorKeyStore<TUser>
     * IUserTwoFactorRecoveryCodeStore<TUser>
     */


    /* optional:
     * IUserAuthenticationTokenStore
     * IUserAuthenticatorKeyStore
     * IUserTwoFactorRecoveryCodeStore
     * IUserTwoFactorStore
     * IUserPasswordStore
     * IUserSecurityStampStore
     * IUserRoleStore
     * IUserLoginStore
     * IUserEmailStore
     * IUserPhoneNumberStore
     * IUserClaimStore
     * IUserLockoutStore
     * IQueryableUserStore
     * 
     */
}
