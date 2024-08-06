// <copyright file="WellKnownMappings.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using Visus.Identity.Properties;
using Visus.Ldap.Claims;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;
using Adds = Visus.Identity.Mapping.ActiveDirectory;
using Idmu = Visus.Identity.Mapping.IdentityManagementForUnix;


namespace Visus.Identity.Mapping {

    /// <summary>
    /// Provides builder callbacks for well-known schema mappings to
    /// <see cref="IdentityUser{TKey}"/> and <see cref="IdentityRole{TKey}"/>.
    /// </summary>
    public static class WellKnownMappings {

        #region Public methods
        /// <summary>
        /// Creates a mapping for <typeparamref name="TRole"/> for one of the
        /// well-known LDAP schemas.
        /// </summary>
        /// <typeparam name="TRole"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void MapRole<TRole, TKey, TOptions>(
                ILdapAttributeMapBuilder<TRole> builder,
                TOptions options)
                where TRole : IdentityRole<TKey>
                where TKey : IEquatable<TKey>
                where TOptions : LdapOptionsBase {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            switch (options.Schema) {
                case Schema.ActiveDirectory:
                    Adds.MapRole<TRole, TKey>(builder);
                    break;

                case Schema.IdentityManagementForUnix:
                    Idmu.MapRole<TRole, TKey>(builder);
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
        /// Creates a mapping for <typeparamref name="TUser"/> for one of the
        /// well-known LDAP schemas.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void MapUser<TUser, TKey, TOptions>(
                ILdapAttributeMapBuilder<TUser> builder,
                TOptions options)
                where TUser : IdentityUser<TKey>
                where TKey : IEquatable<TKey>
                where TOptions : LdapOptionsBase {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            switch (options.Schema) {
                case Schema.ActiveDirectory:
                    Adds.MapUser<TUser, TKey>(builder);
                    break;

                case Schema.IdentityManagementForUnix:
                    Idmu.MapUser<TUser, TKey>(builder);
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
        #endregion

        #region Internal methods
        /// <summary>
        /// Creates a mapper callback for the claims of a role.
        /// </summary>
        /// <remarks>
        /// This method is intended only for use in
        /// <see cref="IdentityRoleClaimsMap{TObject, TKey, TOptions}"/>.
        /// </remarks>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="ldapOptions"></param>
        /// <param name="identityOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Action<IClaimsMapBuilder, TOptions> MapRoleClaims<TOptions>(
                IOptions<TOptions> ldapOptions,
                IOptions<IdentityOptions> identityOptions)
                where TOptions : LdapOptionsBase {
            ArgumentNullException.ThrowIfNull(ldapOptions?.Value,
                nameof(ldapOptions));
            ArgumentNullException.ThrowIfNull(identityOptions?.Value,
                nameof(identityOptions));

            var o = identityOptions.Value.ClaimsIdentity;
            ArgumentNullException.ThrowIfNull(o, nameof(identityOptions));

            return ldapOptions.Value.Schema switch {
                Schema.ActiveDirectory => (b, _) => Adds.MapRoleClaims(b, o),
                Schema.IdentityManagementForUnix => (b, _) => Idmu.MapRoleClaims(b, o),
                Schema.Rfc2307 => (b, _) => Rfc2307.MapRoleClaims(b, o),
                _ => throw new ArgumentException(string.Format(
                    Resources.ErrorSchemaNotWellKnown,
                    ldapOptions.Value.Schema))
            };
        }

        /// <summary>
        /// Creates a mapper callback for the claims of a user.
        /// </summary>
        /// <remarks>
        /// This method is intended only for use in
        /// <see cref="IdentityUserClaimsMap{TObject, TKey, TOptions}"/>.
        /// </remarks>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="ldapOptions"></param>
        /// <param name="identityOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Action<IClaimsMapBuilder, TOptions> MapUserClaims<TOptions>(
                IOptions<TOptions> ldapOptions,
                IOptions<IdentityOptions> identityOptions)
                where TOptions : LdapOptionsBase {
            ArgumentNullException.ThrowIfNull(ldapOptions?.Value,
                nameof(ldapOptions));
            ArgumentNullException.ThrowIfNull(identityOptions?.Value,
                nameof(identityOptions));

            var o = identityOptions.Value.ClaimsIdentity;
            ArgumentNullException.ThrowIfNull(o, nameof(identityOptions));

            return ldapOptions.Value.Schema switch {
                Schema.ActiveDirectory => (b, _) => Adds.MapUserClaims(b, o),
                Schema.IdentityManagementForUnix => (b, _) => Idmu.MapUserClaims(b, o),
                Schema.Rfc2307 => (b, _) => Rfc2307.MapUserClaims(b, o),
                _ => throw new ArgumentException(string.Format(
                    Resources.ErrorSchemaNotWellKnown,
                    ldapOptions.Value.Schema))
            };
        }
        #endregion
    }
}
