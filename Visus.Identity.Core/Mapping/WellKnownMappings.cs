// <copyright file="WellKnownMappings.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Visus.Identity.Properties;
using Visus.Ldap.Claims;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;


namespace Visus.Identity.Mapping {

    /// <summary>
    /// Provides builder callbacks for well-known schema mappings to
    /// <see cref="IdentityUser{TKey}"/> and <see cref="IdentityRole{TKey}"/>.
    /// </summary>
    public static class WellKnownMappings {

        /// <summary>
        /// Creates a mapping for <typeparamref name="TRole"/> for one of the
        /// well-known LDAP schemas.
        /// </summary>
        /// <typeparam name="TRole"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void MapRole<TRole, TKey>(
                ILdapAttributeMapBuilder<TRole> builder,
                LdapOptionsBase options)
                where TRole : IdentityRole<TKey>
                where TKey : IEquatable<TKey> {
            switch (options.Schema) {
                case Schema.ActiveDirectory:
                    ActiveDirectory.MapRole<TRole, TKey>(builder);
                    break;

                case Schema.IdentityManagementForUnix:
                    IdentityManagementForUnix.MapRole<TRole, TKey>(builder);
                    break;

                case Schema.Rfc2307:
                    Rfc2307.MapRole<TRole, TKey>( builder);
                    break;

                default:
                    throw new ArgumentException(string.Format(
                        Resources.ErrorSchemaNotWellKnown,
                        options.Schema));
            }
        }

        /// <summary>
        /// Creates a mapper callback for the claims of a role.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ldapOptions"></param>
        /// <param name="claimOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Action<IClaimsMapBuilder, LdapOptionsBase> MapRoleClaims(
                IServiceProvider services,
                ClaimsIdentityOptions claimOptions) {
            var ldapOptions = services.GetRequiredService<IOptions<LdapOptionsBase>>();
            var claims = services.GetRequiredService<IOptions<IdentityOptions>>();


            return ldapOptions.Value.Schema switch {
                Schema.ActiveDirectory => (b, _)
                    => ActiveDirectory.MapRoleClaims(b, claimOptions),
                Schema.IdentityManagementForUnix => (b, _)
                    => IdentityManagementForUnix.MapRoleClaims(b, claimOptions),
                Schema.Rfc2307 => (b, _)
                    => Rfc2307.MapRoleClaims(b, claimOptions),
                _ => throw new ArgumentException(string.Format(
                    Resources.ErrorSchemaNotWellKnown,
                    ldapOptions.Value.Schema))
            };
        }

        /// <summary>
        /// Creates a mapping for <typeparamref name="TUser"/> for one of the
        /// well-known LDAP schemas.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void MapUser<TUser, TKey>(
                ILdapAttributeMapBuilder<TUser> builder,
                LdapOptionsBase options)
                where TUser : IdentityUser<TKey>
                where TKey : IEquatable<TKey> {
            switch (options.Schema) {
                case Schema.ActiveDirectory:
                    ActiveDirectory.MapUser<TUser, TKey>(builder);
                    break;

                case Schema.IdentityManagementForUnix:
                    IdentityManagementForUnix.MapUser<TUser, TKey>(builder);
                    break;

                case Schema.Rfc2307:
                    Rfc2307.MapUser<TUser, TKey>(builder);
                    break;

                default:
                    throw new ArgumentException(string.Format(
                        Resources.ErrorSchemaNotWellKnown,
                        options.Schema));
            }
        }

        /// <summary>
        /// Creates a mapper callback for the claims of a user.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ldapOptions"></param>
        /// <param name="claimOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Action<IClaimsMapBuilder, LdapOptionsBase> MapUserClaims(
                LdapOptionsBase ldapOptions,
                ClaimsIdentityOptions claimOptions) {
            return ldapOptions.Schema switch {
                Schema.ActiveDirectory => (b, _)
                    => ActiveDirectory.MapUserClaims(b, claimOptions),
                Schema.IdentityManagementForUnix => (b, _)
                    => IdentityManagementForUnix.MapUserClaims(b, claimOptions),
                Schema.Rfc2307 => (b, _)
                    => Rfc2307.MapUserClaims(b, claimOptions),
                _ => throw new ArgumentException(string.Format(
                    Resources.ErrorSchemaNotWellKnown,
                    ldapOptions.Schema))
            };
        }
    }
}
