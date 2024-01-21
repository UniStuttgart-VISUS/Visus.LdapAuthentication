// <copyright file="IdentityBuilderExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// Extension methods for <see cref="IdentityBuilder"/>.
    /// </summary>
    public static class IdentityBuilderExtensions {

        public static IdentityBuilder AddLdapStores(this IdentityBuilder that,
                Action<ILdapOptions> configure) {
            _ = that ?? throw new ArgumentNullException(nameof(that));

            that.Services.Configure(configure);

            return that;
        }


        private static void AddLdapStores(IServiceCollection collection,
                Type userType, Type roleType) {
            Debug.Assert(collection != null);

            //var userTypeInfo = userType.GetGenericBaseTypeInfo(
            //    typeof(IdentityUser<>));
            //if (userType == null) {
            //    throw new ArgumentException("The user used for LDAP identity must "
            //        + $"be an instance of {nameof(IdentityUser)}.", nameof(userType));
            //}
            //if ((roleType != null)
            //        && roleType.IsGenericInstance(typeof(IdentityRole<>))) {
            //    throw new ArgumentException("The role used for LDAP identity must "
            //        + $"be an instance of {nameof(IdentityRole)}.", nameof(roleType));
            //}

#if false
            var identityUserType = FindGenericBaseType(userType, typeof(IdentityUser<>));
            if (identityUserType == null)
            {
                throw new InvalidOperationException("AddEntityFrameworkStores can only be called with a user that derives from IdentityUser<TKey>.");
            }

            var keyType = identityUserType.GenericTypeArguments[0];
        
            var userOnlyStoreType = typeof(UserOnlyStore<,>).MakeGenericType(userType, keyType);

            if (roleType != null)   
            {
                var identityRoleType = FindGenericBaseType(roleType, typeof(IdentityRole<>));
                if (identityRoleType == null)
                {
                    throw new InvalidOperationException("AddEntityFrameworkStores can only be called with a role that derives from IdentityRole<TKey>.");
                }

                var userStoreType = typeof(UserStore<,,>).MakeGenericType(userType, roleType, keyType);
                var roleStoreType = typeof(RoleStore<,>).MakeGenericType(roleType, keyType);

                services.TryAddScoped(typeof(UserOnlyStore<,>).MakeGenericType(userType, keyType), provider => CreateStoreInstance(userOnlyStoreType, getDatabase(provider), provider.GetService<IdentityErrorDescriber>()));
                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), provider => userStoreType.GetConstructor(new Type[] { typeof(IDatabase), userOnlyStoreType, typeof(IdentityErrorDescriber) })
                    .Invoke(new object[] { getDatabase(provider), provider.GetService(userOnlyStoreType), provider.GetService<IdentityErrorDescriber>() }));
                services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), provider => CreateStoreInstance(roleStoreType, getDatabase(provider), provider.GetService<IdentityErrorDescriber>()));
            }
            else
            {   // No Roles
                services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), provider => CreateStoreInstance(userOnlyStoreType, getDatabase(provider), provider.GetService<IdentityErrorDescriber>()));
            }
#endif

        }
    }
}
