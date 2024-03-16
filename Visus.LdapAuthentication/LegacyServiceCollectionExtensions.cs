// <copyright file="ServiceCollectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/> for backwards
    /// compatibility.
    /// </summary>
    /// <remarks>
    /// The extension methods defined in this class should not be used for new
    /// projects. They are only here to provide backwards compatibility for
    /// existing software.
    /// </remarks>
    public static class LegacyServiceCollectionExtensions {

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
        [Obsolete("This method will be removed in future versions of the "
            + "library. Inject LDAP features using the configuration "
            + "action-based methods in ServiceCollectionExtensions.")]
        public static IServiceCollection AddLdapAuthenticationService<TUser>(
                this IServiceCollection that, IOptions options)
                where TUser : class, ILdapUser, new() {
            _ = options ?? throw new ArgumentNullException(nameof(options));
            return that.AddLdapAuthenticationService<TUser>(o => {
                o.AssignFrom(options);
            });
        }

        /// <summary>
        /// Adds an <see cref="ILdapAuthenticationService"/> to the dependency
        /// injection container.
        /// </summary>
        /// <remarks>
        /// This variant only works if the <see cref="IOptions"/> have been
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
        [Obsolete("This method will be removed in future versions of the "
            + "library. Inject LDAP features using the configuration "
            + "action-based methods in ServiceCollectionExtensions.")]
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
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <param name="options">The LDAP options to be used for the connection
        /// to the directory server.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        [Obsolete("This method will be removed in future versions of the "
            + "library. Inject LDAP features using the configuration "
            + "action-based methods in ServiceCollectionExtensions.")]
        public static IServiceCollection AddLdapAuthenticationService(
                this IServiceCollection that, IOptions options) {
            return that.AddLdapAuthenticationService<LdapUser>(options);
        }

        /// <summary>
        /// Adds an <see cref="ILdapAuthenticationService"/> to the dependency
        /// injection container.
        /// </summary>
        /// <remarks>
        /// This variant only works if the <see cref="IOptions"/> have been
        /// registered in the container as well, for instance using
        /// <see cref="AddLdapOptions"/>.
        /// </remarks>
        /// <param name="that">The service collection to add the service to.
        /// </param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        [Obsolete("This method will be removed in future versions of the "
            + "library. Inject LDAP features using the configuration "
            + "action-based methods in ServiceCollectionExtensions.")]
        public static IServiceCollection AddLdapAuthenticationService(
                this IServiceCollection that) {
            return that.AddLdapAuthenticationService<LdapUser>();
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
        [Obsolete("This method will be removed in future versions of the "
            + "library. Inject LDAP features using the configuration "
            + "action-based methods in ServiceCollectionExtensions.")]
        public static IServiceCollection AddLdapConnectionService(
                this IServiceCollection that, IOptions options) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = options ?? throw new ArgumentNullException(nameof(options));
            return that.AddLdapConnectionService(o => {
                o.AssignFrom(options);
            });
        }

        /// <summary>
        /// Adds the given <see cref="IConfigurationSection"/> as
        /// <see cref="LdapOptions"/>.
        /// </summary>
        /// <remarks>
        /// You can resolve this configuration object as
        /// <see cref="IOptions{LdapOptions}"/>, but not as
        /// <see cref="IOptions"/>. The default implementations of
        /// <see cref="ILdapAuthenticationService"/>,
        /// <see cref="ILdapConnectionService"/> and
        /// <see cref="ILdapSearchService"/> and their strongly typed
        /// counterparts have a constructor that supports this.
        /// </remarks>
        /// <param name="that">The service collection to add the options to.
        /// </param>
        /// <param name="section">The configuration section mapped to the LDAP
        /// options.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="section"/> is <c>null</c>.</exception>
        [Obsolete("This method will be removed in future versions of the "
            + "library. Inject LDAP features using the configuration "
            + "action-based methods in ServiceCollectionExtensions.")]
        public static IServiceCollection AddLdapOptions(
                this IServiceCollection that, IConfigurationSection section) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = section ?? throw new ArgumentNullException(nameof(section));
            return that.Configure<LdapOptions>(section);
        }

        /// <summary>
        /// Adds the given specified <paramref name="section"/> of the
        /// <see cref="IConfiguration"/> as <see cref="LdapOptions"/>.
        /// </summary>
        /// <remarks>
        /// You can resolve this configuration object as
        /// <see cref="IOptions{LdapOptions}"/>, but not as
        /// <see cref="IOptions"/>. The default implementations of
        /// <see cref="ILdapAuthenticationService"/>,
        /// <see cref="ILdapConnectionService"/> and
        /// <see cref="ILdapSearchService"/> and their strongly typed
        /// counterparts have a constructor that supports this.
        /// </remarks>
        /// <param name="that">The service collection to add the options to.
        /// </param>
        /// <param name="configuration">The configuration holding the LDAP
        /// section.</param>
        /// <param name="section">The name of the section holding the LDAP
        /// options. This parameter defaults to &quot;LdapOptions&quot;.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="configuration"/> is <c>null</c>.</exception>
        [Obsolete("This method will be removed in future versions of the "
            + "library. Inject LDAP features using the configuration "
            + "action-based methods in ServiceCollectionExtensions.")]
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
        /// Adds the given <see cref="IConfigurationSection"/> as
        /// <see cref="LdapOptions"/>.
        /// </summary>
        /// <remarks>
        /// Note that this method registers the options as a service of type
        /// <see cref="IOptions"/> and as <see cref="IOptions{TOptions}" />,
        /// wherefore <typeparamref name="TOptions"/> cannot be the built-in
        /// <see cref="LdapOptions"/>.
        /// </remarks>
        /// <typeparam name="TOptions">The type of the custom LDAP options,
        /// which must be derived from <see cref="IOptions"/> and can
        /// be resolved as such.</typeparam>
        /// <param name="that">The service collection to add the options to.
        /// </param>
        /// <param name="section">The configuration section mapped to the LDAP
        /// options.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="section"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If
        /// <typeparamref name="TOptions"/> is the built-in type
        /// <see cref="LdapOptions"/>. The reason for that is that this method
        /// registers the options as a service of type
        /// <see cref="IOptions"/> and as the options itself, which would
        /// cause a ambiguous constructor call on the default implementations
        /// of the LDAP services.</exception>
        [Obsolete("This method will be removed in future versions of the "
            + "library. Inject LDAP features using the configuration "
            + "action-based methods in ServiceCollectionExtensions.")]
        public static IServiceCollection AddLdapOptions<TOptions>(
                this IServiceCollection that, IConfigurationSection section)
                where TOptions : class, IOptions, new() {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = section ?? throw new ArgumentNullException(nameof(section));

            if (typeof(TOptions) == typeof(LdapOptions)) {
                throw new InvalidOperationException(
                    Properties.Resources.ErrorLdapOptionsRegistration);
            }

            return that.Configure<TOptions>(section)
                .AddScoped<IOptions>(s => {
                    // Note: we cannot use Configure on an interface, so we add
                    // a factory that retrieves the previously added instance
                    // and returns this instead.
                    var options = s.GetService<IOptions<TOptions>>();
                    return options.Value;
                });
        }

        /// <summary>
        /// Adds the given specified <paramref name="section"/> of the
        /// <see cref="IConfiguration"/> as <see cref="LdapOptions"/>.
        /// </summary>
        /// <remarks>
        /// Note that this method registers the options as a service of type
        /// <see cref="IOptions"/> and as <see cref="IOptions{TOptions}" />,
        /// wherefore <typeparamref name="TOptions"/> cannot be the built-in
        /// <see cref="LdapOptions"/>.
        /// </remarks>
        /// <typeparam name="TOptions">The type of the custom LDAP options,
        /// which must be derived from <see cref="IOptions"/> and can
        /// be resolved as such.</typeparam>
        /// <param name="configuration">The configuration holding the LDAP
        /// section.</param>
        /// <param name="section">The name of the section holding the LDAP
        /// options. This parameter defaults to &quot;LdapOptions&quot;.</param>
        /// <returns><paramref name="that"/> after injection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="configuration"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If
        /// <typeparamref name="TOptions"/> is the built-in type
        /// <see cref="LdapOptions"/>. The reason for that is that this method
        /// registers the options as a service of type
        /// <see cref="IOptions"/> and as the options itself, which would
        /// cause a ambiguous constructor call on the default implementations
        /// of the LDAP services.</exception>
        [Obsolete("This method will be removed in future versions of the "
            + "library. Inject LDAP features using the configuration "
            + "action-based methods in ServiceCollectionExtensions.")]
        public static IServiceCollection AddLdapOptions<TOptions>(
                this IServiceCollection that,
                IConfiguration configuration,
                string section = "LdapOptions")
                where TOptions : class, IOptions, new() {
            _ = that
                ?? throw new ArgumentNullException(nameof(that));
            _ = configuration
                ?? throw new ArgumentNullException(nameof(configuration));
            var s = configuration.GetSection(section ?? "LdapOptions");
            return that.AddLdapOptions<TOptions>(s);
        }

        /// <summary>
        /// Adds an <see cref="ILdapSearchService"/> to the dependency injection
        /// container.
        /// </summary>
        /// <typeparam name="TUser">The type of user to be created for the search
        /// results, which also defines attributes like the unique identity in
        /// combination with the global options from <see cref="IOptions"/>.
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
        [Obsolete("This method will be removed in future versions of the "
            + "library. Inject LDAP features using the configuration "
            + "action-based methods in ServiceCollectionExtensions.")]
        public static IServiceCollection AddLdapSearchService<TUser>(
                this IServiceCollection that, IOptions options)
                where TUser : class, ILdapUser, new() {
            _ = options ?? throw new ArgumentNullException(nameof(options));
            return that.AddLdapSearchService<TUser>(o => {
                o.AssignFrom(options);
            });
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
        [Obsolete("This method will be removed in future versions of the "
            + "library. Inject LDAP features using the configuration "
            + "action-based methods in ServiceCollectionExtensions.")]
        public static IServiceCollection AddLdapSearchService(
                this IServiceCollection that, IOptions options) {
            return that.AddLdapSearchService<LdapUser>(options);
        }

        #region Private methods
        /// <summary>
        /// Copies the options from <paramref name="source"/> to
        /// <paramref name="that"/>.
        /// </summary>
        /// <param name="that"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private static LdapOptions AssignFrom(this LdapOptions that,
                IOptions source) {
            Debug.Assert(that != null);
            Debug.Assert(source != null);
            that.DefaultDomain = source.DefaultDomain;
            that.IsNoCertificateCheck = source.IsNoCertificateCheck;
            that.IsRecursiveGroupMembership = source.IsRecursiveGroupMembership;
            that.IsSsl = source.IsSsl;
            that.Mapping = source.Mapping;
            that.Mappings = source.Mappings;
            that.PageSize = source.PageSize;
            that.Password = source.Password;
            that.Port = source.Port;
            that.RootCaThumbprint = source.RootCaThumbprint;
            that.Schema = source.Schema;
            that.SearchBases = source.SearchBases;
            that.Server = source.Server;
            that.ServerThumbprint = source.ServerThumbprint;
            that.Timeout = source.Timeout;
            that.User = source.User;
            return that;
        }
        #endregion
    }
}
