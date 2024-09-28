// <copyright file="ILdapGroupCache.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.Ldap {

    /// <summary>
    /// The interface of a caching service that allows the library to hold
    /// group objects in memory for reuse without querying the LDAP server
    /// every time.
    /// </summary>
    /// <typeparam name="TGroup">The type of the group objects cached by the
    /// service.</typeparam>
    public interface ILdapGroupCache<TGroup> {

        /// <summary>
        /// Adds the given <paramref name="group"/> to the cache.
        /// </summary>
        /// <param name="group">The group to be cached.</param>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="group"/> is <c>null</c>.</exception>
        void Add(TGroup group);

        /// <summary>
        /// Gets a cached group with the specified distinguished name.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the group
        /// to look for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query was
        /// cached.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="distinguishedName"/> is <c>null</c>.</exception>
        TGroup? GetGroupByDistinguishedName(string distinguishedName);

        /// <summary>
        /// Gets a cached group with the specified value for the identity
        /// attribute.
        /// </summary>
        /// <param name="identity">The value of the identity attribute to
        /// be searched.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was cached.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="identity"/> is <c>null</c>.</exception>
        TGroup? GetGroupByIdentity(string identity);

        /// <summary>
        /// Gets a cached group with the specified name.
        /// </summary>
        /// <param name="name">The name of the group to look for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was cached.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="name"/> is <c>null</c>.</exception>
        TGroup? GetGroupByName(string name);
    }
}
