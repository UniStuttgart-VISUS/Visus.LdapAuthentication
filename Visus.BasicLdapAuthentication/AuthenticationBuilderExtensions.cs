// <copyright file="AuthenticationBuilderExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2026 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;
using Visus.BasicLdapAuthentication.Configuration;
using Visus.Ldap;


namespace Visus.BasicLdapAuthentication {

    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/> to
    /// inject the basic authentication handler and related services.
    /// </summary>
    public static class AuthenticationBuilderExtensions {

        /// <summary>
        /// Adds the basic authentication handler to the given
        /// <see cref="AuthenticationBuilder"/>. The caller is repsonsible for
        /// adding the required
        /// <see cref="Visus.Ldap.ILdapAuthenticationService{TUser}"/>
        /// to the <see cref="IServiceCollection"/> such that the authentication
        /// handler can access it via dependency injection.
        /// </summary>
        /// <typeparam name="TUser">The type of the LDAP user object that is
        /// used to authenticate the user and to determine the claims.
        /// </typeparam>
        /// <param name="builder">The authentication builder to add LDAP-based
        /// basic authentication to.</param>
        /// <param name="scheme">The name of the authentication schema.</param>
        /// <param name="configure">An optional callback to customise the
        /// scheme.</param>
        /// <returns><paramref name="builder"/></returns>
        public static AuthenticationBuilder AddBasicLdapAuthentication<TUser>(
                this AuthenticationBuilder builder,
                string scheme,
                Action<BasicAuthenticationOptions>? configure = null)
                where TUser : class
            => builder.AddScheme<BasicAuthenticationOptions,
                BasicAuthenticationHandler<TUser>>(scheme, configure);

        /// <summary>
        /// Adds the basic authentication handler to the given
        /// <see cref="AuthenticationBuilder"/>. The caller is repsonsible for
        /// adding the required
        /// <see cref="Visus.Ldap.ILdapAuthenticationService{TUser}"/>
        /// to the <see cref="IServiceCollection"/> such that the authentication
        /// handler can access it via dependency injection.
        /// </summary>
        /// <typeparam name="TUser">The type of the LDAP user object that is
        /// used to authenticate the user and to determine the claims.
        /// </typeparam>
        /// <param name="builder">The authentication builder to add LDAP-based
        /// basic authentication to.</param>
        /// <param name="configure">An optional callback to customise the
        /// scheme.</param>
        /// <returns><paramref name="builder"/></returns>
        public static AuthenticationBuilder AddBasicLdapAuthentication<TUser>(
                this AuthenticationBuilder builder,
                Action<BasicAuthenticationOptions>? configure = null)
                where TUser : class
            => builder.AddBasicLdapAuthentication<TUser>(
                BasicAuthenticationDefaults.AuthenticationScheme,
                configure);

        /// <summary>
        /// Adds the basic authentication handler to the given
        /// <see cref="AuthenticationBuilder"/>. The caller is repsonsible for
        /// adding the required
        /// <see cref="Visus.Ldap.ILdapAuthenticationService{Visus.Ldap.LdapUser}"/>
        /// to the <see cref="IServiceCollection"/> such that the authentication
        /// handler can access it via dependency injection.
        /// </summary>
        /// <param name="builder">The authentication builder to add LDAP-based
        /// basic authentication to.</param>
        /// <param name="scheme">The name of the authentication schema.</param>
        /// <param name="configure">An optional callback to customise the
        /// scheme.</param>
        /// <returns><paramref name="builder"/></returns>
        public static AuthenticationBuilder AddBasicLdapAuthentication(
                this AuthenticationBuilder builder,
                string scheme,
                Action<BasicAuthenticationOptions>? configure = null)
            => builder.AddBasicLdapAuthentication<LdapUser>(scheme, configure);

        /// <summary>
        /// Adds the basic authentication handler to the given
        /// <see cref="AuthenticationBuilder"/>. The caller is repsonsible for
        /// adding the required
        /// <see cref="Visus.Ldap.ILdapAuthenticationService{Visus.Ldap.LdapUser}"/>
        /// to the <see cref="IServiceCollection"/> such that the authentication
        /// handler can access it via dependency injection.
        /// </summary>
        /// <param name="builder">The authentication builder to add LDAP-based
        /// basic authentication to.</param>
        /// <param name="configure">An optional callback to customise the
        /// scheme.</param>
        /// <returns><paramref name="builder"/></returns>
        public static AuthenticationBuilder AddBasicLdapAuthentication(
                this AuthenticationBuilder builder,
                Action<BasicAuthenticationOptions>? configure = null)
            => builder.AddBasicLdapAuthentication<LdapUser>(configure);
    }
}
