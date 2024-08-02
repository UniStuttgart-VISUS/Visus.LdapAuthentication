// <copyright file="ServiceCollectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// Extension methods for <see cref="IdentityBuilder"/>.
    /// </summary>
    public static class ServiceCollectionExtensions {

        /// <summary>
        /// Injects an <see cref="LdapUserManager{TUser}"/> as replacement of
        /// the default <see cref="UserManager{TUser}"/> for the specified type
        /// of user.
        /// </summary>
        /// <remarks>
        /// <para>The <see cref="LdapUserManager{TUser}"/> is derived from
        /// <see cref="UserManager{TUser}"/> and only overrides the
        /// <see cref="UserManager{TUser}.CheckPasswordAsync(TUser, string)"/>
        /// method, which it replaces with an LDAP bind.</para>
        /// <para>In order for this method to work, it must be called before
        /// <see cref="IdentityServiceCollectionExtensions.AddIdentityCore{TUser}(IServiceCollection)"/>.
        /// The existing user manager will prevent Identity from registering
        /// its own.</para>
        /// </remarks>
        /// <typeparam name="TUser">The class that is representing an LDAP user.
        /// </typeparam>
        /// <param name="services">The service collection to inject the
        /// user manager to.</param>
        /// <returns><paramref name="services"/>.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="services"/> is <c>null</c>.</exception>
        public static IServiceCollection AddLdapUserManager<TUser>(
                this IServiceCollection services)
                where TUser : class {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            return services.AddScoped<UserManager<TUser>,
                LdapUserManager<TUser>>();
        }
    }
}
