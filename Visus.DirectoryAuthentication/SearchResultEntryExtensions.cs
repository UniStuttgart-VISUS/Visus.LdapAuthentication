// <copyright file="SearchResultEntryExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2022 -2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Visus.DirectoryAuthentication.Properties;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Extension methods for <see cref="SearchResultEntry"/>.
    /// </summary>
    public static class SearchResultEntryExtensions {

        /// <summary>
        /// Assigns LDAP attributes to the given target object.
        /// </summary>
        /// <param name="that">The entry holding the properties to assign.
        /// </param>
        /// <param name="target">The target object to assign the attributes to.
        /// </param>
        /// <param name="schema">The LDAP schema determining the names of the
        /// attributes we search.</param>
        /// <param name="logger">An optional logger to record error messages.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or <paramref name="target"/> is <c>null</c>.
        /// </exception>
        public static void AssignTo(this SearchResultEntry that,
                object target,
                string schema,
                ILogger logger = null) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = target ?? throw new ArgumentNullException(nameof(target));
            var props = LdapAttributeAttribute.GetLdapProperties(
                target.GetType(), schema);

            foreach (var p in props.Keys) {
                try {
                    var a = props[p];
                    var v = a.GetValue(that);
                    p.SetValue(target, v);
                } catch (KeyNotFoundException ex) {
                    logger?.LogError(ex,
                        Resources.ErrorAttributeNotFound,
                        p.Name);
                    continue;
                }
            }
        }

        /// <summary>
        /// Assigns LDAP attributes to the given target object.
        /// </summary>
        /// <param name="that">The entry holding the properties to assign.
        /// </param>
        /// <param name="target">The target object to assign the attributes to.
        /// </param>
        /// <param name="options">The LDAP options determining the schema that
        /// is used while searching for the LDAP attributes.</param>
        /// <param name="logger">An optional logger to record error messages.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or <paramref name="target"/> is <c>null</c>.
        /// </exception>
        public static void AssignTo(this SearchResultEntry that, object target,
                LdapOptions options, ILogger logger = null)
            => that.AssignTo(target, options?.Schema, logger);

        /// <summary>
        /// Gets the <see cref="Claim"/>s that represent the group memberships
        /// of <paramref name="that"/>.
        /// </summary>
        /// <param name="that">The user entry to get the group claims of.
        /// </param>
        /// <param name="mapper">A <see cref="LdapUserMapper{TUser}"/> that
        /// provides access to the identity attribute of the user object the
        /// claims are for.</param>
        /// <param name="connection">An <see cref="LdapConnection"/> to search
        /// for the details of the groups.</param>
        /// <param name="options">The <see cref="LdapOptions"/> determining the
        /// schema and whether group memberships are added recursively.</param>
        /// <param name="logger">An optional logger to record error messages.
        /// </param>
        public static IEnumerable<Claim> GetGroupClaims<TUser>(
                this SearchResultEntry that,
                ILdapUserMapper<TUser> mapper,
                LdapConnection connection,
                LdapOptions options,
                ILogger logger = null) {
            _ = that
                ?? throw new ArgumentNullException(nameof(that));
            _ = connection
                ?? throw new ArgumentNullException(nameof(connection));
            _ = options
                ?? throw new ArgumentNullException(nameof(options));

            var identity = mapper.GetIdentity(that);
            var mapping = options.Mapping;
            var retval = new List<Claim>();

            try {
                var a = that.GetAttribute(mapping.PrimaryGroupAttribute);
                var gid = a.GetValue((ILdapAttributeConverter) null) as string;

                var endOfDomain = identity.LastIndexOf('-');
                if (endOfDomain > 0) {
                    // If we have an actual SID for the user, assume an AD and
                    // convert the RID of the primary group to a SID using the
                    // domain part extracted from the user.
                    var domain = identity.Substring(0, endOfDomain);
                    gid = $"{domain}-{gid}";
                }

                retval.Add(new Claim(ClaimTypes.PrimaryGroupSid, gid));
                retval.Add(new Claim(ClaimTypes.GroupSid, gid));
            } catch (Exception ex) {
                logger?.LogError(ex,
                    Resources.ErrorPrimaryGroupClaim,
                    identity);
            }

            {
                var conv = mapping.GetGroupIdentityConverter();
                var groups = options.IsRecursiveGroupMembership
                    ? GetAllGroupMemberships(that, connection, options)
                    : GetGroupMemberships(that, connection, options);

                foreach (var g in groups) {
                    try {
                        var a = g.GetAttribute(mapping.GroupIdentityAttribute);
                        var gid = a.GetValue(conv) as string;
                        retval.Add(new Claim(ClaimTypes.GroupSid, gid));
                    } catch (Exception ex) {
                        logger?.LogError(ex,
                            Resources.ErrorGroupClaim,
                            identity);
                    }
                }
            }

            return retval;
        }

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
        internal static DirectoryAttribute GetAttribute(
                this SearchResultEntry that, string attribute) {
            return that?.Attributes[attribute];
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets the direct and transitive group memberships of the specified
        /// (user or group) LDAP entry.
        /// </summary>
        /// <param name="that">The entry to retrieve the group memberships of.
        /// </param>
        /// <param name="connection">An <see cref="LdapConnection"/> to retrieve
        /// the details about the groups.</param>
        /// <param name="options">The <see cref="LdapOptions"/> configuring the
        /// mapping of attributes.</param>
        /// <returns>The LDAP entries for the groups <paramref name="that"/> is
        /// member of.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="connection"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        private static IEnumerable<SearchResultEntry> GetAllGroupMemberships(
                this SearchResultEntry that,
                LdapConnection connection,
                LdapOptions options) {
            var stack = new Stack<SearchResultEntry>();
            stack.Push(that);

            while (stack.Count > 0) {
                foreach (var g in GetGroupMemberships(stack.Pop(), connection,
                        options)) {
                    stack.Push(g);
                    yield return g;
                }
            }
        }

        /// <summary>
        /// Gets the group memberships of the specified (user or group) LDAP
        /// entry.
        /// </summary>
        /// <param name="that">The entry to retrieve the group memberships of.
        /// </param>
        /// <param name="connection">An <see cref="LdapConnection"/> to retrieve
        /// the details about the groups.</param>
        /// <param name="options">The <see cref="LdapOptions"/> configuring the
        /// mapping of attributes.</param>
        /// <returns>The LDAP entries for the groups <paramref name="that"/> is
        /// member of.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="connection"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        private static IEnumerable<SearchResultEntry> GetGroupMemberships(
                this SearchResultEntry that,
                LdapConnection connection,
                LdapOptions options) {
            _ = that
                ?? throw new ArgumentNullException(nameof(that));
            _ = connection
                ?? throw new ArgumentNullException(nameof(connection));
            _ = options
                ?? throw new ArgumentNullException(nameof(options));

            var mapping = options.Mapping;
            var groups = (string[]) null;

            try {
                var att = that.GetAttribute(options.Mapping.GroupsAttribute);
                if (att != null) {
                    groups = att.GetValues(typeof(string))
                        .Cast<string>()
                        .ToArray();
                }
            } catch /* TODO: More specific exception? */ {
                // Entry has no group memberships.
                yield break;
            }

            if (groups != null) {
                Debug.WriteLine($"Determining details of {groups.Length} "
                    + $"groups that \"{that.DistinguishedName}\" is member "
                    + "of.");

                foreach (var g in groups) {
                    var q = g.EscapeLdapFilterExpression();

                    foreach (var b in options.SearchBase) {
                        var req = new SearchRequest(b.Key,
                            $"({mapping.DistinguishedNameAttribute}={q})",
                            SearchScope.Subtree,
                            mapping.RequiredGroupAttributes);
                        var res = connection.SendRequest(req, options);

                        if (res is SearchResponse r) {
                            foreach (SearchResultEntry e in r.Entries) {
                                yield return e;
                            }
                        }
                    }
                } /* foreach (var g in groups) */
            } /* if (groups != null) */
        }
        #endregion
    }
}
