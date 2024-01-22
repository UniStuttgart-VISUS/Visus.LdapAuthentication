// <copyright file="IdentityBuilderExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using Visus.DirectoryAuthentication;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// Extension methods for <see cref="IdentityBuilder"/>.
    /// </summary>
    public static class IdentityBuilderExtensions {

        /// <summary>
        /// Adds an <see cref="LdapUserStore{TUser}"/> for the specified type of
        /// user object.
        /// </summary>
        /// <remarks>
        /// <see cref="ILdapOptions"/> must have been registered in the services
        /// collection such that the user store can resolve these.
        /// </remarks>
        /// <typeparam name="TUser">The type of the user object to be used to
        /// represent an identity user.</typeparam>
        /// <param name="that">The builder used to add the store to.</param>
        /// <returns><paramref name="that"/>.</returns>
        public static IdentityBuilder AddLdapStore<TUser>(this IdentityBuilder that)
                where TUser : class, ILdapIdentityUser {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            that.Services.AddScoped<LdapUserStore<TUser>>();
            return that;
        }

        /// <summary>
        /// Adds an <see cref="LdapUserStore{TUser}"/> for the default
        /// <see cref="LdapIdentityUser"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="ILdapOptions"/> must have been registered in the services
        /// collection such that the user store can resolve these.
        /// </remarks>
        /// <param name="that">The builder used to add the store to.</param>
        /// <returns><paramref name="that"/>.</returns>
        public static IdentityBuilder AddLdapStore(this IdentityBuilder that) {
            return that.AddLdapStore<LdapIdentityUser>();
        }
    }
}
