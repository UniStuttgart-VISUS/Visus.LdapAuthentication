// <copyright file="SearchScope.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;


namespace Visus.LdapAuthentication {

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
        One = LdapConnection.ScopeOne,

        /// <summary>
        /// Recursively searches the base DN and all of its children.
        /// </summary>
        Sub = LdapConnection.ScopeSub
    }
}
