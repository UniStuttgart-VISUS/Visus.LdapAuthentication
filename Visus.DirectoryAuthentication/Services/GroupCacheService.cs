// <copyright file="GroupCacheService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Visus.Ldap.Mapping;
using Visus.Ldap.Services;
using Visus.DirectoryAuthentication.Configuration;


namespace Visus.DirectoryAuthentication.Services {

    /// <summary>
    /// Implementation of <see cref="Ldap.ILdapGroupCache{TGroup}"/> that uses
    /// <see cref="IMemoryCache"/> to provide group objects from memory in order
    /// to bypass the LDAP server.
    /// </summary>
    /// <typeparam name="TGroup">The type of the group objects cached by the
    /// service.</typeparam>
    /// <param name="cache"></param>
    /// <param name="options"></param>
    /// <param name="map"></param>
    /// <param name="logger"></param>
    public sealed class GroupCacheService<TGroup>(
            IMemoryCache cache,
            IOptions<LdapOptions> options,
            ILdapAttributeMap<TGroup> map,
            ILogger<GroupCacheService<TGroup>> logger)
        : GroupCacheServiceBase<TGroup>(
            cache, options.Value, map, logger) { }
}
