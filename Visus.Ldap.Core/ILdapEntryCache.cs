// <copyright file="ILdapEntryCache.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;


namespace Visus.Ldap {

    /// <summary>
    /// The interface of a caching service that allows the library to hold
    /// raw LDAP entries in memory for reuse without querying the LDAP server
    /// every time.
    /// </summary>
    /// <typeparam name="TEntry">The type of raw LDAP entries cached by the
    /// service.</typeparam>
    public interface ILdapEntryCache<TEntry> {

        /// <summary>
        /// Adds the given <paramref name="entry"/> to the cache.
        /// </summary>
        /// <param name="entry">The group to be cached.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="entry"/>
        /// is <c>null</c>.</exception>
        void Add(TEntry entry);

        /// <summary>
        /// Gets a cached LDAP entry which matches the given filter.
        /// </summary>
        /// <param name="filter">The LDAP filter selecting the entry to look
        /// for.</param>
        /// <returns>The entry or <c>null</c> if no entry matching the query was
        /// cached.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="filter"/> is <c>null</c>.</exception>
        TEntry? GetEntry(string filter);
    }
}
