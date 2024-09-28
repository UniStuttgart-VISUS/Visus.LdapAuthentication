// <copyright file="GroupCacheServiceBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;


namespace Visus.Ldap.Services {

    /// <summary>
    /// Implementation of <see cref="ILdapGroupCache{TGroup}"/> that uses
    /// <see cref="IMemoryCache"/> to provide group objects from memory in order
    /// to bypass the LDAP server.
    /// </summary>
    /// <typeparam name="TGroup">The type of the group objects cached by the
    /// service.</typeparam>
    public abstract class GroupCacheServiceBase<TGroup>
            : ILdapGroupCache<TGroup> {

        #region Public methods
        /// <inheritdoc />
        public void Add(TGroup group) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));

            if (this._options.GroupCaching != GroupCaching.None) {
                {
                    var att = this._map.DistinguishedNameAttribute;
                    var prop = this._map.DistinguishedNameProperty;

                    if ((att != null) && (prop != null)) {
                        var value = prop.GetValue(group) as string;
                        var filter = $"({att.Name}={value})";
                        this.Add(filter, group);
                    }
                }

                if (this._map.IdentityProperty != null) {
                    var att = this._map.IdentityAttribute;
                    var prop = this._map.IdentityProperty;

                    if ((att != null) && (prop != null)) {
                        var value = prop.GetValue(group) as string;
                        var filter = $"({att.Name}={value})";
                        this.Add(filter, group);
                    }
                }

                if (this._map.AccountNameProperty != null) {
                    var att = this._map.AccountNameAttribute;
                    var prop = this._map.AccountNameProperty;

                    if ((att != null) && (prop != null)) {
                        var value = prop.GetValue(group) as string;
                        var filter = $"({att.Name}={value})";
                        this.Add(filter, group);
                    }
                }
            }
        }

        /// <inheritdoc />
        public TGroup? GetGroup(string filter)
            => this._cache.Get<TGroup>(CreateKey(filter));
        #endregion

        #region Protected constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected GroupCacheServiceBase(IMemoryCache cache,
                LdapOptionsBase options,
                ILdapAttributeMap<TGroup> map,
                ILogger logger) {
            this._cache = cache
                ?? throw new ArgumentNullException(nameof(cache));
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._map = map
                ?? throw new ArgumentNullException(nameof(map));
            this._options = options
                ?? throw new ArgumentNullException(nameof(options));
        }
        #endregion

        #region Private class methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CacheKey<ILdapAttributeMap<TGroup>> CreateKey(
                string filter)
            => CacheKey.Create<ILdapAttributeMap<TGroup>, TGroup>(filter);
        #endregion

        #region Private methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(string filter, TGroup value) {
            Debug.Assert(!string.IsNullOrEmpty(filter));
            Debug.Assert(value != null);

            this._logger.LogTrace("Caching group for LDAP filter {Filter}.",
                filter);

            var duration = this._options.GroupCacheDuration;
            var opts = this._options.GroupCaching switch {
                GroupCaching.FixedExpiration => new MemoryCacheEntryOptions() {
                    AbsoluteExpirationRelativeToNow = duration
                },
                GroupCaching.SlidingExpiration => new() {
                    SlidingExpiration = duration
                },
                _ => throw new InvalidOperationException("This should be "
                        + "unreachable. Make sure that all valid cache options "
                        + "are handled above, specifically when extending "
                        + "the GroupCaching enumeration.")
            };

            this._cache.Set(CreateKey(filter), value, opts);
        }
        #endregion

        #region Private fields
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        private readonly ILdapAttributeMap<TGroup> _map;
        private readonly LdapOptionsBase _options;
        #endregion
    }
}
