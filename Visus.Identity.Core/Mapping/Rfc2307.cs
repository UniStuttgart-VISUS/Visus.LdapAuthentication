// <copyright file="Rfc2307.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using System;
using System.Diagnostics;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;


namespace Visus.Identity.Mapping {

    /// <summary>
    /// Provides RFC 2307 mappings for <see cref="IdentityUser{TKey}"/>
    /// and <see cref="IdentityRole{TKey}"/>
    /// </summary>
    internal static class Rfc2307 {

        /// <summary>
        /// Maps properties of <typeparamref name="TRole"/> to LDAP
        /// attributes.
        /// </summary>
        /// <typeparam name="TRole"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="builder"></param>
        public static void MapRole<TRole, TKey>(
                ILdapAttributeMapBuilder<TRole> builder)
                where TRole : IdentityRole<TKey>
                where TKey : IEquatable<TKey> {
            Debug.Assert(builder != null);

            builder.MapProperty(nameof(IdentityRole<TKey>.Id))
                .StoringIdentity()
                .ToAttribute("gidNumber");

            builder.MapProperty(nameof(IdentityRole<TKey>.Name))
                .StoringAccountName()
                .ToAttribute("gid");
        }

        /// <summary>
        /// Maps group attributes to claims.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        public static void MapRoleClaims(IClaimsMapBuilder builder,
                ClaimsIdentityOptions options) {
            Debug.Assert(builder != null);
            Debug.Assert(options != null);

            builder.MapAttribute("gid")
                .ToClaim(options.RoleClaimType);
        }

        /// <summary>
        /// Maps properties of <typeparamref name="TUser"/> to LDAP attributes.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="builder"></param>
        public static void MapUser<TUser, TKey>(
                ILdapAttributeMapBuilder<TUser> builder)
                where TUser : IdentityUser<TKey>
                where TKey : IEquatable<TKey> {
            Debug.Assert(builder != null);

            builder.MapProperty(nameof(IdentityUser<TKey>.Email))
                .ToAttribute("mail");

            builder.MapProperty(nameof(IdentityUser<TKey>.Id))
                .StoringIdentity()
                .ToAttribute("uidNumber");

            builder.MapProperty(nameof(IdentityUser<TKey>.PhoneNumber))
                .ToAttribute("telephoneNumber");

            builder.MapProperty(nameof(IdentityUser<TKey>.UserName))
                .StoringAccountName()
                .ToAttribute("uid");
        }

        /// <summary>
        /// Maps user attributes to claims.
        /// </summary>
        public static void MapUserClaims(
                IClaimsMapBuilder builder,
                ClaimsIdentityOptions options) {
            Debug.Assert(builder != null);
            Debug.Assert(options != null);

            builder.MapAttribute("uidNumber")
                .ToClaim(options.UserIdClaimType);

            builder.MapAttribute("uid")
                .ToClaim(options.UserNameClaimType);
        }
    }
}
