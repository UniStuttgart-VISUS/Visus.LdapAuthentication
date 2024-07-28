// <copyright file="ServiceCollectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.DirectoryServices.Protocols;
using Visus.DirectoryAuthentication;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Services;
using Visus.Ldap.Claims;
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
        /// <typeparam name="TClaimsMap"></typeparam>
        /// <param name="services">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="services"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="services"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapAuthentication<
                TUser, TGroup,
                TLdapMapper, TUserMap, TGroupMap,
                TClaimsBuilder, TClaimsMapper, TClaimsMap>(
                this IServiceCollection services,
                Action<LdapOptions> options)
                where TUser : class, new()
                where TGroup : class, new()
                where TLdapMapper : class, ILdapMapper<SearchResultEntry, TUser, TGroup>
                where TUserMap : class, ILdapAttributeMap<TUser>
                where TGroupMap : class, ILdapAttributeMap<TGroup>
                where TClaimsBuilder : class, IClaimsBuilder<TUser, TGroup>
                where TClaimsMapper : class, IClaimsMapper<SearchResultEntry>
                where TClaimsMap : class, IClaimsMap {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            services.AddOptions<LdapOptions>()
                .Configure(options)
                .ValidateOnStart()
                .Services.AddSingleton<IValidateOptions<LdapOptions>, ValidateLdapOptions>();

            return services
                .AddSingleton<IClaimsBuilder<TUser, TGroup> , TClaimsBuilder>()
                .AddSingleton<IClaimsMapper<SearchResultEntry>, TClaimsMapper>()
                .AddSingleton<IClaimsMap, TClaimsMap>()
                .AddSingleton<ILdapMapper<SearchResultEntry, TUser, TGroup>, TLdapMapper>()
                .AddSingleton<ILdapAttributeMap<TUser>, TUserMap>()
                .AddSingleton<ILdapAttributeMap<TGroup>, TGroupMap>()
                .AddSingleton<ILdapConnectionService, LdapConnectionService>()
                .AddScoped<ILdapAuthenticationService<TUser>, LdapAuthenticationService<TUser, TGroup>>()
                .AddScoped<ILdapSearchService<TUser, TGroup>, LdapSearchService<TUser, TGroup>>();
        }
    }
}
