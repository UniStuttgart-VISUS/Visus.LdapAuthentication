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
        /// <typeparam name="TMapper">The type of the user mapper.</typeparam>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        public static IServiceCollection AddLdapAuthenticationService<TUser, TMapper>(
                this IServiceCollection that,
                Action<LdapOptions> options)
                where TUser : class, ILdapUser, new()
                where TMapper : class, ILdapUserMapper<TUser> {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = options ?? throw new ArgumentNullException(nameof(options));
            return that.Configure(options)
                .AddSingleton<ILdapUserMapper<TUser>, TMapper>()
                .AddScoped<ILdapAuthenticationService,
                    LdapAuthenticationService<TUser>>()
                .AddScoped<ILdapAuthenticationService<TUser>,
                    LdapAuthenticationService<TUser>>();
        }

        /// <summary>
        /// Adds an <see cref="ILdapAuthenticationService"/> to the dependency
        /// injection container and registers and configures the
        /// <see cref="LdapOptions"/>.
        /// </summary>
        /// <typeparam name="TUser">The type of the user object.</typeparam>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        public static IServiceCollection AddLdapAuthenticationService<TUser>(
                this IServiceCollection that,
                Action<LdapOptions> options)
                where TUser : class, ILdapUser, new() {
            that.AddLdapAuthenticationService<TUser, LdapUserMapper<TUser>>(
                options);
            return that;
        }

        /// <summary>
        /// Adds an <see cref="ILdapAuthenticationService"/> to the dependency
        /// injection container.
        /// </summary>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        public static IServiceCollection AddLdapAuthenticationService(
                this IServiceCollection that, Action<LdapOptions> options) {
            return that.AddLdapAuthenticationService<LdapUser>(options);
        }

        /// <summary>
        /// Adds an <see cref="ILdapConnectionService"/> to the dependency
        /// injection container.
        /// </summary>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        public static IServiceCollection AddLdapConnectionService(
                this IServiceCollection that,
                Action<LdapOptions> options) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = options ?? throw new ArgumentNullException(nameof(options));
            that.Configure(options);
            return that.AddScoped<ILdapConnectionService,
                LdapConnectionService>();
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
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        public static IServiceCollection AddLdapSearchService<TUser, TMapper>(
                this IServiceCollection that,
                Action<LdapOptions> options)
                where TUser : class, ILdapUser, new()
            where TMapper : class, ILdapUserMapper<TUser> {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = options ?? throw new ArgumentNullException(nameof(options));
            return that.Configure(options)
                .AddSingleton<ILdapUserMapper<TUser>,TMapper>()
                .AddScoped<ILdapSearchService,
                    LdapSearchService<TUser>>()
                .AddScoped<ILdapSearchService<TUser>,
                    LdapSearchService<TUser>>();
        }

        /// <summary>
        /// Adds an <see cref="ILdapSearchService"/> to the dependency injection
        /// container.
        /// </summary>
        /// <typeparam name="TUser">The type of user to be created for the search
        /// results, which also defines attributes like the unique identity in
        /// combination with the global options from <see cref="ILdapOptions"/>.
        /// </typeparam>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <param name="options">A callback configuring the options.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        public static IServiceCollection AddLdapSearchService<TUser>(
                this IServiceCollection that,
                Action<LdapOptions> options)
                where TUser : class, ILdapUser, new() {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = options ?? throw new ArgumentNullException(nameof(options));
            that.AddLdapSearchService<TUser, LdapUserMapper<TUser>>(options);
            return that;
        }

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
                Action<LdapOptions> options) {
            return that.AddLdapSearchService<LdapUser>(options);
        }
    }
}
