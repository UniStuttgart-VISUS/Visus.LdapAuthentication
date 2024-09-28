// <copyright file="NoGroupCacheService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.Ldap.Services {

    /// <summary>
    /// Implementation of <see cref="ILdapGroupCache{TGroup}"/> that caches
    /// nothing.
    /// </summary>
    /// <typeparam name="TGroup">The type of the group objects cached by the
    /// service.</typeparam>
    public class NoGroupCacheService<TGroup> : ILdapGroupCache<TGroup> {

        #region Public constants
        /// <summary>
        /// The default instance of the cache.
        /// </summary>
        public static readonly NoGroupCacheService<TGroup> Default = new();
        #endregion

        #region Public methods
        /// <inheritdoc />
        public void Add(TGroup group) {}

        /// <inheritdoc />
        public TGroup? GetGroup(string filter) => default;
        #endregion
    }
}
