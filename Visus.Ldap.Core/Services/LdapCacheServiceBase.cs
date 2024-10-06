// <copyright file="LdapCacheServiceBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Visus.Ldap.Configuration;


namespace Visus.Ldap.Services {

    /// <summary>
    /// Implementation of <see cref="ILdapCache{TEntry}"/> that uses
    /// <see cref="IMemoryCache"/> to provide group objects from memory in
    /// order to bypass the LDAP server.
    /// </summary>
    /// <typeparam name="TEntry">The type of raw LDAP entries cached by the
    /// service.</typeparam>
    public abstract class LdapCacheServiceBase<TEntry>
            : ILdapCache<TEntry> {

        #region Public methods
        /// <inheritdoc />
        public ILdapCache<TEntry> Add(IEnumerable<TEntry> entries,
                IEnumerable<string> key) {
            ArgumentNullException.ThrowIfNull(entries);

            if (this.Options.Caching == LdapCaching.None) {
                return this;
            }

            this._logger.LogTrace("Caching LDAP results for {Key}.",
                string.Join(", ", key));

            var duration = this.Options.CacheDuration;
            var opts = this.Options.Caching switch {
                LdapCaching.FixedExpiration => new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = duration
                },
                LdapCaching.SlidingExpiration => new() {
                    SlidingExpiration = duration
                },
                _ => throw new InvalidOperationException("This should be "
                        + "unreachable. Make sure that all valid cache options "
                        + "are handled above, specifically when extending "
                        + $"the {nameof(LdapCaching)} enumeration.")
            };

            this._cache.Set(CreateKey(key), entries, opts);

            return this;
        }

        /// <inheritdoc />
        public IEnumerable<TEntry>? Get(IEnumerable<string> key) {
            var retval = this._cache.Get<IEnumerable<TEntry>>(CreateKey(key));

            this._logger.LogTrace("Cache {HitOrMiss} for LDAP key {Key}.",
                (retval == null ? "miss" : "hit"), string.Join(", ", key));

            return retval;
        }
        #endregion

        #region Protected constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="cache">The memory caching service to use.</param>
        /// <param name="options">The LDAP options configuring the caching
        /// behaviour.</param>
        /// <param name="logger">A logger to record issues the cache encounters.
        /// </param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="cache"/> is <c>null</c>, or if
        /// <paramref name="options"/> is <c>null</c>, or if
        /// <paramref name="logger"/> is <c>null</c>.</exception>
        protected LdapCacheServiceBase(IMemoryCache cache,
                LdapOptionsBase options,
                ILogger logger) {
            this._cache = cache
                ?? throw new ArgumentNullException(nameof(cache));
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this.Options = options
                ?? throw new ArgumentNullException(nameof(options));
        }
        #endregion

        #region Protected class methods
        /// <summary>
        /// Creates a cache key identifying an object of type
        /// <typeparamref name="TEntry"/> and with the specified
        /// <paramref name="key"/> made of the LDAP filter and the attributes
        /// loaded.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CacheKey<LdapCacheServiceBase<TEntry>> CreateKey(
                IEnumerable<string> key)
            => CacheKey.Create<LdapCacheServiceBase<TEntry>, TEntry>(key);
        #endregion

        #region Protected properties
        /// <summary>
        /// Gets the LDAP configuration that defines the caching behaviour,
        /// which enables skipping any caching if it is disabled.
        /// </summary>
        protected LdapOptionsBase Options { get; }
        #endregion

        #region Private fields
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        #endregion
    }
}
