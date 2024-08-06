// <copyright file="IdentityRoleClaimsMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using Visus.Identity.Mapping;
using Visus.Ldap.Configuration;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// Implements a <see cref="IGroupClaimsMap"/> for
    /// <see cref="IdentityRole{TKey}"/>.
    /// </summary>
    /// <remarks>
    /// This special map is required, because we need to know the names of the
    /// claims from the <see cref="IdentityOptions.ClaimsIdentity"/>, which
    /// we can only obtain by dependency injection, so this does not work with
    /// the callback.
    /// </remarks>
    public class IdentityRoleClaimsMap<TRole, TKey, TOptions>(
            IOptions<IdentityOptions> identityOptions,
            IOptions<TOptions> ldapOptions)
        : ClaimsMapBase<TRole, TOptions>(
            WellKnownMappings.MapRoleClaims(ldapOptions, identityOptions),
            ldapOptions)
            where TRole : IdentityRole<TKey>
            where TKey : IEquatable<TKey>
            where TOptions : LdapOptionsBase;
}
