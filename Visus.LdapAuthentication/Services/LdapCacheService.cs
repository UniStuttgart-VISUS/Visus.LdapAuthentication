// <copyright file="LdapCacheService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using Visus.Ldap.Mapping;
using Visus.Ldap.Services;
using Visus.LdapAuthentication.Configuration;


namespace Visus.LdapAuthentication.Services {

    /// <summary>
    /// Implementation of <see cref="Ldap.ILdapObjectCache{TUser, TGroup}"/> and
    /// <see cref="Ldap.ILdapEntryCache{TEntry}"/> that uses
    /// <see cref="IMemoryCache"/> to provide group objects from memory in
    /// order to bypass the LDAP server.
    /// </summary>
    /// <typeparam name="TEntry">The type of raw LDAP entries cached by the
    /// service.</typeparam>
    /// <typeparam name="TUser">The type of the user objects cached by the
    /// service.</typeparam>
    /// <typeparam name="TGroup">The type of the group objects cached by the
    /// service.</typeparam>
    /// <param name="cache"></param>
    /// <param name="options"></param>
    /// <param name="map"></param>
    /// <param name="logger"></param>
    public sealed class LdapCacheService<TUser, TGroup>(
            IMemoryCache cache,
            IOptions<LdapOptions> options,
            ILdapMapper<LdapEntry, TUser, TGroup> map,
            ILogger<LdapCacheService<TUser, TGroup>> logger)
        : LdapCacheServiceBase<LdapEntry, TUser, TGroup>(
            cache, options.Value, map, logger) { }
}
