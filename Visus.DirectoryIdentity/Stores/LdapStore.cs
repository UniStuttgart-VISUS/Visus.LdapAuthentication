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
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TRole"></typeparam>
    /// <param name="searchService"></param>
    /// <param name="ldapOptions"></param>
    /// <param name="userMap"></param>
    /// <param name="roleMap"></param>
    /// <param name="claimsBuilder"></param>
    /// <param name="userClaims"></param>
    /// <param name="roleClaims"></param>
    /// <param name="logger"></param>
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
        : LdapStoreBase<TUser, TRole, SearchScope>(
            searchService,
            ldapOptions?.Value!,
            userMap,
            roleMap,
            claimsBuilder,
            userClaims,
            roleClaims,
            logger)
            where TUser : class, new()
            where TRole : class, new();
}
