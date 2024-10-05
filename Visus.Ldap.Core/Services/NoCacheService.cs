// <copyright file="NoCacheService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;


namespace Visus.Ldap.Services {

    /// <summary>
    /// Implementation of <see cref="ILdapCacheBase{TEntry}"/> that caches
    /// nothing.
    /// </summary>
    /// <remarks>
    /// This service should be used in cases where caching is undesirable, for
    /// instance due to security concerns resulting from using stale data.
    /// </remarks>
    /// <typeparam name="TEntry">The type of raw LDAP entries cached by the
    /// service.</typeparam>
    public class NoCacheService<TEntry> : ILdapCacheBase<TEntry> {

        #region Public constants
        /// <summary>
        /// The default instance of the cache.
        /// </summary>
        public static readonly NoCacheService<TEntry> Default = new();
        #endregion

        #region Public methods
        /// <inheritdoc />
        public ILdapCacheBase<TEntry> Add(IEnumerable<TEntry> entries,
            IEnumerable<string> key) => this;

        /// <inheritdoc />
        public IEnumerable<TEntry>? Get(IEnumerable<string> key) => default;
        #endregion
    }
}
