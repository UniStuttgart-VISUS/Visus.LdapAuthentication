// <copyright file="LdapConnectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Extension methods for <see cref="LdapConnection"/>.
    /// </summary>
    internal static class LdapConnectionExtensions {

        /// <summary>
        /// Gets the default naming context of the server which
        /// <paramref name="that"/> is connected to.
        /// </summary>
        /// <remarks>
        /// The default naming context is obtained from the root DSE of the
        /// given connection. It can be used as the fallback for search bases
        /// as it usually designates the root location of the directory.
        /// </remarks>
        /// <param name="that">The connection to get the default naming context
        /// of.</param>
        /// <returns>The default naming context or <c>null</c>.</returns>
        public static string GetDefaultNamingContext(this LdapConnection that) {
            var rootDse = that.GetRootDse("defaultNamingContext");
            return rootDse?.GetAttribute("defaultNamingContext")?.ToString();
        }

        /// <summary>
        /// Gets the root DSE for the given connection.
        /// </summary>
        /// <param name="that">The connection from which to get the root DSE.
        /// </param>
        /// <param name="attributes">An optional list of attributes to load
        /// for the root DSE.</param>
        /// <returns>The entry of the root DSE.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        public static LdapEntry GetRootDse(this LdapConnection that,
                params string[] attributes) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            // Cf. https://stackoverflow.com/questions/19696753/how-does-one-connect-to-the-rootdse-and-or-retrieve-highestcommittedusn-with-sys
            var result = that.Search(string.Empty,
                SearchScope.Base,
                "(objectClass=*)",
                attributes,
                false);

            if (result.HasMore()) {
                var retval = result.NextEntry();
                if (retval != null) {
                    return retval;
                }
            }

            return null;
        }

        /// <summary>
        /// Performs an LDAP search while converting our strongly typed
        /// <see cref="SearchScope"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="that"></param>
        /// <param name="base"></param>
        /// <param name="scope"></param>
        /// <param name="filter"></param>
        /// <param name="attrs"></param>
        /// <param name="typesOnly"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILdapSearchResults Search(this LdapConnection that,
                string @base,
                SearchScope scope,
                string filter,
                string[] attrs,
                bool typesOnly = false) {
            return that.Search(@base, (int) scope, filter, attrs, typesOnly);
        }

        /// <summary>
        /// Performs an LDAP search while converting our strongly typed
        /// <see cref="SearchScope"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="that"></param>
        /// <param name="base"></param>
        /// <param name="filter"></param>
        /// <param name="attrs"></param>
        /// <param name="typesOnly"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILdapSearchResults Search(this LdapConnection that,
                KeyValuePair<string, SearchScope> @base,
                string filter,
                string[] attrs,
                bool typesOnly = false) {
            return that.Search(@base.Key, (int) @base.Value, filter, attrs,
                typesOnly);
        }

        /// <summary>
        /// Performs an LDAP search while converting our strongly typed
        /// <see cref="SearchScope"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="that"></param>
        /// <param name="base"></param>
        /// <param name="scope"></param>
        /// <param name="filter"></param>
        /// <param name="attrs"></param>
        /// <param name="typesOnly"></param>
        /// <param name="constraints"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILdapSearchResults Search(this LdapConnection that,
                string @base,
                SearchScope scope,
                string filter,
                string[] attrs,
                bool typesOnly,
                LdapSearchConstraints constraints) {
            return that.Search(@base, (int) scope, filter, attrs, typesOnly,
                constraints);
        }
    }
}
