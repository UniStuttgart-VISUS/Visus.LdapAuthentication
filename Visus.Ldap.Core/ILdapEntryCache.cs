// <copyright file="ILdapEntryCache.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Threading.Tasks;


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

        /// <summary>
        /// Gets a cached entry which matches the given filter or obtains a new
        /// entry from <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="filter">The LDAP filter selecting the entry to look
        /// for.</param>
        /// <param name="fallback">A function to produce the entry from
        /// <parmref name="filter" /> if it was not found in the cache.</param>
        /// <param name="name">The name of the entry to look for.</param>
        /// <returns>The entry or <c>null</c> if no entry matching the query
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="filter"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public TEntry? GetEntry(string filter,
                Func<string, TEntry?> fallback) {
            ArgumentNullException.ThrowIfNull(fallback, nameof(fallback));

            var retval = this.GetEntry(filter);
            if (retval != null) {
                return retval;
            }

            retval = fallback(filter);
            if (retval != null) {
                this.Add(retval);
            }

            return retval;
        }

        /// <summary>
        /// Gets a cached entry which matches the given filter or obtains a new
        /// entry from <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="filter">The LDAP filter selecting the entry to look
        /// for.</param>
        /// <param name="fallback">A function to produce the entry from
        /// <parmref name="filter" /> if it was not found in the cache.</param>
        /// <param name="name">The name of the entry to look for.</param>
        /// <returns>The entry or <c>null</c> if no entry matching the query
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="filter"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public async Task<TEntry?> GetEntry(string filter,
                Func<string, Task<TEntry?>> fallback) {
            ArgumentNullException.ThrowIfNull(fallback, nameof(fallback));

            var retval = this.GetEntry(filter);
            if (retval != null) {
                return retval;
            }

            retval = await fallback(filter);
            if (retval != null) {
                this.Add(retval);
            }

            return retval;
        }
    }
}
