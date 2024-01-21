// <copyright file="ServiceCollectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions {

        /// <summary>
        /// Adds an <see cref="ILdapAuthenticationService"/> to the dependency
        /// injection container.
        /// </summary>
        /// <typeparam name="TUser">The type of the user that is being
        /// authenticated.</typeparam>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <param name="options">The LDAP options to be used for the connection
        /// to the directory server.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapAuthenticationService<TUser>(
                this IServiceCollection that, ILdapOptions options)
                where TUser : class, ILdapUser, new() {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = options ?? throw new ArgumentNullException(nameof(options));

            return that.AddScoped<ILdapAuthenticationService,
                    LdapAuthenticationService<TUser>>(
                s => {
                    var l = s.GetService<ILogger<LdapAuthenticationService<
                        TUser>>>();
                    return new LdapAuthenticationService<TUser>(options, l);
                })

                .AddScoped<ILdapAuthenticationService<TUser>,
                    LdapAuthenticationService<TUser>>(
                s => {
                    var l = s.GetService<ILogger<LdapAuthenticationService<
                        TUser>>>();
                    return new LdapAuthenticationService<TUser>(options, l);
                });
        }

        /// <summary>
        /// Adds an <see cref="ILdapAuthenticationService"/> to the dependency
        /// injection container.
        /// </summary>
        /// <remarks>
        /// This variant only works if the <see cref="ILdapOptions"/> have been
        /// registered in the container as well, for instance using
        /// <see cref="AddLdapOptions"/>.
        /// </remarks>
        /// <typeparam name="TUser">The type of the user that is being
        /// authenticated.</typeparam>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        public static IServiceCollection AddLdapAuthenticationService<TUser>(
                this IServiceCollection that)
                where TUser : class, ILdapUser, new() {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            return that.AddScoped<ILdapAuthenticationService,
                    LdapAuthenticationService<TUser>>()
                .AddScoped<ILdapAuthenticationService<TUser>,
                    LdapAuthenticationService<TUser>>();
        }

        /// <summary>
        /// Adds an <see cref="ILdapAuthenticationService"/> to the dependency
        /// injection container.
        /// </summary>
        /// <remarks>
        /// This variant only works if the <see cref="ILdapOptions"/> have been
        /// registered in the container as well, for instance using
        /// <see cref="AddLdapOptions"/>.
        /// </remarks>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        public static IServiceCollection AddLdapAuthenticationService(
                this IServiceCollection that) {
            return that.AddLdapAuthenticationService<LdapUser>();
        }

        /// <summary>
        /// Adds an <see cref="ILdapAuthenticationService"/> to the dependency
        /// injection container.
        /// </summary>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <param name="options">The LDAP options to be used for the connection
        /// to the directory server.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapAuthenticationService(
                this IServiceCollection that, ILdapOptions options) {
            return that.AddLdapAuthenticationService<LdapUser>(options);
        }

        /// <summary>
        /// Adds an <see cref="ILdapConnectionService"/> to the dependency
        /// injection container.
        /// </summary>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <param name="options">The LDAP options to be used for the connection
        /// to the directory server.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapConnectionService(
                this IServiceCollection that, ILdapOptions options) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = options ?? throw new ArgumentNullException(nameof(options));

            return that.AddScoped<ILdapConnectionService,
                    LdapConnectionService>(s => {
                var l = s.GetService<ILogger<LdapConnectionService>>();
                return new LdapConnectionService(options, l);
            });
        }

        /// <summary>
        /// Adds an <see cref="ILdapConnectionService"/> to the dependency
        /// injection container.
        /// </summary>
        /// <remarks>
        /// This variant only works if the <see cref="ILdapOptions"/> have been
        /// registered in the container as well, for instance using
        /// <see cref="AddLdapOptions"/>.
        /// </remarks>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        public static IServiceCollection AddLdapConnectionService(
                this IServiceCollection that) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            return that.AddScoped<ILdapConnectionService,
                LdapConnectionService>();
        }

        /// <summary>
        /// Adds the given <see cref="IConfigurationSection"/> as
        /// <see cref="LdapOptions"/>.
        /// </summary>
        /// <param name="section">The configuration section mapped to the LDAP
        /// options.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="section"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapOptions(
                this IServiceCollection that, IConfigurationSection section) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = section ?? throw new ArgumentNullException(nameof(section));
            return that.Configure<ILdapOptions>(section)
                .Configure<LdapOptions>(section);
        }

        /// <summary>
        /// Adds the given specified <paramref name="section"/> of the
        /// <see cref="IConfiguration"/> as <see cref="LdapOptions"/>.
        /// </summary>
        /// <param name="configuration">The configuration holding the LDAP
        /// section.</param>
        /// <param name="section">The name of the section holding the LDAP
        /// options. This parameter defaults to &quot;LdapOptions&quot;.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="configuration"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapOptions(
                this IServiceCollection that,
                IConfiguration configuration,
                string section = "LdapOptions") {
            _ = that
                ?? throw new ArgumentNullException(nameof(that));
            _ = configuration
                ?? throw new ArgumentNullException(nameof(configuration));
            var s = configuration.GetSection(section ?? "LdapOptions");
            return that.AddLdapOptions(s);
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
        /// <param name="options">The LDAP options to be used for the connection
        /// to the directory server.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapSearchService<TUser>(
                this IServiceCollection that, ILdapOptions options)
                where TUser : class, ILdapUser, new() {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = options ?? throw new ArgumentNullException(nameof(options));

            return that.AddScoped<ILdapSearchService, LdapSearchService<TUser>>(
                s => {
                    var l = s.GetService<ILogger<LdapSearchService<TUser>>>();
                    return new LdapSearchService<TUser>(options, l);
                })

                .AddScoped<ILdapSearchService<TUser>, LdapSearchService<TUser>>(
                s => {
                    var l = s.GetService<ILogger<LdapSearchService<TUser>>>();
                    return new LdapSearchService<TUser>(options, l);
                });
        }

        /// <summary>
        /// Adds an <see cref="ILdapSearchService"/> to the dependency injection
        /// container.
        /// </summary>
        /// <remarks>
        /// This variant only works if the <see cref="ILdapOptions"/> have been
        /// registered in the container as well, for instance using
        /// <see cref="AddLdapOptions"/>.
        /// </remarks>
        /// <typeparam name="TUser">The type of user to be created for the search
        /// results, which also defines attributes like the unique identity in
        /// combination with the global options from <see cref="ILdapOptions"/>.
        /// </typeparam>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        public static IServiceCollection AddLdapSearchService<TUser>(
                this IServiceCollection that)
                where TUser : class, ILdapUser, new() {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            return that.AddScoped<ILdapSearchService,
                    LdapSearchService<TUser>>()
                .AddScoped<ILdapSearchService<TUser>,
                    LdapSearchService<TUser>>();
        }

        /// <summary>
        /// Adds an <see cref="ILdapSearchService"/> to the dependency injection
        /// container.
        /// </summary>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <param name="options">The LDAP options to be used for the connection
        /// to the directory server.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapSearchService(
                this IServiceCollection that, ILdapOptions options) {
            return that.AddLdapSearchService<LdapUser>(options);
        }

        /// <summary>
        /// Adds an <see cref="ILdapSearchService"/> to the dependency injection
        /// container.
        /// </summary>
        /// <remarks>
        /// This variant only works if the <see cref="ILdapOptions"/> have been
        /// registered in the container as well, for instance using
        /// <see cref="AddLdapOptions"/>.
        /// </remarks>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        public static IServiceCollection AddLdapSearchService(
                this IServiceCollection that) {
            return that.AddLdapSearchService<LdapUser>();
        }
    }
}
