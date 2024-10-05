// <copyright file="ServiceCollectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using System;
using Visus.Ldap;
using Visus.Ldap.Claims;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;
using Visus.Ldap.Services;
using Visus.LdapAuthentication.Claims;
using Visus.LdapAuthentication.Configuration;
using Visus.LdapAuthentication.Mapping;
using Visus.LdapAuthentication.Services;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions {

        /// <summary>
        /// Adds <see cref="ILdapAuthenticationService{TUser}"/>,
        /// <see cref="ILdapConnectionService"/> and
        /// <see cref="ILdapSearchService{TUser, TGroup}"/> to the dependency
        /// injection container and configures <see cref="LdapOptions"/>.
        /// </summary>
        /// <typeparam name="TUser">The type of user to be created for LDAP
        /// entries of users.</typeparam>
        /// <typeparam name="TGroup">The type of group to created for LDAP
        /// entries of groups.</typeparam>
        /// <typeparam name="TLdapMapper"></typeparam>
        /// <typeparam name="TUserMap"></typeparam>
        /// <typeparam name="TGroupMap"></typeparam>
        /// <typeparam name="TClaimsBuilder"></typeparam>
        /// <typeparam name="TClaimsMapper"></typeparam>
        /// <typeparam name="TUserClaimsMap"></typeparam>
        /// <typeparam name="TGroupClaimsMap"></typeparam>
        /// <param name="services">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <param name="mapUser">If not <c>null</c>, the method will call this
        /// function to build a custom mapping of <typeparamref name="TUser"/>
        /// to LDAP attributes.</param>
        /// <param name="mapGroup">If not <c>null</c>, the method will call this
        /// function to build a custom mapping of <typeparamref name="TGroup"/>
        /// to LDAP attributes.</param>
        /// <param name="mapUserClaims">If not <c>null</c>, the method will call
        /// this function to build a custom mapping to
        /// <typeparamref name="TUser"/> to claim types.</param>
        /// <param name="mapGroupClaims">If not <c>null</c>, the method will call
        /// this function to build a custom mapping to
        /// <typeparamref name="TGroup"/> to claim types.</param>
        /// <returns><paramref name="services"/>.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="services"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapAuthentication<
                TUser, TGroup,
                TLdapMapper, TUserMap, TGroupMap,
                TClaimsBuilder, TClaimsMapper, TUserClaimsMap, TGroupClaimsMap>(
                this IServiceCollection services,
                Action<LdapOptions> options,
                Action<ILdapAttributeMapBuilder<TUser>, LdapOptions>? mapUser = null,
                Action<ILdapAttributeMapBuilder<TGroup>, LdapOptions>? mapGroup = null,
                Action<IClaimsMapBuilder, LdapOptions>? mapUserClaims = null,
                Action<IClaimsMapBuilder, LdapOptions>? mapGroupClaims = null)
                where TUser : class, new()
                where TGroup : class, new()
                where TLdapMapper : class, ILdapMapper<LdapEntry, TUser, TGroup>
                where TUserMap : class, ILdapAttributeMap<TUser>
                where TGroupMap : class, ILdapAttributeMap<TGroup>
                where TClaimsBuilder : class, IClaimsBuilder<TUser, TGroup>
                where TClaimsMapper : class, IClaimsMapper<LdapEntry>
                where TUserClaimsMap : class, IUserClaimsMap
                where TGroupClaimsMap : class, IGroupClaimsMap {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            {
                var b = services.AddOptions<LdapOptions>()
                    .Configure(options)
                    .ValidateOnStart();
                b.Services.AddSingleton<LdapOptionsValidator>();
                b.Services.AddSingleton<IValidateOptions<LdapOptions>,
                    FluentValidateOptions<LdapOptions, LdapOptionsValidator>>();
            }

            // If a callback for a custom user map was installed, create a
            // builder and obtain the mapping, but only register it if nothing
            // has been registered before.
            if (mapUser != null) {
                services.TryAddSingleton<ILdapAttributeMap<TUser>>(s => {
                    var o = s.GetRequiredService<IOptions<LdapOptions>>();
                    return new LdapAttributeMap<TUser>(mapUser, o);
                });
            }

            // If a callback for a custom group map was installed, create a
            // builder and obtain the mapping, but only register it if nothing
            // has been registered before.
            if (mapGroup != null) {
                services.TryAddSingleton<ILdapAttributeMap<TGroup>>(s => {
                    var o = s.GetRequiredService<IOptions<LdapOptions>>();
                    return new LdapAttributeMap<TGroup>(mapGroup, o);
                });
            }

            // If a callback for custom user claims was isntalled, create a
            // builder and obtain the mapping, but only register it if nothing
            // has been registered before.
            if (mapUserClaims != null) {
                services.TryAddSingleton<IUserClaimsMap>(s => {
                    var o = s.GetRequiredService<IOptions<LdapOptions>>();
                    return new ClaimsMap<TUser>(mapUserClaims, o);
                });
            }

            // If a callback for custom user claims was isntalled, create a
            // builder and obtain the mapping, but only register it if nothing
            // has been registered before.
            if (mapGroupClaims != null) {
                services.TryAddSingleton<IGroupClaimsMap>(s => {
                    var o = s.GetRequiredService<IOptions<LdapOptions>>();
                    return new ClaimsMap<TGroup>(mapGroupClaims, o);
                });
            }

            // The following maps are only installed if the user has not provided
            // a custom implementation before.
            services.TryAddSingleton<IClaimsBuilder<TUser, TGroup>, TClaimsBuilder>();
            services.TryAddSingleton<IClaimsMapper<LdapEntry>, TClaimsMapper>();
            services.TryAddSingleton<IGroupClaimsMap, TGroupClaimsMap>();
            services.TryAddSingleton<IUserClaimsMap, TUserClaimsMap>();
            services.TryAddSingleton<ILdapMapper<LdapEntry, TUser, TGroup>, TLdapMapper>();
            services.TryAddSingleton<ILdapAttributeMap<TUser>, TUserMap>();
            services.TryAddSingleton<ILdapAttributeMap<TGroup>, TGroupMap>();

            // Try adding the connection service, which allows users to register
            // multiple users and group types. This would otherwise conflict as
            // the connection service is not typed.
            services.TryAddSingleton<ILdapConnectionService, LdapConnectionService>();

            // Add the in-memory caches.
            services.AddMemoryCache();
            services.TryAddSingleton<ILdapCache, LdapCacheService>();

            return services.AddScoped<ILdapAuthenticationService<TUser>,
                    LdapAuthenticationService<TUser, TGroup>>()
                .AddScoped<ILdapSearchService<TUser, TGroup>,
                    LdapSearchService<TUser, TGroup>>();
        }

        /// <summary>
        /// Adds <see cref="ILdapAuthenticationService{TUser}"/>,
        /// <see cref="ILdapConnectionService"/> and
        /// <see cref="ILdapSearchService{TUser, TGroup}"/> to the dependency
        /// injection container and configures <see cref="LdapOptions"/>.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TGroup"></typeparam>
        /// <param name="services">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <param name="mapUser">If not <c>null</c>, the method will call this
        /// function to build a custom mapping of <typeparamref name="TUser"/>
        /// to LDAP attributes.</param>
        /// <param name="mapGroup">If not <c>null</c>, the method will call this
        /// function to build a custom mapping of <typeparamref name="TGroup"/>
        /// to LDAP attributes.</param>
        /// <param name="mapUserClaims">If not <c>null</c>, the method will call
        /// this function to build a custom mapping to
        /// <typeparamref name="TUser"/> to claim types.</param>
        /// <param name="mapGroupClaims">If not <c>null</c>, the method will call
        /// this function to build a custom mapping to
        /// <typeparamref name="TGroup"/> to claim types.</param>
        /// <returns><paramref name="services"/>.</returns>
        public static IServiceCollection AddLdapAuthentication<TUser, TGroup>(
                this IServiceCollection services,
                Action<LdapOptions> options,
                Action<ILdapAttributeMapBuilder<TUser>, LdapOptions>? mapUser = null,
                Action<ILdapAttributeMapBuilder<TGroup>, LdapOptions>? mapGroup = null,
                Action<IClaimsMapBuilder, LdapOptions>? mapUserClaims = null,
                Action<IClaimsMapBuilder, LdapOptions>? mapGroupClaims = null)
                where TUser : class, new()
                where TGroup : class, new()
            => services.AddLdapAuthentication<TUser,
                TGroup,
                LdapMapper<TUser, TGroup>,
                LdapAttributeMap<TUser>,
                LdapAttributeMap<TGroup>,
                ClaimsBuilder<TUser, TGroup>,
                ClaimsMapper,
                ClaimsMap<TUser>,
                ClaimsMap<TGroup>>(options,
                mapUser, mapGroup,
                mapUserClaims, mapGroupClaims);

        /// <summary>
        /// Adds <see cref="ILdapAuthenticationService{TUser}"/>,
        /// <see cref="ILdapConnectionService"/> and
        /// <see cref="ILdapSearchService{TUser, TGroup}"/> using the default
        /// <see cref="LdapUser"/> and <see cref="LdapGroup"/> representations
        /// to the dependency injection container and configures
        /// <see cref="LdapOptions"/>.
        /// </summary>
        /// <param name="services">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <param name="mapUser">If not <c>null</c>, the method will call this
        /// function to build a custom mapping of <typeparamref name="TUser"/>
        /// to LDAP attributes.</param>
        /// <param name="mapGroup">If not <c>null</c>, the method will call this
        /// function to build a custom mapping of <typeparamref name="TGroup"/>
        /// to LDAP attributes.</param>
        /// <param name="mapUserClaims">If not <c>null</c>, the method will call
        /// this function to build a custom mapping to
        /// <typeparamref name="TUser"/> to claim types.</param>
        /// <param name="mapGroupClaims">If not <c>null</c>, the method will call
        /// this function to build a custom mapping to
        /// <typeparamref name="TGroup"/> to claim types.</param>
        /// <returns><paramref name="services"/>.</returns>
        public static IServiceCollection AddLdapAuthentication(
                this IServiceCollection services,
                Action<LdapOptions> options,
                Action<ILdapAttributeMapBuilder<LdapUser>, LdapOptions>? mapUser = null,
                Action<ILdapAttributeMapBuilder<LdapGroup>, LdapOptions>? mapGroup = null,
                Action<IClaimsMapBuilder, LdapOptions>? mapUserClaims = null,
                Action<IClaimsMapBuilder, LdapOptions>? mapGroupClaims = null)
            => services.AddLdapAuthentication<LdapUser, LdapGroup>(options,
                mapUser, mapGroup, mapUserClaims, mapGroupClaims);

    }
}
