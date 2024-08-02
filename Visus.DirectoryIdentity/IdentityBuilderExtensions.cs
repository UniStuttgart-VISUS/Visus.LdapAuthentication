// <copyright file="IdentityBuilderExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using Visus.DirectoryAuthentication;
using Visus.DirectoryAuthentication.Configuration;
using Visus.Ldap;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// Extension methods for <see cref="IdentityBuilder"/>.
    /// </summary>
    public static class IdentityBuilderExtensions {

        //public static IdentityBuilder AddLdapStore<TUser>(
        //        this IdentityBuilder that,
        //        Action<LdapOptions> ldapOptions) {
        //    ArgumentNullException.ThrowIfNull(that, nameof(that));
        //    that.Services.AddLdapAuthentication<IdentityUser, IdentityRole>(
        //        ldapOptions, u, => {
        //            u.ForSchema("")

        //        }, g => {

        //        });
        //}

        //    /// <summary>
        //    /// Adds an <see cref="LdapUserStore{TUser}"/> for the specified type of
        //    /// user object.
        //    /// </summary>
        //    /// <remarks>
        //    /// <see cref="ILdapOptions"/> must have been registered in the services
        //    /// collection such that the user store can resolve these. The method
        //    /// will register <see cref="ILdapAuthenticationService"/> and
        //    /// <see cref="ILdapSearchService"/> for <typeparamref name="TUser"/>.
        //    /// </remarks>
        //    /// <typeparam name="TUser">The type of the user object to be used to
        //    /// represent an identity user.</typeparam>
        //    /// <param name="that">The builder used to add the store to.</param>
        //    /// <param name="ldapOptions">A callback to configure the
        //    /// <see cref="LdapOptions"/> used to connect to the directory server.
        //    /// </param>
        //    /// <returns><paramref name="that"/>.</returns>
        //    /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        //    /// is <c>null</c>, or if <paramref name="ldapOptions"/> is <c>null</c>.
        //    /// </exception>
        //    public static IdentityBuilder AddLdapStore<TUser>(this IdentityBuilder that,
        //            Action<LdapOptions> ldapOptions)
        //            where TUser : class, ILdapIdentityUser, new() {
        //        _ = that ?? throw new ArgumentNullException(nameof(that));
        //        that.Services.AddLdapAuthentication<TUser, LdapGroup>(ldapOptions);
        //        that.Services.AddScoped<IPasswordHasher<TUser>,
        //            PasswordHasher<TUser>>();
        //        that.Services.AddScoped<IUserStore<TUser>, LdapUserStore<TUser>>();
        //        return that;
        //    }

        //    /// <summary>
        //    /// Adds an <see cref="LdapUserStore{TUser}"/> for the default
        //    /// <see cref="LdapIdentityUser"/>.
        //    /// </summary>
        //    /// <remarks>
        //    /// <see cref="ILdapOptions"/> must have been registered in the services
        //    /// collection such that the user store can resolve these. Likewise, the
        //    /// caller is responsible for registering both, the
        //    /// <see cref="ILdapAuthenticationService"/> and the
        //    /// <see cref="ILdapSearchService"/>.
        //    /// </remarks>
        //    /// <param name="that">The builder used to add the store to.</param>
        //    /// <param name="ldapOptions">A callback to configure the
        //    /// <see cref="LdapOptions"/> used to connect to the directory server.
        //    /// </param>
        //    /// <returns><paramref name="that"/>.</returns>
        //    /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        //    /// is <c>null</c>, or if <paramref name="ldapOptions"/> is <c>null</c>.
        //    /// </exception>
        //    public static IdentityBuilder AddLdapStore(this IdentityBuilder that,
        //            Action<LdapOptions> ldapOptions) {
        //        //return that.AddLdapStore<LdapIdentityUser, LdapGroup>(ldapOptions);
        //        throw new NotImplementedException();
        //    }
    }
}
