// <copyright file="IdentityBuilderExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Security.Claims;
using Visus.DirectoryAuthentication;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryIdentity.Properties;
using Visus.DirectoryIdentity.Stores;
using Visus.Identity.Managers;
using Visus.Ldap.Claims;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// Extension methods for <see cref="IdentityBuilder"/>.
    /// </summary>
    public static class IdentityBuilderExtensions {

        /// <summary>
        /// Adds an <see cref="LdapStore{TUser, TRole}"/> for obtaining users
        /// and their roles (groups in the directory) from an LDAP server along
        /// with a <see cref="LdapUserManager{TUser}"/> to authenticate a
        /// <typeparamref name="TUser"/> with an LDAP bind.
        /// </summary>
        /// <typeparam name="TUser">The type of the identity user.</typeparam>
        /// <typeparam name="TRole">The type of the identity role (the groups).
        /// </typeparam>
        /// <param name="builder">The identity builder to add the store to.
        /// </param>
        /// <param name="options">The LDAP options determining the connection to
        /// the LDAP server as well as basic mappings.</param>
        /// <param name="mapUser">A callback for providing the property to
        /// attribute maps for the user object. If this parameter is
        /// <c>null</c>, the map will be build using reflection and annotations
        /// on properties of <typeparamref name="TUser"/>.</param>
        /// <param name="mapRole">A callback for providing the property to
        /// attribute maps for the role object. If this parameter is
        /// <c>null</c>, the map will be build using reflection and annotations
        /// on properties of <typeparamref name="TRole"/>.</param>
        /// <param name="mapUserClaims">A callback to provide custom mappings
        /// from LDAP attributes to claims. If this parameter is <c>null</c>,
        /// the map will be build using reflection an annotation son properties
        /// of <typeparamref name="TUser"/>.</param>
        /// <param name="mapRoleClaims">A callback to provide custom mappings
        /// from LDAP attributes to claims. If this parameter is <c>null</c>,
        /// the map will be build using reflection an annotation son properties
        /// of <typeparamref name="TRole"/>.</param>
        /// <returns><paramref name="builder"/>.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="builder"/> is <c>null</c>.</exception>
        public static IdentityBuilder AddLdapStore<TUser, TRole>(
                this IdentityBuilder builder,
                Action<LdapOptions> options,
                Action<ILdapAttributeMapBuilder<TUser>, LdapOptionsBase>? mapUser = null,
                Action<ILdapAttributeMapBuilder<TRole>, LdapOptionsBase>? mapRole = null,
                Action<IClaimsMapBuilder, LdapOptionsBase>? mapUserClaims = null,
                Action<IClaimsMapBuilder, LdapOptionsBase>? mapRoleClaims = null)
                where TUser : class, new()
                where TRole : class, new() {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            builder.AddLdapUserManager<TUser>();

            builder.Services.AddLdapAuthentication(options,
                mapUser, mapRole,
                mapUserClaims, mapRoleClaims);

            builder.Services
                .AddScoped<IUserStore<TUser>, LdapStore<TUser, TRole>>()
                .AddScoped<IQueryableUserStore<TUser>, LdapStore<TUser, TRole>>()
                .AddScoped<IUserClaimStore<TUser>, LdapStore<TUser, TRole>>()
                .AddScoped<IRoleStore<TRole>, LdapStore<TUser, TRole>>()
                .AddScoped<IQueryableRoleStore<TRole>, LdapStore<TUser, TRole>>()
                .AddScoped<IRoleClaimStore<TRole>, LdapStore<TUser, TRole>>();

            return builder;
        }

        /// <summary>
        /// Adds a store for retrieving <see cref="IdentityUser{TKey}"/>s and
        /// their <see cref="IdentityRole{TKey}"/>s from an LDAP server along
        /// with a <see cref="LdapUserManager{TUser}"/> to authenticate a
        /// <typeparamref name="TUser"/> with an LDAP bind.
        /// </summary>
        /// <typeparam name="TUser">The <see cref="IdentityUser{TKey}"/>-derived
        /// type of the user object.</typeparam>
        /// <typeparam name="TUserKey">The key used for the user object.
        /// </typeparam>
        /// <typeparam name="TRole">The <see cref="IdentityRole{TKey}"/>-derived
        /// type of the role object.</typeparam>
        /// <typeparam name="TRoleKey">The key used for the role object.
        /// </typeparam>
        /// <param name="builder">The identity builder to add the store to.
        /// </param>
        /// <param name="options">The LDAP options determining the connection to
        /// the LDAP server as well as basic mappings.</param>
        /// <param name="mapUser">A callback for providing the property to
        /// attribute maps for the user object. If this parameter is
        /// <c>null</c>, a built-in mapping will be used.</param>
        /// <param name="mapRole">A callback for providing the property to
        /// attribute maps for the role object. If this parameter is
        /// <c>null</c>, a built-in mapping will be used.</param>
        /// <param name="mapUserClaims">A callback to provide custom mappings
        /// from LDAP attributes to claims. If this parameter is <c>null</c>,
        /// a built-in mapping will be used.</param>
        /// <param name="mapRoleClaims">A callback to provide custom mappings
        /// from LDAP attributes to claims. If this parameter is <c>null</c>,
        /// a built-in mapping will be used.</param>
        /// <returns><paramref name="builder"/>.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="builder"/> is <c>null</c>.</exception>
        public static IdentityBuilder AddIdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>(
                this IdentityBuilder builder,
                Action<LdapOptions> options,
                Action<ILdapAttributeMapBuilder<TUser>, LdapOptionsBase>? mapUser = null,
                Action<ILdapAttributeMapBuilder<TRole>, LdapOptionsBase>? mapRole = null,
                Action<IClaimsMapBuilder, LdapOptionsBase>? mapUserClaims = null,
                Action<IClaimsMapBuilder, LdapOptionsBase>? mapRoleClaims = null)
                where TUser : IdentityUser<TUserKey>, new()
                where TRole : IdentityRole<TRoleKey>, new()
                where TUserKey : IEquatable<TUserKey>
                where TRoleKey : IEquatable<TRoleKey> {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            builder.AddLdapUserManager<TUser>();

            builder.Services.AddLdapAuthentication(options,
                mapUser ?? MapWellKnownUser<TUser, TUserKey>,
                mapRole ?? MapWellKnownRole<TRole, TRoleKey>,
                mapUserClaims ?? MapWellKnownUserClaims,
                mapRoleClaims ?? MapWellKnownRoleClaims);

            builder.Services
                .AddScoped<IUserStore<TUser>, IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>>()
                .AddScoped<IQueryableUserStore<TUser>, IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>>()
                .AddScoped<IUserClaimStore<TUser>, IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>>()
                .AddScoped<IUserEmailStore<TUser>, IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>>()
                .AddScoped<IUserLockoutStore<TUser>, IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>>()
                .AddScoped<IUserPhoneNumberStore<TUser>, IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>>()
                .AddScoped<IRoleStore<TRole>, IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>>()
                .AddScoped<IQueryableRoleStore<TRole>, IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>>()
                .AddScoped<IRoleClaimStore<TRole>, IdentityLdapStore<TUser, TUserKey, TRole, TRoleKey>>();

            return builder;
        }

        /// <summary>
        /// Adds a store for retrieving <see cref="IdentityUser"/>s and their
        /// <see cref="IdentityRole"/>s from an LDAP server along
        /// with a <see cref="LdapUserManager{TUser}"/> to authenticate a
        /// <typeparamref name="TUser"/> with an LDAP bind.
        /// </summary>
        /// <param name="builder">The identity builder to add the store to.
        /// </param>
        /// <param name="options">The LDAP options determining the connection to
        /// the LDAP server as well as basic mappings.</param>
        /// <param name="mapUser">A callback for providing the property to
        /// attribute maps for the user object. If this parameter is
        /// <c>null</c>, a built-in mapping will be used.</param>
        /// <param name="mapRole">A callback for providing the property to
        /// attribute maps for the role object. If this parameter is
        /// <c>null</c>, a built-in mapping will be used.</param>
        /// <param name="mapUserClaims">A callback to provide custom mappings
        /// from LDAP attributes to claims. If this parameter is <c>null</c>,
        /// a built-in mapping will be used.</param>
        /// <param name="mapRoleClaims">A callback to provide custom mappings
        /// from LDAP attributes to claims. If this parameter is <c>null</c>,
        /// a built-in mapping will be used.</param>
        /// <returns><paramref name="builder"/>.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="builder"/> is <c>null</c>.</exception>
        public static IdentityBuilder AddIdentityLdapStore(
                this IdentityBuilder builder,
                Action<LdapOptions> options,
                Action<ILdapAttributeMapBuilder<IdentityUser>, LdapOptionsBase>? mapUser = null,
                Action<ILdapAttributeMapBuilder<IdentityRole>, LdapOptionsBase>? mapRole = null,
                Action<IClaimsMapBuilder, LdapOptionsBase>? mapUserClaims = null,
                Action<IClaimsMapBuilder, LdapOptionsBase>? mapRoleClaims = null)
            => builder.AddIdentityLdapStore<IdentityUser, string, IdentityRole, string>(
                options, mapUser, mapRole, mapUserClaims, mapRoleClaims);

        #region Private methods
        /// <summary>
        /// Maps <typeparamref name="TRole"/> to ADDS attributes.
        /// </summary>
        private static void MapAddsRole<TRole, TKey>(
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
        }

        /// <summary>
        /// Maps group attributes to claims in an ADDS schema.
        /// </summary>
        /// <param name="builder"></param>
        private static void MapAddsRoleClaims(
                IClaimsMapBuilder builder) {
            Debug.Assert(builder != null);

            builder.MapAttribute("objectSid")
                .WithConverter<SidConverter>()
                .ToClaims(ClaimTypes.GroupSid);

            builder.MapAttribute("sAMAccountName")
                .ToClaim(ClaimTypes.Role);
        }

        /// <summary>
        /// Maps <typeparamref name="TUser"/> to ADDS attributes.
        /// </summary>
        private static void MapAddsUser<TUser, TKey>(
                ILdapAttributeMapBuilder<TUser> builder)
                where TUser : IdentityUser<TKey>
                where TKey : IEquatable<TKey> {
            Debug.Assert(builder != null);

            builder.MapProperty(nameof(IdentityUser<TKey>.AccessFailedCount))
                .ToAttribute("badPwdCount");

            builder.MapProperty(nameof(IdentityUser<TKey>.Email))
                .ToAttribute("mail");

            builder.MapProperty(nameof(IdentityUser<TKey>.Id))
                .StoringIdentity()
                .ToAttribute("objectSid")
                .WithConverter<SidConverter>();

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
        /// Maps user attributes to claims in an ADDS schema.
        /// </summary>
        private static void MapAddsUserClaims(
                IClaimsMapBuilder builder) {
            Debug.Assert(builder != null);

            builder.MapAttribute("objectSid")
                .WithConverter<SidConverter>()
                .ToClaims(ClaimTypes.NameIdentifier, ClaimTypes.Sid);

            builder.MapAttribute("sAMAccountName")
                .ToClaim(ClaimTypes.Name);
        }

        /// <summary>
        /// Maps <typeparamref name="TRole"/> to IDMU attributes.
        /// </summary>
        private static void MapIdmuRole<TRole, TKey>(
                ILdapAttributeMapBuilder<TRole> builder)
                where TRole : IdentityRole<TKey>
                where TKey : IEquatable<TKey> {
            Debug.Assert(builder != null);

            builder.MapProperty(nameof(IdentityRole<TKey>.Id))
                .StoringIdentity()
                .ToAttribute("gidNumber");

            builder.MapProperty(nameof(IdentityRole<TKey>.Name))
                .StoringAccountName()
                .ToAttribute("sAMAccountName");
        }

        /// <summary>
        /// Maps group attributes to claims in an IDMU schema.
        /// </summary>
        /// <param name="builder"></param>
        private static void MapIdmuRoleClaims(
                IClaimsMapBuilder builder) {
            Debug.Assert(builder != null);

            builder.MapAttribute("objectSid")
                .WithConverter<SidConverter>()
                .ToClaims(ClaimTypes.GroupSid);

            builder.MapAttribute("sAMAccountName")
                .ToClaim(ClaimTypes.Role);
        }

        /// <summary>
        /// Maps <typeparamref name="TUser"/> to IDMU attributes.
        /// </summary>
        private static void MapIdmuUser<TUser, TKey>(
                ILdapAttributeMapBuilder<TUser> builder)
                where TUser : IdentityUser<TKey>
                where TKey : IEquatable<TKey> {
            Debug.Assert(builder != null);

            builder.MapProperty(nameof(IdentityUser<TKey>.AccessFailedCount))
                .ToAttribute("badPwdCount");

            builder.MapProperty(nameof(IdentityUser<TKey>.Email))
                .ToAttribute("mail");

            builder.MapProperty(nameof(IdentityUser<TKey>.Id))
                .StoringIdentity()
                .ToAttribute("uidNumber")
                .WithConverter<SidConverter>();

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
        /// Maps user attributes to claims in an IDMU schema.
        /// </summary>
        private static void MapIdmuUserClaims(
                IClaimsMapBuilder builder) {
            Debug.Assert(builder != null);

            builder.MapAttribute("objectSid")
                .WithConverter<SidConverter>()
                .ToClaim(ClaimTypes.Sid);

            builder.MapAttribute("uidNumber")
                .ToClaim(ClaimTypes.NameIdentifier);

            builder.MapAttribute("sAMAccountName")
                .ToClaim(ClaimTypes.Name);
        }

        /// <summary>
        /// Maps <typeparamref name="TRole"/> to RFC 2307 attributes.
        /// </summary>
        private static void MapRfc2307Role<TRole, TKey>(
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
        /// Makes <typeparamref name="TUser"/> to RFC 2307 attributes.
        /// </summary>
        private static void MapRfc2307User<TUser, TKey>(
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
        /// Maps group attributes to claims in an RFC 2307 schema.
        /// </summary>
        /// <param name="builder"></param>
        private static void MapRfc2307RoleClaims(
                IClaimsMapBuilder builder) {
            Debug.Assert(builder != null);

            builder.MapAttribute("gid")
                .ToClaim(ClaimTypes.Role);
        }

        /// <summary>
        /// Maps user attributes to claims in an RFC 2307 schema.
        /// </summary>
        private static void MapRfc2307UserClaims(
                IClaimsMapBuilder builder) {
            Debug.Assert(builder != null);

            builder.MapAttribute("uidNumber")
                .ToClaim(ClaimTypes.NameIdentifier);

            builder.MapAttribute("uid")
                .ToClaim(ClaimTypes.Name);
        }

        /// <summary>
        /// Creates a mapping for <typeparamref name="TRole"/> for one of
        /// the well-known LDAP schemas.
        /// </summary>
        private static void MapWellKnownRole<TRole, TKey>(
                ILdapAttributeMapBuilder<TRole> builder,
                LdapOptionsBase options)
                where TRole : IdentityRole<TKey>
                where TKey : IEquatable<TKey> {
            switch (options.Schema) {
                case Schema.ActiveDirectory:
                    MapAddsRole<TRole, TKey>(builder);
                    break;

                case Schema.IdentityManagementForUnix:
                    MapIdmuRole<TRole, TKey>(builder);
                    break;

                case Schema.Rfc2307:
                    MapRfc2307Role<TRole, TKey>(builder);
                    break;

                default:
                    throw new ArgumentException(string.Format(
                        Resources.ErrorSchemaNotWellKnown,
                        options.Schema));
            }
        }

        /// <summary>
        /// Maps group claims for one of the well-known LDAP schemas.
        /// </summary>
        private static void MapWellKnownRoleClaims(
                IClaimsMapBuilder builder,
                LdapOptionsBase options) {
            switch (options.Schema) {
                case Schema.ActiveDirectory:
                    MapAddsRoleClaims(builder);
                    break;

                case Schema.IdentityManagementForUnix:
                    MapIdmuRoleClaims(builder);
                    break;

                case Schema.Rfc2307:
                    MapRfc2307RoleClaims(builder);
                    break;

                default:
                    throw new ArgumentException(string.Format(
                        Resources.ErrorSchemaNotWellKnown,
                        options.Schema));
            }
        }

        /// <summary>
        /// Creates a mapping for <typeparamref name="TUser"/> for one of
        /// the well-known LDAP schemas.
        /// </summary>
        private static void MapWellKnownUser<TUser, TKey>(
                ILdapAttributeMapBuilder<TUser> builder,
                LdapOptionsBase options)
                where TUser : IdentityUser<TKey>
                where TKey : IEquatable<TKey> {
            switch (options.Schema) {
                case Schema.ActiveDirectory:
                    MapAddsUser<TUser, TKey>(builder);
                    break;

                case Schema.IdentityManagementForUnix:
                    MapIdmuUser<TUser, TKey>(builder);
                    break;

                case Schema.Rfc2307:
                    MapRfc2307User<TUser, TKey>(builder);
                    break;

                default:
                    throw new ArgumentException(string.Format(
                        Resources.ErrorSchemaNotWellKnown,
                        options.Schema));
            }
        }

        /// <summary>
        /// Maps user claims for one of the well-known LDAP schemas.
        /// </summary>
        private static void MapWellKnownUserClaims(
                IClaimsMapBuilder builder,
                LdapOptionsBase options) {
            switch (options.Schema) {
                case Schema.ActiveDirectory:
                    MapAddsUserClaims(builder);
                    break;

                case Schema.IdentityManagementForUnix:
                    MapIdmuUserClaims(builder);
                    break;

                case Schema.Rfc2307:
                    MapRfc2307UserClaims(builder);
                    break;

                default:
                    throw new ArgumentException(string.Format(
                        Resources.ErrorSchemaNotWellKnown,
                        options.Schema));
            }
        }
        #endregion
    }
}
