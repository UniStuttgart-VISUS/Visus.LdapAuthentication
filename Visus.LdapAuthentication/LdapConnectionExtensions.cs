// <copyright file="LdapConnectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Extension methods for <see cref="LdapConnection"/>.
    /// </summary>
    internal static class LdapConnectionExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILdapSearchResults Search(this LdapConnection that,
                string @base,
                SearchScope scope,
                string filter,
                string[] attrs,
                bool typesOnly = false) {
            return that.Search(@base, (int) scope, filter, attrs, typesOnly);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILdapSearchResults Search(this LdapConnection that,
                KeyValuePair<string, SearchScope> @base,
                string filter,
                string[] attrs,
                bool typesOnly = false) {
            return that.Search(@base.Key, (int) @base.Value, filter, attrs,
                typesOnly);
        }

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
