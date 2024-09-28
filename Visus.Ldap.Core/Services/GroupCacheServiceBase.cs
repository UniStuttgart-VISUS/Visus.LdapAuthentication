// <copyright file="GroupCacheBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
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
    /// <typeparam name="TEntry">The type of the LDAP entry, which is required
    /// to instantiate a <see cref="ILdapMapper{TEntry, TUser, TGroup}"/>.
    /// </typeparam>
    /// <typeparam name="TUser">The type of the LDAP user objects, which is
    /// required to instantiate a
    /// <see cref="ILdapMapper{TEntry, TUser, TGroup}"/>.</typeparam>
    /// <typeparam name="TGroup">The type of the group objects cached by the
    /// service.</typeparam>
    public abstract class GroupCacheServiceBase<TEntry, TUser, TGroup>
            : ILdapGroupCache<TGroup> {

        #region Public methods
        /// <inheritdoc />
        public void Add(TGroup group) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));

            {
                var key = this._mapper.GetDistinguishedName(group);
                if (!string.IsNullOrEmpty(key)) {
                    this.Add(nameof(this.GetGroupByDistinguishedName),
                        key, group);
                }
            }

            {
                var key = this._mapper.GetIdentity(group);
                if (!string.IsNullOrEmpty(key)) {
                    this.Add(nameof(this.GetGroupByIdentity),
                        key, group);
                }
            }

            {
                var key = this._mapper.GetAccountName(group);
                if (!string.IsNullOrEmpty(key)) {
                    this.Add(nameof(this.GetGroupByName),
                        key, group);
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
                ILdapMapper<TEntry, TUser, TGroup> mapper,
                ILogger logger) {
            this._cache = cache
                ?? throw new ArgumentNullException(nameof(cache));
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
            this._options = options
                ?? throw new ArgumentNullException(nameof(options));
        }
        #endregion

        #region Private class methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CacheKey<GroupCacheServiceBase<TEntry, TUser, TGroup>>
        CreateKey(string key,
                [CallerMemberName] string attribute = null!) {
            Debug.Assert(!string.IsNullOrEmpty(attribute));
            Debug.Assert(!string.IsNullOrEmpty(key));
            return CacheKey.Create<GroupCacheServiceBase<TEntry, TUser, TGroup>,
                TGroup>($"{attribute}:{key}");
        }
        #endregion

        #region Private methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add(string attribute, string key, TGroup value) {
            Debug.Assert(!string.IsNullOrEmpty(attribute));
            Debug.Assert(!string.IsNullOrEmpty(key));
            Debug.Assert(value != null);

            if (this._options.GroupCaching != GroupCaching.None) {
                this._logger.LogTrace("Caching group with key {Key} for "
                    + "attribute {Attribute}.", key, attribute);

                var entry = this._cache.CreateEntry(CreateKey(key, attribute));
                entry.Value = key;

                switch (this._options.GroupCaching) {
                    case GroupCaching.FixedExpiration:
                        entry.AbsoluteExpirationRelativeToNow
                            = this._options.GroupCacheDuration;
                        break;

                    case GroupCaching.SlidingExpiration:
                        entry.SlidingExpiration
                            = this._options.GroupCacheDuration;
                        break;

                    default:
                        throw new InvalidOperationException(
                            "This should be unreachable.");
                }
            }
        }
        #endregion

        #region Private fields
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        private readonly ILdapMapper<TEntry, TUser, TGroup> _mapper;
        private readonly LdapOptionsBase _options;
        #endregion
    }
}
