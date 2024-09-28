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
                if (this._map.DistinguishedNameProperty != null) {
                    var k = this._map.DistinguishedNameProperty.GetValue(group)
                        as string;
                    if (!string.IsNullOrEmpty(k)) {
                        this.Add(nameof(this.GetGroupByDistinguishedName),
                            k, group);
                    }
                }

                if (this._map.IdentityProperty != null) {
                    var k = this._map.IdentityProperty.GetValue(group)
                        as string;
                    if (!string.IsNullOrEmpty(k)) {
                        this.Add(nameof(this.GetGroupByIdentity),
                            k, group);
                    }
                }

                if (this._map.AccountNameProperty != null) {
                    var k = this._map.AccountNameProperty.GetValue(group)
                        as string;
                    if (!string.IsNullOrEmpty(k)) {
                        this.Add(nameof(this.GetGroupByName),
                            k, group);
                    }
                }
            }
        }

        /// <inheritdoc />
        public TGroup? GetGroupByDistinguishedName(string distinguishedName)
            => this._cache.Get<TGroup>(CreateKey(distinguishedName));

        /// <inheritdoc />
        public TGroup? GetGroupByIdentity(string identity)
            => this._cache.Get<TGroup>(CreateKey(identity));

        /// <inheritdoc />
        public TGroup? GetGroupByName(string name)
            => this._cache.Get<TGroup>(CreateKey(name));
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
        private static CacheKey<ILdapAttributeMap<TGroup>> CreateKey(string key,
                [CallerMemberName] string attribute = null!) {
            Debug.Assert(!string.IsNullOrEmpty(attribute));
            Debug.Assert(!string.IsNullOrEmpty(key));
            return CacheKey.Create<ILdapAttributeMap<TGroup>, TGroup>(
                $"{attribute}:{key}");
        }
        #endregion

        #region Private methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(string attribute, string key, TGroup value) {
            Debug.Assert(!string.IsNullOrEmpty(attribute));
            Debug.Assert(!string.IsNullOrEmpty(key));
            Debug.Assert(value != null);

            this._logger.LogTrace("Caching group with key {Key} for "
                + "attribute {Attribute}.", key, attribute);

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

            this._cache.Set(CreateKey(key, attribute), value, opts);
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
