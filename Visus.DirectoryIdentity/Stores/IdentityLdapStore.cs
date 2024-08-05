// <copyright file="LdapUserStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryIdentity.Properties;
using Visus.Identity.Stores;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryIdentity.Stores {

    /// <summary>
    /// The implementation of an LDAP user store using the default
    /// <see cref="IdentityUser"/>.
    /// </summary>
    /// <typeparam name="TUserKey">The type used for the primary key for the
    /// users.</typeparam>
    /// <typeparam name="TRoleKey">The type used for the primray key for the
    /// roles.</typeparam>
    public class IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>(
            ILdapSearchService<TUser, TRole> searchService,
            IOptions<LdapOptions> ldapOptions,
            ILdapAttributeMap<TUser> userMap,
            ILdapAttributeMap<TRole> roleMap,
            IClaimsBuilder<TUser, TRole> claimsBuilder,
            IUserClaimsMap userClaims,
            IGroupClaimsMap roleClaims,
            ILogger<IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>> logger)
        : IdentityLdapStoreBase<TUser, TUserKey, TRole, TRoleKey, SearchScope>(
            searchService,
            ldapOptions?.Value!,
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
