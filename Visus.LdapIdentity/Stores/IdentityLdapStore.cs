// <copyright file="LdapUserStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Visus.Identity.Stores;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;
using Visus.LdapAuthentication;
using Visus.LdapAuthentication.Configuration;


namespace Visus.LdapIdentity.Stores {

    /// <summary>
    /// The implementation of an LDAP user store using the default
    /// <see cref="IdentityUser"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of <see cref="IdentityUser{TKey}"/>
    /// used to represent a user.</typeparam>
    /// <typeparam name="TUserKey">The type used for the primary key for the
    /// users.</typeparam>
    /// <typeparam name="TRole">The type of <see cref="IdentityRole{TKey}"/>
    /// used to represent a role.</typeparam>
    /// <typeparam name="TRoleKey">The type used for the primray key for the
    /// roles.</typeparam>
    /// <param name="searchService">The search service used to look up users
    /// and roles in the directory.</param>
    /// <param name="ldapOptions">The LDAP configuration, which most importantly
    /// defines the LDAP schema and thus how attributes are mapped to claims.
    /// </param>
    /// <param name="userMap">The map from LDAP attributes to properties
    /// of <typeparamref name="TUser"/>.</param>
    /// <param name="roleMap">The map from LDAP attributes to properties
    /// of <typeparamref name="TRole"/>.</param>
    /// <param name="claimsBuilder">A claims builder that generates claims
    /// from <typeparamref name="TUser"/> and <typeparamref name="TRole"/>
    /// objects.</param>
    /// <param name="userClaims">The map from LDAP attributes to user
    /// claims.</param>
    /// <param name="roleClaims">The map from LDAP attributes to role
    /// claims.</param>
    /// <param name="logger">A logger for persisting messages.</param>
    /// <exception cref="ArgumentNullException">If any of the parameters
    /// is <c>null</c>.</exception>
    public class IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>(
            ILdapSearchService<TUser, TRole> searchService,
            IOptions<LdapOptions> ldapOptions,
            ILdapAttributeMap<TUser> userMap,
            ILdapAttributeMap<TRole> roleMap,
            IClaimsBuilder<TUser, TRole> claimsBuilder,
            IUserClaimsMap userClaims,
            IGroupClaimsMap roleClaims,
            ILogger<IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>> logger)
        : IdentityLdapStoreBase<TUser, TUserKey, TRole, TRoleKey, SearchScope, LdapOptions>(
            searchService,
            ldapOptions,
            userMap,
            roleMap,
            claimsBuilder,
            userClaims,
            roleClaims,
            logger)
            where TUser : IdentityUser<TUserKey>, new()
            where TRole : IdentityRole<TRoleKey>, new()
            where TUserKey : IEquatable<TUserKey>
            where TRoleKey : IEquatable<TRoleKey>;
}
