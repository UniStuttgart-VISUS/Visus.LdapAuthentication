// <copyright file="IdentityBuilderExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Visus.DirectoryAuthentication;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryIdentity.Properties;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// Extension methods for <see cref="IdentityBuilder"/>.
    /// </summary>
    public static class IdentityBuilderExtensions {

        public static IdentityBuilder AddLdapStore<TUser, TRole>(
                this IdentityBuilder builder,
                Action<LdapOptions> options,
                Action<ILdapAttributeMapBuilder<TUser>, LdapOptionsBase>? mapUser = null,
                Action<ILdapAttributeMapBuilder<TRole>, LdapOptionsBase>? mapRole = null)
                where TUser : class, new()
                where TRole : class, new() {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            builder.Services.AddLdapAuthentication(options, mapUser, mapRole)
                .AddScoped<IUserStore<TUser>, LdapStore<TUser, TRole>>()
                .AddScoped<IQueryableUserStore<TUser>, LdapStore<TUser, TRole>>()
                .AddScoped<IRoleStore<TRole>, LdapStore<TUser, TRole>>()
                .AddScoped<IQueryableRoleStore<TRole>, LdapStore<TUser, TRole>>();
            return builder;
        }

        public static IdentityBuilder AddLdapStore(
                this IdentityBuilder builder,
                Action<LdapOptions> options) {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            builder.Services.AddLdapAuthentication<IdentityUser, IdentityRole>(
                options, MapWellKnownSchema, MapWellKnownSchema)
                .AddScoped<IQueryableUserStore<IdentityUser>, LdapUserStore>()
                .AddScoped<IUserClaimStore<IdentityUser>, LdapUserStore>()
                .AddScoped<IUserEmailStore<IdentityUser>, LdapUserStore>()
                .AddScoped<IUserLockoutStore<IdentityUser>, LdapUserStore>()
                .AddScoped<IUserPhoneNumberStore<IdentityUser>, LdapUserStore>()
                .AddScoped<IUserStore<IdentityUser>, LdapUserStore>();
            return builder;
        }

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

        #region Private methods
        /// <summary>
        /// Maps <see cref="IdentityUser"/> to ADDS attributes.
        /// </summary>
        private static void MapActiveDirectory(
                ILdapAttributeMapBuilder<IdentityUser> builder) {
            Debug.Assert(builder != null);

            builder.MapProperty(nameof(IdentityUser.AccessFailedCount))
                .ToAttribute("badPwdCount");

            builder.MapProperty(nameof(IdentityUser.Email))
                .ToAttribute("mail");

            builder.MapProperty(nameof(IdentityUser.Id))
                .StoringIdentity()
                .ToAttribute("objectSid")
                .WithConverter<SidConverter>();

            builder.MapProperty(nameof(IdentityUser.LockoutEnd))
                .ToAttribute("lockoutTime")
                .WithConverter<FileTimeConverter>();

            builder.MapProperty(nameof(IdentityUser.PhoneNumber))
                .ToAttribute("telephoneNumber");

            builder.MapProperty(nameof(IdentityUser.UserName))
                .StoringAccountName()
                .ToAttribute("sAMAccountName");
        }

        /// <summary>
        /// Maps <see cref="IdentityRole"/> to ADDS attributes.
        /// </summary>
        private static void MapActiveDirectory(
                ILdapAttributeMapBuilder<IdentityRole> builder) {
            Debug.Assert(builder != null);

            builder.MapProperty(nameof(IdentityRole.Id))
                .StoringIdentity()
                .ToAttribute("objectSid")
                .WithConverter<SidConverter>();

            builder.MapProperty(nameof(IdentityRole.Name))
                .StoringAccountName()
                .ToAttribute("sAMAccountName");
        }

        /// <summary>
        /// Maps <see cref="IdentityUser"/> to IDMU attributes.
        /// </summary>
        private static void MapIdmu(
                ILdapAttributeMapBuilder<IdentityUser> builder) {
            Debug.Assert(builder != null);

            builder.MapProperty(nameof(IdentityUser.AccessFailedCount))
                .ToAttribute("badPwdCount");

            builder.MapProperty(nameof(IdentityUser.Email))
                .ToAttribute("mail");

            builder.MapProperty(nameof(IdentityUser.Id))
                .StoringIdentity()
                .ToAttribute("uidNumber")
                .WithConverter<SidConverter>();

            builder.MapProperty(nameof(IdentityUser.LockoutEnd))
                .ToAttribute("lockoutTime")
                .WithConverter<FileTimeConverter>();

            builder.MapProperty(nameof(IdentityUser.PhoneNumber))
                .ToAttribute("telephoneNumber");

            builder.MapProperty(nameof(IdentityUser.UserName))
                .StoringAccountName()
                .ToAttribute("sAMAccountName");
        }

        /// <summary>
        /// Maps <see cref="IdentityRole"/> to IDMU attributes.
        /// </summary>
        private static void MapIdmu(
                ILdapAttributeMapBuilder<IdentityRole> builder) {
            Debug.Assert(builder != null);

            builder.MapProperty(nameof(IdentityRole.Id))
                .StoringIdentity()
                .ToAttribute("gidNumber");

            builder.MapProperty(nameof(IdentityRole.Name))
                .StoringAccountName()
                .ToAttribute("sAMAccountName");
        }

        /// <summary>
        /// Makes <see cref="IdentityUser"/> to RFC 2307 attributes.
        /// </summary>
        private static void MapRfc2307(
                ILdapAttributeMapBuilder<IdentityUser> builder) {
            Debug.Assert(builder != null);

            builder.MapProperty(nameof(IdentityUser.Email))
                .ToAttribute("mail");

            builder.MapProperty(nameof(IdentityUser.Id))
                .StoringIdentity()
                .ToAttribute("uidNumber");

            builder.MapProperty(nameof(IdentityUser.PhoneNumber))
                .ToAttribute("telephoneNumber");

            builder.MapProperty(nameof(IdentityUser.UserName))
                .StoringAccountName()
                .ToAttribute("uid");
        }

        /// <summary>
        /// Maps <see cref="IdentityRole"/> to RFC 2307 attributes.
        /// </summary>
        private static void MapRfc2307(
                ILdapAttributeMapBuilder<IdentityRole> builder) {
            Debug.Assert(builder != null);

            builder.MapProperty(nameof(IdentityRole.Id))
                .StoringIdentity()
                .ToAttribute("gidNumber");

            builder.MapProperty(nameof(IdentityRole.Name))
                .StoringAccountName()
                .ToAttribute("gid");
        }

        /// <summary>
        /// Creates a mapping for <see cref="IdentityUser"/> for one of the
        /// well-known LDAP schemas.
        /// </summary>
        private static void MapWellKnownSchema(
                ILdapAttributeMapBuilder<IdentityUser> builder,
                LdapOptionsBase options) {
            switch (options.Schema) {
                case Schema.ActiveDirectory:
                    MapActiveDirectory(builder);
                    break;

                case Schema.IdentityManagementForUnix:
                    MapIdmu(builder);
                    break;

                case Schema.Rfc2307:
                    MapRfc2307(builder);
                    break;

                default:
                    throw new ArgumentException(string.Format(
                        Resources.ErrorSchemaNotWellKnown,
                        options.Schema));
            }
        }

        /// <summary>
        /// Creates a mapping for <see cref="IdentityRole"/> for one of the
        /// well-known LDAP schemas.
        /// </summary>
        private static void MapWellKnownSchema(
                ILdapAttributeMapBuilder<IdentityRole> builder,
                LdapOptionsBase options) {
            switch (options.Schema) {
                case Schema.ActiveDirectory:
                    MapActiveDirectory(builder);
                    break;

                case Schema.IdentityManagementForUnix:
                    MapIdmu(builder);
                    break;

                case Schema.Rfc2307:
                    MapRfc2307(builder);
                    break;

                default:
                    throw new ArgumentException(string.Format(
                        Resources.ErrorSchemaNotWellKnown,
                        options.Schema));
            }
        }
        #endregion
    }
}
