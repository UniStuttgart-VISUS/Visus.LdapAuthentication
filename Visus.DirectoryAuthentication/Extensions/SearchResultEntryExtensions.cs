// <copyright file="SearchResultEntryExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2022 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication.Configuration;
using Visus.Ldap;
using Visus.Ldap.Extensions;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Extensions {

    /// <summary>
    /// Extension methods for <see cref="SearchResultEntry"/>.
    /// </summary>
    internal static class SearchResultEntryExtensions {

        #region Internal methods
        /// <summary>
        /// Gets the attribute with the specified name from
        /// <paramref name="that"/>.
        /// </summary>
        /// <remarks>
        /// This is a convenience accessor for the
        /// <see cref="SearchResultEntry.Attributes"/> property, which reduces
        /// the changes required to port from Novell LDAP.
        /// </remarks>
        /// <param name="that">The entry to retrieve the attribute for.</param>
        /// <param name="attribute">The name of the attribute to be retrived.
        /// </param>
        /// <returns>The <see cref="DirectoryAttribute"/> with the specified
        /// name.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static DirectoryAttribute? GetAttribute(
                this SearchResultEntry? that, string attribute) {
            return (that != null) ? that.Attributes[attribute] : null;
        }

        /// <summary>
        /// Gets an LDAP filter that selects the given entry via its DN.
        /// </summary>
        /// <param name="that">The entry to get the filter for.</param>
        /// <param name="options">The <see cref="LdapOptions"/> that provide the
        /// information what the name of the DN attribute is.</param>
        /// <returns>A filter for the given entry.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetFilter(this SearchResultEntry that,
                LdapOptions options) {
            Debug.Assert(that != null);
            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);
            return that.DistinguishedName.ToLdapFilter(
                options.Mapping.DistinguishedNameAttribute);
        }

        /// <summary>
        /// Gets the DNs of all groups <paramref name="that"/> is directly
        /// member of.
        /// </summary>
        /// <param name="that"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        internal static IEnumerable<string> GetGroups(
                this SearchResultEntry that,
                LdapOptions options) {
            ArgumentNullException.ThrowIfNull(that, nameof(that));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            var mapping = options.Mapping;
            Debug.Assert(mapping != null);

            try {
                var att = that.GetAttribute(mapping.GroupsAttribute);
                if (att != null) {
                    return att.GetValues(typeof(string))
                        .Cast<string>()
                        .ToHashSet();
                }
            } catch { /* Entry has no valid group memberships. */ }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets the LDAP entries for the groups of <paramref name="that"/>
        /// using the specified LDAP <paramref name="connection"/>.
        /// </summary>
        /// <remarks>
        /// Note that this will only include the groups <paramref name="that"/>
        /// is direct memory of, which are returned by
        /// <see cref="GetGroups(LdapEntry, LdapOptions)"/>. If you need LDAP
        /// entries for all (recursive) groups including the primary group,
        /// use <see cref="GetGroupEntries"/> instead.
        /// </remarks>
        /// <param name="that">The entry to get the groups for.</param>
        /// <param name="connection">The LDAP connection used to retrieve the
        /// entry representing the groups obtained from <paramref name="that"/>.
        /// </param>
        /// <param name="cache">A cache for the LDAP entries which potentially
        /// prevents the method from performing an actual LDAP lookup.</param>
        /// <param name="attributes">The attributes to load for the entry of the
        /// group.</param>
        /// <param name="options">The LDAP options determining the search scope
        /// and the attribute mapping.</param>
        /// <param name="directOnly">If <c>true</c>, the method will ignore
        /// <see cref="LdapOptions.IsRecursiveGroupMembership"/> and return only
        /// direct groups <paramref name="that"/> is member of. This parameter is
        /// intended for implementing other overloads of the method and defaults
        /// to <c>false</c>.</param>
        /// <returns>The LDAP entries of all groups except for the primary one.
        /// </returns>
        internal static IEnumerable<SearchResultEntry> GetGroups(
                this SearchResultEntry that,
                LdapConnection connection,
                ILdapCache<SearchResultEntry> cache,
                string[] attributes,
                LdapOptions options,
                bool directOnly = false) {
            Debug.Assert(that != null);
            Debug.Assert(connection != null);
            Debug.Assert(cache != null);
            Debug.Assert(attributes != null);
            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);
            var dnAtt = options.Mapping.DistinguishedNameAttribute;

            // Get LDAP filters for the groups of 'that'.
            var filters = new Stack<string>();

            var dns = that.GetGroups(options);
            if (dns.Any()) {
                filters.Push(dns.ToLdapFilter(dnAtt));
            }

            while (filters.TryPop(out var filter)) {
                // Look up the entries for the current group.
#if WITH_LDAP_CACHE
                var groups = cache.GetOrAdd(filter, attributes,
                    () => connection.Search(filter, attributes, options));
#else // WITH_LDAP_CACHE
                var groups = connection.Search(filter, attributes, options);
#endif // WITH_LDAP_CACHE

                foreach (var g in groups) {
                    yield return g;

                    // If we are requested to recurse, get the LDAP filters
                    // for 'r' and queue them for retrieval.
                    if (options.IsRecursiveGroupMembership && !directOnly) {
                        dns = g.GetGroups(options);
                        if (dns.Any()) {
                            filters.Push(dns.ToLdapFilter(dnAtt));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves all groups of <paramref name="that"/> and maps them to
        /// <typeparamref name="TGroup"/>.
        /// </summary>
        /// <typeparam name="TUser">The type of user used by the
        /// <paramref name="mapper"/>.</typeparam>
        /// <typeparam name="TGroup">The type of the group objects to be
        /// created.</typeparam>
        /// <param name="that"></param>
        /// <param name="connection">The LDAP connection used to retrieve the
        /// entry representing the groups obtained from <paramref name="that"/>.
        /// </param>
        /// <param name="cache">A cache for the LDAP entries which potentially
        /// prevents the method from performing an actual LDAP lookup.</param>
        /// <param name="mapper"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        internal static IEnumerable<TGroup> GetGroups<TUser, TGroup>(
                this SearchResultEntry that,
                LdapConnection connection,
                ILdapCache<SearchResultEntry> cache,
                ILdapMapper<SearchResultEntry, TUser, TGroup> mapper,
                LdapOptions options)
                where TGroup : new() {
            Debug.Assert(that != null);
            Debug.Assert(connection != null);
            Debug.Assert(mapper != null);
            Debug.Assert(cache != null);
            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);

            // Compile the attributes we need to read for a group.
            var attributes = mapper.RequiredGroupAttributes
                .Append(options.Mapping.PrimaryGroupAttribute)
                .Append(options.Mapping.PrimaryGroupIdentityAttribute)
                .Append(options.Mapping.GroupsAttribute)
                .ToArray();

            // Prepare a caching mechanism for group objects.
            var knownGroups = new Dictionary<string, TGroup>();
            var mapUnknown = (SearchResultEntry e) => {
                if (!knownGroups.TryGetValue(e.DistinguishedName,
                        out var retval)) {
                    retval = mapper.MapGroup(e, new TGroup());
                    knownGroups.Add(e.DistinguishedName, retval);
                }
                return retval;
            };
            var getGroups = (SearchResultEntry e) => e.GetGroups(connection,
                cache, attributes, options).Select(p => mapUnknown(p));

            // Get the primary group, which requires special handling.
            var primaryGroup = that.GetPrimaryGroup(connection,
                cache,
                attributes,
                options);

            if (mapper.GroupIsGroupMember) {
                // Recursively reconstruct the hierarchy in the group object if
                // it contains its parent groups.

                if (primaryGroup != null) {
                    var group = knownGroups[primaryGroup.DistinguishedName]
                        = mapper.MapPrimaryGroup(primaryGroup, new TGroup());
                    var parents = getGroups(primaryGroup);
                    yield return mapper.SetGroups(group, parents);
                }

                var groups = that.GetGroups(connection, cache, attributes,
                    options, false);
                foreach (var g in groups) {
                    var group = mapUnknown(g);
                    var parents = getGroups(g);
                    yield return mapper.SetGroups(group, parents);
                }

            } else {
                // Just get individual group object w/o setting their parents.
                if (primaryGroup != null) {
                    knownGroups[primaryGroup.DistinguishedName]
                        = mapper.MapPrimaryGroup(primaryGroup, new TGroup());

                    // The primary group might be a group member itself, so we
                    // need to expand the hierarchy as requested.
                    if (options.IsRecursiveGroupMembership) {
                        _ = getGroups(primaryGroup).ToList();
                    }
                }

                // Retrieve the "normal" groups, which the results of will be
                // already expanded if recursive groups are requested.
                _ = getGroups(that).ToList();

                // At this point, we have a unique set of 'knownGroups' to
                // return.
                foreach (var k in knownGroups.Keys) {
                    yield return knownGroups[k];
                }
            }
        }

        /// <summary>
        /// Gets the LDAP entries for the groups of <paramref name="that"/>
        /// using the specified LDAP <paramref name="connection"/>.
        /// </summary>
        /// <param name="that">The entry to get the groups for.</param>
        /// <param name="connection">The LDAP connection used to retrieve the
        /// entry representing the groups obtained from <paramref name="that"/>.
        /// </param>
        /// <param name="cache">A cache for the LDAP entries which potentially
        /// prevents the method from performing an actual LDAP lookup.</param>
        /// <param name="attributes">The attributes to load for the entry of the
        /// group.</param>
        /// <param name="options">The LDAP options determining the search scope
        /// and the attribute mapping.</param>
        /// <returns>The LDAP entries of all groups except for the primary one.
        /// </returns>
        internal static async Task<IEnumerable<SearchResultEntry>> GetGroupsAsync(
                this SearchResultEntry that,
                LdapConnection connection,
                ILdapCache<SearchResultEntry> cache,
                string[] attributes,
                LdapOptions options) {
            Debug.Assert(that != null);
            Debug.Assert(connection != null);
            Debug.Assert(cache != null);
            Debug.Assert(attributes != null);
            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);

            var groups = that.GetGroups(options);

            var retval = new List<SearchResultEntry>();
            retval.Capacity = groups.Count();

            foreach (var g in groups) {
                var f = $"({options.Mapping.DistinguishedNameAttribute}={g})";
#if WITH_LDAP_CACHE
                var e = (await cache.GetOrAdd(f,
                    attributes,
                    () => connection.SearchAsync(f, attributes, options)))
                    .SingleOrDefault();
#else // WITH_LDAP_CACHE
                var e = (await connection.SearchAsync(f, attributes, options))
                    .SingleOrDefault();
#endif // WITH_LDAP_CACHE
                if (e != null) {
                    retval.Add(e);
                }
            }

            return retval;
        }

        /// <summary>
        /// Gets the identifier for the primary group <paramref name="that"/> is
        /// member of.
        /// </summary>
        /// <param name="that">The entry to get the primary group of.</param>
        /// <param name="options">The LDAP options specifying the attribute to
        /// look for the primary group identifier.</param>
        /// <returns>The primary group ID, which is typically a SID or a
        /// GID number, depending on the type of directory.</returns>
        internal static string? GetPrimaryGroup(
                this SearchResultEntry that,
                LdapOptions options) {
            Debug.Assert(that != null);
            Debug.Assert(options != null);
            var mapping = options.Mapping;
            Debug.Assert(mapping != null);

            var rid = that.GetAttribute(mapping.PrimaryGroupAttribute)
                .GetValue(null)?.ToString();
            if (rid == null) {
                return null;
            }

            var sid = that.GetAttribute(mapping.PrimaryGroupIdentityAttribute)
                .GetValue(SidConverter)?.ToString();
            var endOfDomain = sid?.LastIndexOf('-') ?? -1;
            if (endOfDomain > 0) {
                // If we have an actual SID for the user, assume an AD and
                // convert the RID of the primary group to a SID using the
                // domain part extracted from the user.
                Debug.Assert(sid != null);
                var domain = sid.Substring(0, endOfDomain);
                return $"{domain}-{rid}";

            } else {
                return rid;
            }
        }

        /// <summary>
        /// Gets the LDAP entry for the primary group of <paramref name="that"/>
        /// using the specified LDAP <paramref name="connection"/>.
        /// </summary>
        /// <param name="that">The entry to get the primary group for.</param>
        /// <param name="connection">The LDAP connection used to retrieve the
        /// entry representing the primary group obtained from
        /// <paramref name="that"/>.</param>
        /// <param name="cache">A cache for the LDAP entries which potentially
        /// prevents the method from performing an actual LDAP lookup.</param>
        /// <param name="attributes">The attributes to load for the entry of the
        /// group. Note that this parameter has no effect on the result if the
        /// group was previously cached.</param>
        /// <param name="options">The LDAP options determining the search scope
        /// and the attribute mapping.</param>
        /// <returns>The LDAP entry of the primary group, or <c>null</c> if the
        /// entry does not have a valid primary group within the specified
        /// scope.</returns>
        internal static SearchResultEntry? GetPrimaryGroup(
                this SearchResultEntry that,
                LdapConnection connection,
                ILdapCache<SearchResultEntry> cache,
                string[] attributes,
                LdapOptions options) {
            Debug.Assert(that != null);
            Debug.Assert(connection != null);
            Debug.Assert(cache != null);
            Debug.Assert(attributes != null);
            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);

            var filter = that.GetPrimaryGroupFilter(options);
            if (filter == null) {
                return null;
            }

#if WITH_LDAP_CACHE
            return cache.GetOrAdd(filter,
                attributes,
                () => connection.Search(filter, attributes, options))
                .SingleOrDefault();
#else // WITH_LDAP_CACHE
            return connection.Search(filter, attributes, options)
                .SingleOrDefault();
#endif // WITH_LDAP_CACHE
        }

        /// <summary>
        /// Gets the LDAP entry for the primary group of <paramref name="that"/>
        /// using the specified LDAP <paramref name="connection"/>.
        /// </summary>
        /// <param name="that">The entry to get the primary group for.</param>
        /// <param name="connection">The LDAP connection used to retrieve the
        /// entry representing the primary group obtained from
        /// <paramref name="that"/>.</param>
        /// <param name="cache">A cache for the LDAP entries which potentially
        /// prevents the method from performing an actual LDAP lookup.</param>
        /// <param name="attributes">The attributes to load for the entry of the
        /// group. Note that this parameter has no effect on the result if the
        /// group was previously cached.</param>
        /// <param name="options">The LDAP options determining the search scope
        /// and the attribute mapping.</param>
        /// <returns>The LDAP entry of the primary group, or <c>null</c> if the
        /// entry does not have a valid primary group within the specified
        /// scope.</returns>
        internal static async Task<SearchResultEntry?> GetPrimaryGroupAsync(
                this SearchResultEntry that,
                LdapConnection connection,
                ILdapCache<SearchResultEntry> cache,
                string[] attributes,
                LdapOptions options) {
            Debug.Assert(that != null);
            Debug.Assert(connection != null);
            Debug.Assert(cache != null);
            Debug.Assert(attributes != null);
            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);

            var filter = that.GetPrimaryGroupFilter(options);
            if (filter == null) {
                return null;
            }

#if WITH_LDAP_CACHE
            return (await cache.GetOrAdd(filter,
                attributes,
                () => connection.SearchAsync(filter, attributes, options)))
                .SingleOrDefault();
#else // WITH_LDAP_CACHE
            return (await connection.SearchAsync(filter, attributes, options))
                .SingleOrDefault();
#endif // WITH_LDAP_CACHE
        }

        /// <summary>
        /// Gets an LDAP filter for the primary group of <paramref name="that"/>.
        /// </summary>
        /// <param name="that">The entry to get the primary group of.</param>
        /// <param name="options">The LDAP options specifying the attribute to
        /// look for the primary group identifier.</param>
        /// <returns>An LDAP filter to retrieve the primary group of
        /// <paramref name="that"/>.</returns>
        internal static string? GetPrimaryGroupFilter(
                this SearchResultEntry that,
                LdapOptions options) {
            Debug.Assert(that != null);
            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);
            var retval = that.GetPrimaryGroup(options);

            if (retval != null) {
                var att = options.Mapping.PrimaryGroupIdentityAttribute;
                Debug.Assert(att != null);
                retval = $"({att}={retval})";
            }

            return retval;
        }
        #endregion

        #region Private constants
        private static readonly SidConverter SidConverter = new();
        #endregion
    }
}
