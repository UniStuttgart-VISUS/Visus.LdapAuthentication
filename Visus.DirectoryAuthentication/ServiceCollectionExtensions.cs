// <copyright file="ServiceCollectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.DependencyInjection;
using System;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions {

        /// <summary>
        /// Adds an <see cref="ILdapAuthenticationService"/> with a custom user
        /// mapper to the dependency injection container and registers and
        /// configures the <see cref="LdapOptions"/>.
        /// </summary>
        /// <typeparam name="TUser">The type of user to be created for the search
        /// results, which also defines attributes like the unique identity in
        /// combination with the global options from <see cref="ILdapOptions"/>.
        /// </typeparam>
        /// <typeparam name="TGroup"></typeparam>
        /// <typeparam name="TClaims"></typeparam>
        /// <typeparam name="TMapper">The type of the user mapper.</typeparam>
        /// <param name="services">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="services"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="services"/> is <c>null</c>, or if
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapAuthenticationService<
                TUser, TGroup, TClaims, TMapper>(
                this IServiceCollection services,
                Action<LdapOptions> options)
                where TUser : class, new()
                where TGroup : class, new()
                where TClaims: class, IClaimsBuilder<TUser, TGroup>
                where TMapper : class, ILdapMapper<TUser, TGroup> {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = options ?? throw new ArgumentNullException(nameof(options));
            return services.Configure(options)
                .AddSingleton<IClaimsBuilder<TUser, TGroup>, TClaims>()
                .AddSingleton<ILdapMapper<TUser, TGroup>, TMapper>()
                .AddScoped<ILdapAuthenticationService<TUser>,
                    LdapAuthenticationService<TUser, TGroup>>();
        }

        /// <summary>
        /// Adds an <see cref="ILdapAuthenticationService"/> to the dependency
        /// injection container and registers and configures the
        /// <see cref="LdapOptions"/>.
        /// </summary>
        /// <typeparam name="TUser">The type of the user object.</typeparam>
        /// <param name="services">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="services"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="services"/> is <c>null</c>, or if
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapAuthenticationService<
                TUser, TGroup>(
                this IServiceCollection services,
                Action<LdapOptions> options)
                where TUser : class, new()
                where TGroup : class, new() {
            services.AddLdapAuthenticationService<TUser, TGroup,
                ClaimsBuilder<TUser, TGroup>,
                LdapMapper<TUser, TGroup>>(
                options);
            return services;
        }

        /// <summary>
        /// Adds an <see cref="ILdapAuthenticationService"/> to the dependency
        /// injection container.
        /// </summary>
        /// <param name="services">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="services"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="services"/> is <c>null</c>, or if
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapAuthenticationService(
                this IServiceCollection services, Action<LdapOptions> options)
            => services.AddLdapAuthenticationService<LdapUser, LdapGroup>(
                options);

        /// <summary>
        /// Adds an <see cref="ILdapConnectionService"/> to the dependency
        /// injection container.
        /// </summary>
        /// <param name="services">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="services"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="services"/> is <c>null</c>, or if
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapConnectionService(
                this IServiceCollection services,
                Action<LdapOptions> options) {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = options ?? throw new ArgumentNullException(nameof(options));
            return services.Configure(options)
                .AddSingleton<ILdapConnectionService, LdapConnectionService>();
        }

        /// <summary>
        /// Adds an <see cref="ILdapSearchService"/> with a custom user
        /// mapper to the dependency injection container.
        /// </summary>
        /// <typeparam name="TMapper">The type of the user mapper.</typeparam>
        /// <typeparam name="TUser">The type of user to be created for the search
        /// results, which also defines attributes like the unique identity in
        /// combination with the global options from <see cref="ILdapOptions"/>.
        /// </typeparam>
        /// <param name="services">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="services"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="services"/> is <c>null</c>, or if
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapSearchService<TUser, TGroup,
                TMapper>(
                this IServiceCollection services,
                Action<LdapOptions> options)
                where TUser : class, new()
                where TGroup : class, new()
                where TMapper : class, ILdapMapper<TUser, TGroup> {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = options ?? throw new ArgumentNullException(nameof(options));
            return services.Configure(options)
                .AddSingleton<ILdapMapper<TUser, TGroup>,TMapper>()
                .AddScoped<ILdapSearchService<TUser, TGroup>,
                    LdapSearchService<TUser, TGroup>>();
        }

        /// <summary>
        /// Adds an <see cref="ILdapSearchService"/> to the dependency injection
        /// container.
        /// </summary>
        /// <typeparam name="TUser">The type of user to be created for the search
        /// results, which also defines attributes like the unique identity in
        /// combination with the global options from <see cref="ILdapOptions"/>.
        /// </typeparam>
        /// <param name="services">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="services"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="services"/> is <c>null</c>, or if
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapSearchService<TUser, TGroup>(
                this IServiceCollection services,
                Action<LdapOptions> options)
                where TUser : class, new()
                where TGroup : class, new()
            =>  services.AddLdapSearchService<TUser, TGroup,
                LdapMapper<TUser, TGroup>>(options);

        /// <summary>
        /// Adds an <see cref="ILdapSearchService"/> to the dependency injection
        /// container.
        /// </summary>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        public static IServiceCollection AddLdapSearchService(
                this IServiceCollection that,
                Action<LdapOptions> options)
            => that.AddLdapSearchService<LdapUser, LdapGroup>(options);

        /// <summary>
        /// Adds <see cref="ILdapAuthenticationService{TUser}"/>,
        /// <see cref="ILdapConnectionService"/> and
        /// <see cref="ILdapSearchService{TUser, TGroup}"/> to the dependency
        /// injection container.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TGroup"></typeparam>
        /// <typeparam name="TClaims"></typeparam>
        /// <typeparam name="TMapper"></typeparam>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddLdapServices<
                TUser, TGroup, TClaims, TMapper>(
                this IServiceCollection services,
                Action<LdapOptions> options)
                where TUser : class, new()
                where TGroup : class, new()
                where TClaims : class, IClaimsBuilder<TUser, TGroup>
                where TMapper : class, ILdapMapper<TUser, TGroup> {
            _ = services ?? throw new ArgumentNullException(nameof(services));
            _ = options ?? throw new ArgumentNullException(nameof(options));
            return services.Configure(options)
                .AddSingleton<IClaimsBuilder<TUser, TGroup>, TClaims>()
                .AddSingleton<ILdapMapper<TUser, TGroup>, TMapper>()
                .AddScoped<ILdapAuthenticationService<TUser>,
                    LdapAuthenticationService<TUser, TGroup>>()
                .AddScoped<ILdapSearchService<TUser, TGroup>,
                    LdapSearchService<TUser, TGroup>>()
                .AddSingleton<ILdapConnectionService, LdapConnectionService>();
        }

        /// <summary>
        /// Adds <see cref="ILdapAuthenticationService{TUser}"/>,
        /// <see cref="ILdapConnectionService"/> and
        /// <see cref="ILdapSearchService{TUser, TGroup}"/> to the dependency
        /// injection container.
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TGroup"></typeparam>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddLdapServices<
                TUser, TGroup>(
                this IServiceCollection services,
                Action<LdapOptions> options)
                where TUser : class, new()
                where TGroup : class, new()
            =>  services.AddLdapServices<TUser, TGroup,
                ClaimsBuilder<TUser, TGroup>,
                LdapMapper<TUser, TGroup>>(
                options);

        /// <summary>
        /// Adds <see cref="ILdapAuthenticationService{TUser}"/>,
        /// <see cref="ILdapConnectionService"/> and
        /// <see cref="ILdapSearchService{TUser, TGroup}"/> to the dependency
        /// injection container.
        /// </summary>
        /// <param name="services">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="services"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="services"/> is <c>null</c>, or if
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapServices(
                this IServiceCollection services, Action<LdapOptions> options)
            => services.AddLdapServices<LdapUser, LdapGroup>(options);

    }
}
