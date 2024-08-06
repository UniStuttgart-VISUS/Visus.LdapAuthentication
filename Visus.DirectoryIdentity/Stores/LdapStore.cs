// <copyright file="LdapStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.DirectoryServices.Protocols;
using Visus.DirectoryAuthentication;
using Visus.DirectoryAuthentication.Configuration;
using Visus.Identity.Stores;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryIdentity.Stores {

    /// <summary>
    /// The most basic read-only LDAP-backed user and role (group) store
    /// we can build.
    /// </summary>
    /// <typeparam name="TUser">The type used to represent a user.</typeparam>
    /// <typeparam name="TRole">The type used to represent a role.</typeparam>
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
    /// <exception cref="ArgumentNullException">If any of the parameters
    /// is <c>null</c>.</exception>
    public class LdapStore<TUser, TRole>(
            ILdapSearchService<TUser, TRole> searchService,
            IOptions<LdapOptions> ldapOptions,
            ILdapAttributeMap<TUser> userMap,
            ILdapAttributeMap<TRole> roleMap,
            IClaimsBuilder<TUser, TRole> claimsBuilder,
            IUserClaimsMap userClaims,
            IGroupClaimsMap roleClaims,
            ILogger<LdapStore<TUser, TRole>> logger)
        : LdapStoreBase<TUser, TRole, SearchScope, LdapOptions>(
            searchService,
            ldapOptions,
            userMap,
            roleMap,
            claimsBuilder,
            userClaims,
            roleClaims,
            logger)
            where TUser : class, new()
            where TRole : class, new();
}
