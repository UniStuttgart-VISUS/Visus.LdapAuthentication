// <copyright file="ILdapGroupCache.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


using System;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        /// <exception cref="ArgumentNullException">If <paramref name="group"/>
        /// is <c>null</c>.</exception>
        void Add(TGroup group);

        /// <summary>
        /// Gets a cached group which matches the given filter.
        /// </summary>
        /// <param name="filter">The LDAP filter selecting the group to look
        /// for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query was
        /// cached.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="filter"/> is <c>null</c>.</exception>
        TGroup? GetGroup(string filter);

        /// <summary>
        /// Gets a cached group which matches the given filter or obtains a new 
        /// group from <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="filter">The LDAP filter selecting the group to look
        /// for.</param>
        /// <param name="fallback">A function to produce the group from
        /// <parmref name="filter" /> if it was not found in the cache.</param>
        /// <param name="name">The name of the group to look for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="filter"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public TGroup? GetGroup( string filter,
                Func<string, TGroup?> fallback) {
            ArgumentNullException.ThrowIfNull(fallback, nameof(fallback));

            var retval = this.GetGroup(filter);
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
        /// Gets a cached group which matches the given filter or obtains a new 
        /// group from <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="filter">The LDAP filter selecting the group to look
        /// for.</param>
        /// <param name="fallback">A function to produce the group from
        /// <parmref name="filter" /> if it was not found in the cache.</param>
        /// <param name="name">The name of the group to look for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="filter"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public async Task<TGroup?> GetGroup(string filter,
                Func<string, Task<TGroup?>> fallback) {
            ArgumentNullException.ThrowIfNull(fallback, nameof(fallback));

            var retval = this.GetGroup(filter);
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
