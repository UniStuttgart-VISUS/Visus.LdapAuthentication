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
        /// <param name="that">The entry to get the groups for.</param>
        /// <param name="connection">The LDAP connection used to retrieve the
        /// entry representing the groups obtained from <paramref name="that"/>.
        /// </param>
        /// <param name="connection">The LDAP connection used to retrieve the
        /// entry of the group.</param>
        /// <param name="attributes">The attributes to load for the entry of the
        /// group.</param>
        /// <param name="options">The LDAP options determining the search scope
        /// and the attribute mapping.</param>
        /// <returns>The LDAP entries of all groups except for the primary one.
        /// </returns>
        internal static IEnumerable<SearchResultEntry> GetGroups(
                this SearchResultEntry that,
                LdapConnection connection,
                string[] attributes,
                LdapOptions options) {
            ArgumentNullException.ThrowIfNull(connection, nameof(connection));
            var groups = that.GetGroups(options);

            if (groups.Any()) {
                Debug.Assert(options != null);
                Debug.Assert(options.Mapping != null);
                groups = groups.Select(g => g.EscapeLdapFilterExpression()!);
                groups = groups.Select(
                    g => $"({options.Mapping.DistinguishedNameAttribute}={g})");
                return connection.Search($"(|{string.Join("", groups)})",
                    attributes, options);
            } else {
                return Enumerable.Empty<SearchResultEntry>();
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
        /// <param name="connection"></param>
        /// <param name="mapper"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        internal static IEnumerable<TGroup> GetGroups<TUser, TGroup>(
                this SearchResultEntry that,
                LdapConnection connection,
                ILdapMapper<SearchResultEntry, TUser, TGroup> mapper,
                ILdapGroupCache<TGroup> groupCache,
                LdapOptions options)
                where TGroup : new() {
            ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
            Debug.Assert(options.Mapping != null);
            var attributes = mapper.RequiredGroupAttributes
                .Append(options.Mapping.PrimaryGroupAttribute)
                .Append(options.Mapping.PrimaryGroupIdentityAttribute)
                .Append(options.Mapping.GroupsAttribute)
                .ToArray();

            // Obtain the primary group first.
            var primaryGroup = that.GetPrimaryGroup(connection,
                attributes,
                options);
            if (primaryGroup != null) {
                yield return mapper.SetPrimary(mapper.MapGroup(primaryGroup,
                    new TGroup()), true);
            }

            var groups = that.GetGroups(connection, attributes, options);

            if (options.IsRecursiveGroupMembership) {
                // Accumulate all groups into one enumeration.
                var stack = new Stack<SearchResultEntry>(groups);

                while (stack.Any()) {
                    var group = stack.Pop();
                    yield return mapper.MapGroup(group, new TGroup());

                    groups = group.GetGroups(connection, attributes, options);
                    foreach (var g in groups) {
                        yield return mapper.MapGroup(g, new TGroup());
                        stack.Push(g);
                    }
                }

            } else if (mapper.GroupIsGroupMember) {
                // Recursively reconstruct the hierarchy itself.
                foreach (var g in groups) {
                    var group = mapper.MapGroup(g, new TGroup());
                    var parents = g.GetGroups(connection, mapper, groupCache,
                        options);
                    yield return mapper.SetGroups(group, parents);
                }

            } else {
                // The options do not require groups to be evaluated
                // recursively, so we can just map and return them.
                foreach (var g in groups) {
                    yield return mapper.MapGroup(g, new TGroup());
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
        /// <param name="connection">The LDAP connection used to retrieve the
        /// entry of the group.</param>
        /// <param name="attributes">The attributes to load for the entry of the
        /// group.</param>
        /// <param name="options">The LDAP options determining the search scope
        /// and the attribute mapping.</param>
        /// <returns>The LDAP entries of all groups except for the primary one.
        /// </returns>
        internal static Task<IEnumerable<SearchResultEntry>> GetGroupsAsync(
                this SearchResultEntry that,
                LdapConnection connection,
                string[] attributes,
                LdapOptions options) {
            ArgumentNullException.ThrowIfNull(connection, nameof(connection));
            var groups = that.GetGroups(options);

            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);
            groups = groups.Select(
                g => $"({options.Mapping.DistinguishedNameAttribute}={g})");

            return connection.SearchAsync($"(|{string.Join("", groups)})",
                attributes, options);
        }

        /// <summary>
        /// Gets the identifier for the primary group <paramref name="that"/> is
        /// member of.
        /// </summary>
        /// <param name="that"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        internal static string? GetPrimaryGroup(
                this SearchResultEntry that,
                LdapOptions options) {
            ArgumentNullException.ThrowIfNull(that, nameof(that));
            ArgumentNullException.ThrowIfNull(options, nameof(options));
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
        /// <param name="connection">The LDAP connection used to retrieve the
        /// entry of the group.</param>
        /// <param name="attributes">The attributes to load for the entry of the
        /// group.</param>
        /// <param name="options">The LDAP options determining the search scope
        /// and the attribute mapping.</param>
        /// <returns>The LDAP entry of the primary group, or <c>null</c> if the
        /// entry does not have a valid primary group within the specified
        /// scope.</returns>
        internal static SearchResultEntry? GetPrimaryGroup(
                this SearchResultEntry that,
                LdapConnection connection,
                string[] attributes,
                LdapOptions options) {
            ArgumentNullException.ThrowIfNull(connection, nameof(connection));
            var id = that.GetPrimaryGroup(options);
            if (id == null) {
                return null;
            }

            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);
            var retval = connection.Search(
                $"({options.Mapping.PrimaryGroupIdentityAttribute}={id})",
                attributes,
                options);

            return retval.FirstOrDefault();
        }

        /// <summary>
        /// Gets the LDAP entry for the primary group of <paramref name="that"/>
        /// using the specified LDAP <paramref name="connection"/>.
        /// </summary>
        /// <param name="that">The entry to get the primary group for.</param>
        /// <param name="connection">The LDAP connection used to retrieve the
        /// entry representing the primary group obtained from
        /// <paramref name="that"/>.</param>
        /// <param name="connection">The LDAP connection used to retrieve the
        /// entry of the group.</param>
        /// <param name="attributes">The attributes to load for the entry of the
        /// group.</param>
        /// <param name="options">The LDAP options determining the search scope
        /// and the attribute mapping.</param>
        /// <returns>The LDAP entry of the primary group, or <c>null</c> if the
        /// entry does not have a valid primary group within the specified
        /// scope.</returns>
        internal static async Task<SearchResultEntry?> GetPrimaryGroupAsync(
                this SearchResultEntry that,
                LdapConnection connection,
                string[] attributes,
                LdapOptions options) {
            ArgumentNullException.ThrowIfNull(connection, nameof(connection));
            var id = that.GetPrimaryGroup(options);
            if (id == null) {
                return null;
            }

            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);
            var retval = await connection.SearchAsync(
                $"({options.Mapping.PrimaryGroupIdentityAttribute}={id})",
                attributes,
                options);

            return retval.FirstOrDefault();
        }
        #endregion

        #region Private constants
        private static readonly SidConverter SidConverter = new();
        #endregion
    }
}
