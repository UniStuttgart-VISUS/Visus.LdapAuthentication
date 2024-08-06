// <copyright file="IdentityBuilderExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using Visus.LdapAuthentication;
using Visus.LdapAuthentication.Claims;
using Visus.LdapAuthentication.Configuration;
using Visus.LdapAuthentication.Mapping;
using Visus.LdapIdentity.Stores;
using Visus.Identity;
using Visus.Identity.Managers;
using Visus.Identity.Mapping;
using Visus.Ldap.Claims;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;


namespace Visus.LdapIdentity {

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
                Action<ILdapAttributeMapBuilder<TUser>, LdapOptions>? mapUser = null,
                Action<ILdapAttributeMapBuilder<TRole>, LdapOptions>? mapRole = null,
                Action<IClaimsMapBuilder, LdapOptions>? mapUserClaims = null,
                Action<IClaimsMapBuilder, LdapOptions>? mapRoleClaims = null)
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
                Action<ILdapAttributeMapBuilder<TUser>, LdapOptions>? mapUser = null,
                Action<ILdapAttributeMapBuilder<TRole>, LdapOptions>? mapRole = null,
                Action<IClaimsMapBuilder, LdapOptions>? mapUserClaims = null,
                Action<IClaimsMapBuilder, LdapOptions>? mapRoleClaims = null)
                where TUser : IdentityUser<TUserKey>, new()
                where TRole : IdentityRole<TRoleKey>, new()
                where TUserKey : IEquatable<TUserKey>
                where TRoleKey : IEquatable<TRoleKey> {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            builder.AddLdapUserManager<TUser>();

            builder.Services.AddLdapAuthentication<
                    TUser, TRole,
                    LdapMapper<TUser, TRole>,
                    LdapAttributeMap<TUser>,
                    LdapAttributeMap<TRole>,
                    ClaimsBuilder<TUser, TRole>,
                    ClaimsMapper,
                    IdentityUserClaimsMap<TUser, TUserKey, LdapOptions>,
                    IdentityRoleClaimsMap<TRole, TRoleKey, LdapOptions>>(
                options,
                mapUser ?? WellKnownMappings.MapUser<TUser, TUserKey, LdapOptions>,
                mapRole ?? WellKnownMappings.MapRole<TRole, TRoleKey, LdapOptions>,
                mapUserClaims,
                mapRoleClaims);

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
                Action<ILdapAttributeMapBuilder<IdentityUser>, LdapOptions>? mapUser = null,
                Action<ILdapAttributeMapBuilder<IdentityRole>, LdapOptions>? mapRole = null,
                Action<IClaimsMapBuilder, LdapOptions>? mapUserClaims = null,
                Action<IClaimsMapBuilder, LdapOptions>? mapRoleClaims = null)
            => builder.AddIdentityLdapStore<IdentityUser, string, IdentityRole, string>(
                options, mapUser, mapRole, mapUserClaims, mapRoleClaims);
    }
}
