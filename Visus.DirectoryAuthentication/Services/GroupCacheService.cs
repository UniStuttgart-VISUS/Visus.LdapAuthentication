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
    /// <param name="cache"></param>
    /// <param name="options"></param>
    /// <param name="mapper"></param>
    /// <param name="logger"></param>
    public sealed class GroupCacheService<TEntry, TUser, TGroup>(
            IMemoryCache cache,
            IOptions<LdapOptions> options,
            ILdapMapper<TEntry, TUser, TGroup> mapper,
            ILogger<GroupCacheService<TEntry, TUser, TGroup>> logger)
        : GroupCacheServiceBase<TEntry, TUser, TGroup>(
            cache, options.Value, mapper, logger) { }
}
