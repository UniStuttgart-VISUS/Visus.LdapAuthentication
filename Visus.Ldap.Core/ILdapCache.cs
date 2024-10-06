// <copyright file="ILdapCache.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Visus.Ldap {


    /// <summary>
    /// The interface of a caching service that allows the library to hold
    /// raw LDAP entries in memory for reuse without querying the LDAP server
    /// every time.
    /// </summary>
    /// <typeparam name="TEntry">The type of raw LDAP entries cached by the
    /// service.</typeparam>
    public interface ILdapCache<TEntry> {

        /// <summary>
        /// Adds the given <paramref name="entries"/> to the cache, which must
        /// come from the same query identified by <paramref name="key"/>.
        /// </summary>
        /// <param name="entries">The entries to be cached.</param>
        /// <param name="key">The key, which comprises the filter string and the
        /// attributes loaded.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="entries"/> is <c>null</c>, or if
        /// <paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is
        /// empty.</exception>
        ILdapCache<TEntry> Add(IEnumerable<TEntry> entries,
            IEnumerable<string> key);

        /// <summary>
        /// Adds the given <paramref name="entries"/> to the cache, which was
        /// retrieved by the given <paramref name="filter"/> and include the
        /// given <paramref name="attributes"/>.
        /// </summary>
        /// <param name="entries">The entries to be cached.</param>
        /// <param name="filter">The LDAP filter used to obtain the
        /// <paramref name="entries"/>.</param>
        /// <param name="attributes">The attributes that have been loaded for
        /// <paramref name="entries"/>.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="entries"/> is <c>null</c>, or if
        /// <paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is
        /// empty.</exception>
        ILdapCache<TEntry> Add(IEnumerable<TEntry> entries,
                string filter,
                IEnumerable<string> attributes)
            => this.Add(entries, attributes?.Append(filter)!);

        /// <summary>
        /// Adds the given <paramref name="entry"/> to the cache, which was
        /// retrieved by the given <paramref name="filter"/> and includes the
        /// given <paramref name="attributes"/>.
        /// </summary>
        /// <param name="entry">The entry to be cached.</param>
        /// <param name="filter">The LDAP filter used to obtain the
        /// <paramref name="entry"/>.</param>
        /// <param name="attributes">The attributes that have been loaded for
        /// <paramref name="entry"/>.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="entry"/> is <c>null</c>, or if
        /// <paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is
        /// empty.</exception>
        ILdapCache<TEntry> Add(TEntry entry,
                string filter,
                IEnumerable<string> attributes)
            => this.Add([entry], filter, attributes);

        /// <summary>
        /// Gets cached LDAP entries which matches the key, which is a
        /// combination of the filter and the attributes that have been loaded.
        /// </summary>
        /// <param name="key">The key of the entries to retrieve, which are a
        /// combination of the LDAP search filter and the names of the loaded
        /// atrributes..</param>
        /// <returns>The entries that have been cached or <c>null</c> if there
        /// was no cache entry.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="key"/> is
        /// empty.</exception>
        IEnumerable<TEntry>? Get(IEnumerable<string> key);

        /// <summary>
        /// Gets cached LDAP entries for the given <paramref name="filter"/>
        /// expression and the set of loaded <paramref name="attributes"/>.
        /// </summary>
        /// <param name="filter">The filter to look for.</param>
        /// <param name="attributes">The attributes the cached entries must
        /// have.</param>
        /// <returns>The entries that have been cached or <c>null</c> if there
        /// was no cache entry.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="attributes"/> is <c>null</c>.</exception>
        IEnumerable<TEntry>? Get(string filter, IEnumerable<string> attributes)
            => this.Get(attributes?.Append(filter)!);

        bool GetOrAdd(string filter,
                IEnumerable<string> attributes,
                Func<IEnumerable<TEntry>> fallback,
                out IEnumerable<TEntry> retval) {
            ArgumentNullException.ThrowIfNull(filter);
            ArgumentNullException.ThrowIfNull(attributes);
            ArgumentNullException.ThrowIfNull(fallback);

            var key = attributes.Append(filter);

            retval = this.Get(key)!;
            if (retval != null) {
                return true;
            }

            retval = fallback();
            if (retval != null) {
                this.Add(retval, key);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerable<TEntry> GetOrAdd(string filter,
                IEnumerable<string> attributes,
                Func<IEnumerable<TEntry>> fallback) {
            this.GetOrAdd(filter, attributes, fallback, out var retval);
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
        public async Task<IEnumerable<TEntry>> GetOrAdd(string filter,
                IEnumerable<string> attributes,
                Func<Task<IEnumerable<TEntry>>> fallback) {
            ArgumentNullException.ThrowIfNull(filter);
            ArgumentNullException.ThrowIfNull(attributes);
            ArgumentNullException.ThrowIfNull(fallback);

            var key = attributes.Append(filter);

            var retval = this.Get(key);
            if (retval != null) {
                return retval;
            }

            retval = await fallback();
            if (retval != null) {
                this.Add(retval, key);
            }

            return retval!;
        }
    }
}
