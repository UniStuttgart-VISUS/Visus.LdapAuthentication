﻿// <copyright file="LdapCacheServiceBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
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
    /// Implementation of <see cref="ILdapObjectCache{TUser, TGroup}"/> and
    /// <see cref="ILdapEntryCache{TEntry}"/> that uses
    /// <see cref="IMemoryCache"/> to provide group objects from memory in
    /// order to bypass the LDAP server.
    /// </summary>
    /// <typeparam name="TEntry">The type of raw LDAP entries cached by the
    /// service.</typeparam>
    /// <typeparam name="TUser">The type of the user objects cached by the
    /// service.</typeparam>
    /// <typeparam name="TGroup">The type of the group objects cached by the
    /// service.</typeparam>
    public abstract class LdapCacheServiceBase<TEntry, TUser, TGroup>
            : ILdapObjectCache<TUser, TGroup>, ILdapEntryCache<TEntry> {

        #region Public methods
        /// <inheritdoc />
        public void Add(TEntry entry) {
            ArgumentNullException.ThrowIfNull(entry, nameof(entry));

            if (this._options.Caching != LdapCaching.None) {
                //{
                //    var att = map.DistinguishedNameAttribute?.Name;
                //    var value = this._mapper.GetDistinguishedName(entry);

                //    if ((att != null) && (value != null)) {
                //        var filter = $"({att}={value})";
                //        this.Add(filter, entry);
                //    }
                //}

                //{
                //    var att = map.IdentityAttribute?.Name;
                //    var value = this._mapper.GetIdentity(entry);

                //    if ((att != null) && (value != null)) {
                //        var filter = $"({att}={value})";
                //        this.Add(filter, entry);
                //    }
                //}

                //{
                //    var att = map.AccountNameAttribute?.Name;
                //    var value = this._mapper.GetAccountName(entry);

                //    if ((att != null) && (value != null)) {
                //        var filter = $"({att}={value})";
                //        this.Add(filter, entry);
                //    }
                //}
            }
        }

        /// <inheritdoc />
        public void Add(TGroup group) {
            ArgumentNullException.ThrowIfNull(group, nameof(group));

            if (this._options.Caching != LdapCaching.None) {
                var map = this._mapper.GroupMap;

                {
                    var att = map.DistinguishedNameAttribute?.Name;
                    var value = this._mapper.GetDistinguishedName(group);

                    if ((att != null) && (value != null)) {
                        var filter = $"({att}={value})";
                        this.Add(filter, group);
                    }
                }

                {
                    var att = map.IdentityAttribute?.Name;
                    var value = this._mapper.GetIdentity(group);

                    if ((att != null) && (value != null)) {
                        var filter = $"({att}={value})";
                        this.Add(filter, group);
                    }
                }

                {
                    var att = map.AccountNameAttribute?.Name;
                    var value = this._mapper.GetAccountName(group);

                    if ((att != null) && (value != null)) {
                        var filter = $"({att}={value})";
                        this.Add(filter, group);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Add(TUser user) {
            ArgumentNullException.ThrowIfNull(user, nameof(user));

            if (this._options.Caching != LdapCaching.None) {
                var map = this._mapper.UserMap;

                {
                    var att = map.DistinguishedNameAttribute?.Name;
                    var value = this._mapper.GetDistinguishedName(user);

                    if ((att != null) && (value != null)) {
                        var filter = $"({att}={value})";
                        this.Add(filter, user);
                    }
                }

                {
                    var att = map.IdentityAttribute?.Name;
                    var value = this._mapper.GetIdentity(user);

                    if ((att != null) && (value != null)) {
                        var filter = $"({att}={value})";
                        this.Add(filter, user);
                    }
                }

                {
                    var att = map.AccountNameAttribute?.Name;
                    var value = this._mapper.GetAccountName(user);

                    if ((att != null) && (value != null)) {
                        var filter = $"({att}={value})";
                        this.Add(filter, user);
                    }
                }
            }
        }

        /// <inheritdoc />
        public TEntry? GetEntry(string filter)
            => this._cache.Get<TEntry>(CreateKey<TEntry>(filter));

        /// <inheritdoc />
        public TGroup? GetGroup(string filter)
            => this._cache.Get<TGroup>(CreateKey<TGroup>(filter));

        /// <inheritdoc />
        public TUser? GetUser(string filter)
            => this._cache.Get<TUser>(CreateKey<TUser>(filter));
        #endregion

        #region Protected constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="cache">The memory caching service to use.</param>
        /// <param name="options">The LDAP options configuring global mappings.
        /// </param>
        /// <param name="mapper">The mapper for retrieving the correct LDAP
        /// attributes for caching.</param>
        /// <param name="logger">A logger to record issues the cache encounters.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="cache"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>, or
        /// if <paramref name="mapper"/> is <c>null</c>, or if
        /// <paramref name="logger"/> is <c>null</c>.</exception>
        protected LdapCacheServiceBase(IMemoryCache cache,
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

        #region Protected class methods
        /// <summary>
        /// Creates a cache key identifying an object of type
        /// <typeparamref name="TObject"/> and with the specified LDAP
        /// <paramref name="filter"/>.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CacheKey<LdapCacheServiceBase<TEntry, TUser, TGroup>>
        CreateKey<TObject>(string filter) => CacheKey.Create<
            LdapCacheServiceBase<TEntry, TUser, TGroup>, TObject>(filter);
        #endregion

        #region Private methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Add<TObject>(string filter, TObject value) {
            Debug.Assert(!string.IsNullOrEmpty(filter));
            Debug.Assert(value != null);

            this._logger.LogTrace("Caching {Type} for LDAP filter {Filter}.",
                typeof(TObject).Name, filter);

            var duration = this._options.CacheDuration;
            var opts = this._options.Caching switch {
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

            this._cache.Set(CreateKey<TObject>(filter), value, opts);
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
