﻿// <copyright file="CacheKey.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Caching.Memory;
using System;
using System.Diagnostics;


namespace Visus.Ldap.Services {

    /// <summary>
    /// Implements a key for the memory cache including the type of the owner
    /// in the key, which allows storage of per-class data in the global memory
    /// cache.
    /// </summary>
    /// <typeparam name="TOwner">The owner of the key.</typeparam>
    [DebuggerDisplay("{_identity}: {_type}")]
    internal sealed class CacheKey<TOwner> : IEquatable<CacheKey<TOwner>> {

        #region Public constructors
        /// <summary>
        /// Initialises an new instance.
        /// </summary>
        /// <param name="type">The type of the item encoded.</param>
        /// <param name="identity">The identity of the item, which must be
        /// unique for the <paramref name="type"/>.</param>
        public CacheKey(Type type, string identity) {
            this._identity = identity
                ?? throw new ArgumentNullException(nameof(identity));
            this._type = type
                ?? throw new ArgumentNullException(nameof(identity));
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public bool Equals(CacheKey<TOwner>? other) {
            return (other != null)
                && (other._type == this._type)
                && (other._identity == this._identity);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) {
            return this.Equals(obj as CacheKey<TOwner>);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            return this.GetType().GetHashCode()
                ^ this._type.GetHashCode()
                ^ this._identity.GetHashCode();
        }
        #endregion

        #region Private fields
        private readonly string _identity;
        private readonly Type _type;
        #endregion
    }


    /// <summary>
    /// Implements factory methods for <see cref="CacheKey{TOwner}"/>.
    /// </summary>
    internal static class CacheKey {

        /// <summary>
        /// Creates a new key for the object identified by
        /// <paramref name="identity"/> and owned by the given
        /// <typeparamref name="TOwner"/>.
        /// </summary>
        /// <typeparam name="TOwner">The type of the class owning the cached
        /// object.</typeparam>
        /// <param name="type">The type of the object being cached.</param>
        /// <param name="identity">A string uniquely identifying the cached
        /// object among the objects of type <paramref name="type"/> cached
        /// by <typeparamref name="TOwner"/>.</param>
        /// <returns>A new cache key.</returns>
        public static CacheKey<TOwner> Create<TOwner>(Type type,
                string identity) => new CacheKey<TOwner>(type, identity);

        /// <summary>
        /// Creates a new key for the object identified by
        /// <paramref name="identity"/> and owned by the given
        /// <typeparamref name="TOwner"/>.
        /// </summary>
        /// <typeparam name="TOwner">The type of the class owning the cached
        /// object.</typeparam>
        /// <typeparam name="TType">The type of the object being cached.
        /// </typeparam>
        /// <param name="identity">A string uniquely identifying the cached
        /// object among the objects of type <typeparamref name="TType"/> cached
        /// by <typeparamref name="TOwner"/>.</param>
        /// <returns>A new cache key.</returns>
        public static CacheKey<TOwner> Create<TOwner, TType>(string identity)
            => new CacheKey<TOwner>(typeof(TType), identity);
    }
}