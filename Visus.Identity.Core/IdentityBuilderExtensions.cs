// <copyright file="IdentityBuilderExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using System;
using Visus.Identity.Managers;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// Extension methods for <see cref="IdentityBuilder"/>.
    /// </summary>
    public static class IdentityBuilderExtensions {

        /// <summary>
        /// Adds a <see cref="UserManager{TUser}"/> that uses LDAP bind to
        /// implement
        /// <see cref="UserManager{TUser}.CheckPasswordAsync(TUser, string)"/>.
        /// </summary>
        /// <typeparam name="TUser">The type of the user to inject the manager
        /// for.</typeparam>
        /// <param name="builder">The identity builder to inject the user
        /// manager into.</param>
        /// <returns><paramref name="builder"/>.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="builder"/> is <c>null</c>.</exception>
        public static IdentityBuilder AddLdapUserManager<TUser>(
                this IdentityBuilder builder)
                where TUser : class, new() {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            return builder.AddUserManager<LdapUserManager<TUser>>();
        }
    }
}
