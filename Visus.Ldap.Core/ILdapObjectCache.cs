﻿// <copyright file="ILdapObjectCache.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Visus.Ldap {

    /// <summary>
    /// The interface of a caching service that allows the library to hold
    /// mapped LDAP objects in memory for reuse without querying the LDAP
    /// server every time.
    /// </summary>
    /// <typeparam name="TUser">The type of the user objects cached by the
    /// service.</typeparam>
    /// <typeparam name="TGroup">The type of the group objects cached by the
    /// service.</typeparam>
    public interface ILdapObjectCache<TUser, TGroup> {

        /// <summary>
        /// Adds the given <paramref name="group"/> to the cache.
        /// </summary>
        /// <param name="group">The group to be cached.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="group"/>
        /// is <c>null</c>.</exception>
        void Add(TGroup group);

        /// <summary>
        /// Adds the given <paramref name="user"/> to the cache.
        /// </summary>
        /// <param name="user">The grouserup to be cached.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="user"/>
        /// is <c>null</c>.</exception>
        void Add(TUser user);

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
        /// <param name="retval">Receives the cached or newly created group.
        /// </param>
        /// <returns><c>true</c> if <paramref name="retval"/> is from the cache,
        /// <c>false</c> if it was obtained from <paramref name="fallback"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="filter"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public bool GetGroup(string filter,
                Func<string, TGroup?> fallback,
                out TGroup? retval) {
            ArgumentNullException.ThrowIfNull(fallback, nameof(fallback));

            retval = this.GetGroup(filter);
            if (retval != null) {
                return true;
            }

            retval = fallback(filter);
            if (retval != null) {
                this.Add(retval);
            }

            return false;
        }

        /// <summary>
        /// Gets a cached group which matches the given filter or obtains a new 
        /// group from <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="filter">The LDAP filter selecting the group to look
        /// for.</param>
        /// <param name="fallback">A function to produce the group from
        /// <parmref name="filter" /> if it was not found in the cache.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="filter"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TGroup? GetGroup(string filter,
                Func<string, TGroup?> fallback) {
            this.GetGroup(filter, fallback, out var retval);
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

        /// <summary>
        /// Gets a cached user which matches the given filter.
        /// </summary>
        /// <param name="filter">The LDAP filter selecting the user to look
        /// for.</param>
        /// <returns>The user or <c>null</c> if no user matching the query was
        /// cached.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="filter"/> is <c>null</c>.</exception>
        TUser? GetUser(string filter);

        /// <summary>
        /// Gets a cached user which matches the given filter or obtains a new
        /// user from <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="filter">The LDAP filter selecting the user to look
        /// for.</param>
        /// <param name="fallback">A function to produce the user from
        /// <parmref name="filter" /> if it was not found in the cache.</param>
        /// <param name="retval">Receives the cached or newly created group.
        /// </param>
        /// <returns><c>true</c> if <paramref name="retval"/> is from the cache,
        /// <c>false</c> if it was obtained from <paramref name="fallback"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="filter"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public bool GetUser(string filter,
                Func<string, TUser?> fallback,
                out TUser? retval) {
            ArgumentNullException.ThrowIfNull(fallback, nameof(fallback));

            retval = this.GetUser(filter);
            if (retval != null) {
                return true;
            }

            retval = fallback(filter);
            if (retval != null) {
                this.Add(retval);
            }

            return false;
        }

        /// <summary>
        /// Gets a cached user which matches the given filter or obtains a new
        /// user from <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="filter">The LDAP filter selecting the user to look
        /// for.</param>
        /// <param name="fallback">A function to produce the user from
        /// <parmref name="filter"> if it was not found in the cache.</param>
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="filter"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TUser? GetUser(string filter,
                Func<string, TUser?> fallback) {
            this.GetUser(filter, fallback, out var retval);
            return retval;
        }

        /// <summary>
        /// Gets a cached user which matches the given filter or obtains a new
        /// user from <paramref name="fallback"/> and caches it for future use.
        /// </summary>
        /// <param name="filter">The LDAP filter selecting the user to look
        /// for.</param>
        /// <param name="fallback">A function to produce the user from
        /// <parmref name="filter" /> if it was not found in the cache.</param>
        /// <param name="name">The name of the user to look for.</param>
        /// <returns>The user or <c>null</c> if no user matching the query
        /// was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="filter"/> is <c>null</c>, or if
        /// <paramref name="fallback"/> is <c>null</c>.</exception>
        public async Task<TUser?> GetUser(string filter,
                Func<string, Task<TUser?>> fallback) {
            ArgumentNullException.ThrowIfNull(fallback, nameof(fallback));

            var retval = this.GetUser(filter);
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