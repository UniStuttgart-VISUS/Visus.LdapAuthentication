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
        /// Gets a cached group with the specified distinguished name.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the group
        /// to look for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query was
        /// cached.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="distinguishedName"/> is <c>null</c>.</exception>
        TGroup? GetGroupByDistinguishedName(string distinguishedName);

        /// <summary>
        /// Gets a cached group with the specified distinguished name or obtains
        /// it from <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the group
        /// to look for.</param>
        /// <param name="fallback">A function to produce the group if it was not
        /// found in the cache.</param>
        /// <param name="name">The name of the group to look for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="distinguishedName"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public TGroup? GetGroupByDistinguishedName(
                string distinguishedName,
                Func<string, TGroup?> fallback)
            => this.GetOrFallback(distinguishedName,
                this.GetGroupByDistinguishedName,
                fallback);

        /// <summary>
        /// Gets a cached group with the specified distinguished name or obtains
        /// it from <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the group
        /// to look for.</param>
        /// <param name="fallback">A function to produce the group if it was not
        /// found in the cache.</param>
        /// <param name="name">The name of the group to look for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="distinguishedName"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public Task<TGroup?> GetGroupByDistinguishedName(
                string distinguishedName,
                Func<string, Task<TGroup?>> fallback)
            => this.GetOrFallback(distinguishedName,
                this.GetGroupByDistinguishedName,
                fallback);

        /// <summary>
        /// Gets a cached group with the specified value for the identity
        /// attribute.
        /// </summary>
        /// <param name="identity">The value of the identity attribute to
        /// be searched.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was cached.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="identity"/> is <c>null</c>.</exception>
        TGroup? GetGroupByIdentity(string identity);

        /// <summary>
        /// Gets a cached group with the specified identity attribute or obtains
        /// it from <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="identity">The value of the identity attribute to
        /// be searched.</param>
        /// <param name="fallback">A function to produce the group if it was not
        /// found in the cache.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="identity"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public TGroup? GetGroupByIdentity(
                string identity,
                Func<string, TGroup?> fallback)
            => this.GetOrFallback(identity,
                this.GetGroupByIdentity,
                fallback);

        /// <summary>
        /// Gets a cached group with the specified identity attribute or obtains
        /// it from <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="identity">The value of the identity attribute to
        /// be searched.</param>
        /// <param name="fallback">A function to produce the group if it was not
        /// found in the cache.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="identity"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public Task<TGroup?> GetGroupByIdentity(
                string identity,
                Func<string, Task<TGroup?>> fallback)
            => this.GetOrFallback(identity,
                this.GetGroupByIdentity,
                fallback);

        /// <summary>
        /// Gets a cached group with the specified name.
        /// </summary>
        /// <param name="name">The name of the group to look for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was cached.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="name"/> is <c>null</c>.</exception>
        TGroup? GetGroupByName(string name);

        /// <summary>
        /// Gets a cached group with the specified name or obtains it from
        /// <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="name">The name of the group to look for.</param>
        /// <param name="fallback">A function to produce the group if it was not
        /// found in the cache.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="name"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public TGroup? GetGroupByName(string name,
                Func<string, TGroup?> fallback)
            => this.GetOrFallback(name,
                this.GetGroupByName,
                fallback);

        /// <summary>
        /// Gets a cached group with the specified name or obtains it from
        /// <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="name">The name of the group to look for.</param>
        /// <param name="fallback">A function to produce the group if it was not
        /// found in the cache.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="name"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public Task<TGroup?> GetGroupByName(string name,
                Func<string, Task<TGroup?>> fallback)
            => this.GetOrFallback(name,
                this.GetGroupByName,
                fallback);

        #region Private methods
        private TGroup? GetOrFallback(string key,
                Func<string, TGroup?> lookup,
                Func<string, TGroup?> fallback) {
            ArgumentNullException.ThrowIfNull(fallback, nameof(fallback));

            var retval = lookup(key);
            if (retval != null) {
                return retval;
            }

            retval = fallback(key);
            if (retval != null) {
                this.Add(retval);
            }

            return retval;
        }

        private async Task<TGroup?> GetOrFallback(string key,
                Func<string, TGroup?> lookup,
                Func<string, Task<TGroup?>> fallback) {
            ArgumentNullException.ThrowIfNull(fallback, nameof(fallback));

            var retval = lookup(key);
            if (retval != null) {
                return retval;
            }

            retval = await fallback(key);
            if (retval != null) {
                this.Add(retval);
            }

            return retval;
        }
        #endregion
    }
}
