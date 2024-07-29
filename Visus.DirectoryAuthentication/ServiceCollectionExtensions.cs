// <copyright file="ServiceCollectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.DirectoryServices.Protocols;
using System.Runtime.CompilerServices;
using Visus.DirectoryAuthentication.Claims;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Mapping;
using Visus.DirectoryAuthentication.Services;
using Visus.Ldap;
using Visus.Ldap.Claims;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication {

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
        /// <returns><paramref name="services"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="services"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapAuthentication<
                TUser, TGroup,
                TLdapMapper, TUserMap, TGroupMap,
                TClaimsBuilder, TClaimsMapper, TUserClaimsMap, TGroupClaimsMap>(
                this IServiceCollection services,
                Action<LdapOptions> options)
                where TUser : class, new()
                where TGroup : class, new()
                where TLdapMapper : class, ILdapMapper<SearchResultEntry, TUser, TGroup>
                where TUserMap : class, ILdapAttributeMap<TUser>
                where TGroupMap : class, ILdapAttributeMap<TGroup>
                where TClaimsBuilder : class, IClaimsBuilder<TUser, TGroup>
                where TClaimsMapper : class, IClaimsMapper<SearchResultEntry>
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

            return services
                .AddSingleton<IClaimsBuilder<TUser, TGroup> , TClaimsBuilder>()
                .AddSingleton<IClaimsMapper<SearchResultEntry>, TClaimsMapper>()
                .AddSingleton<IGroupClaimsMap, TGroupClaimsMap>()
                .AddSingleton<IUserClaimsMap, TUserClaimsMap>()
                .AddSingleton<ILdapMapper<SearchResultEntry, TUser, TGroup>, TLdapMapper>()
                .AddSingleton<ILdapAttributeMap<TUser>, TUserMap>()
                .AddSingleton<ILdapAttributeMap<TGroup>, TGroupMap>()
                .AddSingleton<ILdapConnectionService, LdapConnectionService>()
                .AddScoped<ILdapAuthenticationService<TUser>, LdapAuthenticationService<TUser, TGroup>>()
                .AddScoped<ILdapSearchService<TUser, TGroup>, LdapSearchService<TUser, TGroup>>();
        }

        /// <summary>
        /// Adds <see cref="ILdapAuthenticationService{TUser}"/>,
        /// <see cref="ILdapConnectionService"/> and
        /// <see cref="ILdapSearchService{TUser, TGroup}"/> to the dependency
        /// injection container and configures <see cref="LdapOptions"/>.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TGroup"></typeparam>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddLdapAuthentication<TUser, TGroup>(
                this IServiceCollection services, Action<LdapOptions> options)
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
                ClaimsMap<TGroup>>(options);

        /// <summary>
        /// Adds <see cref="ILdapAuthenticationService{TUser}"/>,
        /// <see cref="ILdapConnectionService"/> and
        /// <see cref="ILdapSearchService{TUser, TGroup}"/> using the default
        /// <see cref="LdapUser"/> and <see cref="LdapGroup"/> representations
        /// to the dependency injection container and configures
        /// <see cref="LdapOptions"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddLdapAuthentication(
                this IServiceCollection services, Action<LdapOptions> options)
            => services.AddLdapAuthentication<LdapUser, LdapGroup>(options);
    }
}
