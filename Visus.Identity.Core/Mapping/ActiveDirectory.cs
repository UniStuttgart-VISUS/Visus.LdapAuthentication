// <copyright file="ActiveDirectory.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using System;
using System.Diagnostics;
using System.Security.Claims;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;


namespace Visus.Identity.Mapping {

    /// <summary>
    /// Provides IDMU mappings for <see cref="IdentityUser{TKey}"/>
    /// and <see cref="IdentityRole{TKey}"/>
    /// </summary>
    internal static class ActiveDirectory {

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
                .ToAttribute("objectSid")
                .WithConverter<SidConverter>();

            builder.MapProperty(nameof(IdentityRole<TKey>.Name))
                .StoringAccountName()
                .ToAttribute("sAMAccountName");

            builder.MapProperty(nameof(IdentityRole<TKey>.NormalizedName))
                .StoringDistinguishedName()
                .ToAttribute("distinguishedName");
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

            builder.MapAttribute("objectSid")
                .WithConverter<SidConverter>()
                .ToClaims(ClaimTypes.GroupSid);

            builder.MapAttribute("sAMAccountName")
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

            builder.MapProperty(nameof(IdentityUser<TKey>.AccessFailedCount))
                .ToAttribute("badPwdCount")
                .WithConverter<NumberConverter>();

            builder.MapProperty(nameof(IdentityUser<TKey>.Email))
                .ToAttribute("mail");

            builder.MapProperty(nameof(IdentityUser<TKey>.Id))
                .StoringIdentity()
                .ToAttribute("objectSid")
                .WithConverter<SidConverter>();

            builder.MapProperty(nameof(IdentityUser<TKey>.NormalizedUserName))
                .StoringDistinguishedName()
                .ToAttribute("distinguishedName");

            builder.MapProperty(nameof(IdentityUser<TKey>.LockoutEnd))
                .ToAttribute("lockoutTime")
                .WithConverter<FileTimeConverter>();

            builder.MapProperty(nameof(IdentityUser<TKey>.PhoneNumber))
                .ToAttribute("telephoneNumber");

            builder.MapProperty(nameof(IdentityUser<TKey>.UserName))
                .StoringAccountName()
                .ToAttribute("sAMAccountName");
        }

        /// <summary>
        /// Maps user attributes to claims.
        /// </summary>
        public static void MapUserClaims(
                IClaimsMapBuilder builder,
                ClaimsIdentityOptions options) {
            Debug.Assert(builder != null);
            Debug.Assert(options != null);

            builder.MapAttribute("objectSid")
                .WithConverter<SidConverter>()
                .ToClaims(options.UserIdClaimType, ClaimTypes.Sid);

            builder.MapAttribute("sAMAccountName")
                .ToClaim(options.UserNameClaimType);
        }
    }
}
