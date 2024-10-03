// <copyright file="NoCacheService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


using System.Collections.Generic;

namespace Visus.Ldap.Services {

    /// <summary>
    /// Implementation of <see cref="ILdapObjectCache{TUser, TGroup}"/> and
    /// <see cref="ILdapEntryCache{TEntry}"/> that caches nothing.
    /// </summary>
    /// <remarks>
    /// This service should be used in cases where caching is undesirable, for
    /// instance due to security concerns resulting from using stale data.
    /// </remarks>
    /// <typeparam name="TEntry">The type of raw LDAP entries cached by the
    /// service.</typeparam>
    /// <typeparam name="TUser">The type of the user objects cached by the
    /// service.</typeparam>
    /// <typeparam name="TGroup">The type of the group objects cached by the
    /// service.</typeparam>
    public class NoCacheService<TEntry, TUser, TGroup>
            : ILdapObjectCache<TUser, TGroup>, ILdapEntryCache<TEntry> {

        #region Public constants
        /// <summary>
        /// The default instance of the cache.
        /// </summary>
        public static readonly NoCacheService<TEntry, TUser, TGroup> Default
            = new();
        #endregion

        #region Public methods
        /// <inheritdoc />
        public void Add(TEntry entry, IEnumerable<string>? filters) { }

        /// <inheritdoc />
        public void Add(TGroup group) { }

        /// <inheritdoc />
        public void Add(TUser user) { }

        /// <inheritdoc />
        public TEntry? GetEntry(string filter) => default;

        /// <inheritdoc />
        public TGroup? GetGroup(string filter) => default;

        /// <inheritdoc />
        public TUser? GetUser(string filter) => default;
        #endregion
    }
}
