// <copyright file="SearchScope.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;


namespace Visus.LdapAuthentication.Configuration {

    /// <summary>
    /// Type-safe defintion of LDAP search scopes.
    /// </summary>
    public enum SearchScope {

        /// <summary>
        /// Only searches the base DN.
        /// </summary>
        Base = LdapConnection.ScopeBase,

        /// <summary>
        /// Only searches the DN below the base DN.
        /// </summary>
        OneLevel = LdapConnection.ScopeOne,

        /// <summary>
        /// Recursively searches the base DN and all of its children.
        /// </summary>
        Subtree = LdapConnection.ScopeSub
    }
}
