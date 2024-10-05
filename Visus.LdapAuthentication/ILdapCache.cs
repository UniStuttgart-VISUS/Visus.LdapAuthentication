// <copyright file="ILdapCache.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using Visus.Ldap;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// The interface of a caching service that allows the library to hold
    /// raw LDAP entries in memory for reuse without querying the LDAP server
    /// every time.
    /// </summary>
    public interface ILdapCache : ILdapCacheBase<LdapEntry>;
}
