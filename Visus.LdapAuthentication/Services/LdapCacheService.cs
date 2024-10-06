// <copyright file="LdapCacheService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using Visus.Ldap.Services;
using Visus.LdapAuthentication.Configuration;


namespace Visus.LdapAuthentication.Services {

    /// <summary>
    /// Implementation of <see cref="ILdapCache"/>.
    /// </summary>
    /// <param name="cache">The memory caching service to use.</param>
    /// <param name="options">The LDAP options configuring the caching
    /// behaviour.</param>
    /// <param name="logger">A logger to record issues the cache encounters.
    /// </param>
    /// <exception cref="System.ArgumentNullException">If
    /// <paramref name="cache"/> is <c>null</c>, or if
    /// <paramref name="options"/> is <c>null</c>, or if
    /// <paramref name="logger"/> is <c>null</c>.</exception>
    public sealed class LdapCacheService(
            IMemoryCache cache,
            IOptions<LdapOptions> options,
            ILogger<LdapCacheService> logger)
        : LdapCacheServiceBase<LdapEntry>(cache,
            options?.Value!,
            logger);
}
