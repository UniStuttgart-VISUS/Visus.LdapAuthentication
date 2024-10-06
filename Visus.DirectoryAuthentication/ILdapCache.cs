// <copyright file="ILdapCache.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.DirectoryServices.Protocols;
using Visus.Ldap;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// The interface of a caching service that allows the library to hold
    /// raw LDAP entries in memory for reuse without querying the LDAP server
    /// every time.
    /// </summary>
    public interface ILdapCache : ILdapCache<SearchResultEntry>;
}
