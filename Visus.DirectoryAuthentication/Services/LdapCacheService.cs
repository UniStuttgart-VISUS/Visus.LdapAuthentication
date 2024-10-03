// <copyright file="LdapCacheService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
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
using System.DirectoryServices.Protocols;
using Visus.Ldap.Configuration;
using System;
using System.Diagnostics;
using System.Collections.Generic;


namespace Visus.DirectoryAuthentication.Services {

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
            ILdapMapper<SearchResultEntry, TUser, TGroup> map,
            ILogger<LdapCacheService<TUser, TGroup>> logger)
        : LdapCacheServiceBase<SearchResultEntry, TUser, TGroup>(
            cache, options.Value, map, logger) {

        /// <inheritdoc />
        public override void Add(SearchResultEntry entry,
                IEnumerable<string>? filters) {
            ArgumentNullException.ThrowIfNull(entry, nameof(entry));

            if (this.Options.Caching != LdapCaching.None) {
                {
                    Debug.Assert(this.Options.Mapping != null);
                    var att = this.Options.Mapping.DistinguishedNameAttribute;
                    var value = entry.DistinguishedName;

                    if ((att != null) && (value != null)) {
                        var filter = $"({att}={value})";
                        this.Add(filter, entry);
                    }
                }

                if (filters != null) {
                    foreach (var filter in filters) {
                        this.Add(filter, entry);
                    }
                }
            }
        }

    }
}
