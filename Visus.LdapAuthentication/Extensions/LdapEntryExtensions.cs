// <copyright file="LdapEntryExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Visus.Ldap;
using Visus.Ldap.Extensions;
using Visus.Ldap.Mapping;
using Visus.LdapAuthentication.Configuration;


namespace Visus.LdapAuthentication.Extensions {

    /// <summary>
    /// Extension methods for <see cref="LdapEntry"/>.
    /// </summary>
    internal static class LdapEntryExtensions {

        #region Internal methods
        /// <summary>
        /// Gets an LDAP filter that selects the given entry via its DN.
        /// </summary>
        /// <param name="that">The entry to get the filter for.</param>
        /// <param name="options">The <see cref="LdapOptions"/> that provide the
        /// information what the name of the DN attribute is.</param>
        /// <returns>A filter for the given entry.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetFilter(this LdapEntry that,
                LdapOptions options) {
            Debug.Assert(that != null);
            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);
            return $"({options.Mapping.DistinguishedNameAttribute}={that.Dn})";
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
                this LdapEntry that,
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
        internal static IEnumerable<LdapEntry> GetGroups(
                this LdapEntry that,
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
                return Enumerable.Empty<LdapEntry>();
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
        /// <param name="objectCache"></param>
        /// <param name="entryCache"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        internal static IEnumerable<TGroup> GetGroups<TUser, TGroup>(
                this LdapEntry that,
                LdapConnection connection,
                ILdapMapper<LdapEntry, TUser, TGroup> mapper,
                ILdapObjectCache<TUser, TGroup> objectCache,
                ILdapEntryCache<LdapEntry> entryCache,
                LdapOptions options)
                where TGroup : new() {
            Debug.Assert(that != null);
            Debug.Assert(connection != null);
            Debug.Assert(mapper != null);
            Debug.Assert(objectCache != null);
            Debug.Assert(entryCache != null);
            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);

            var attributes = mapper.RequiredGroupAttributes
                .Append(options.Mapping.PrimaryGroupAttribute)
                .Append(options.Mapping.PrimaryGroupIdentityAttribute)
                .Append(options.Mapping.GroupsAttribute)
                .ToArray();

            // Obtain the primary group first.
            var primaryGroupFilter = that.GetPrimaryGroupFilter(options);
            var primaryGroup = (primaryGroupFilter != null)
                ? objectCache.GetGroup(primaryGroupFilter,
                    f => {
                        // Note: using the GetPrimaryGroup method here instead
                        // of performing a search on 'connection' ensures that
                        // the entry will be cached, too.
                        var e = that.GetPrimaryGroup(connection,
                            entryCache,
                            attributes,
                            options);
                        return (e != null)
                            ? mapper.MapPrimaryGroup(e, new TGroup())
                            : default;
                    })
                : default;

            if (primaryGroup != null) {
                yield return primaryGroup;
            }

            var groups = that.GetGroups(connection, attributes, options);

            if (options.IsRecursiveGroupMembership) {
                // Accumulate all groups into one enumeration.
                var stack = new Stack<LdapEntry>(groups);

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
                    var parents = g.GetGroups(connection, mapper, objectCache,
                        entryCache, options);
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
        /// <param name="cache">A cache for the LDAP entries which potentially
        /// prevents the method from performing an actual LDAP lookup.</param>
        /// <param name="attributes">The attributes to load for the entry of the
        /// group. Note that this parameter has no effect on the result if the
        /// group was previously cached.</param>
        /// <param name="options">The LDAP options determining the search scope
        /// and the attribute mapping.</param>
        /// <returns>The LDAP entries of all groups except for the primary one.
        /// </returns>
        internal static async Task<IEnumerable<LdapEntry>> GetGroupsAsync(
                this LdapEntry that,
                LdapConnection connection,
                ILdapEntryCache<LdapEntry> cache,
                string[] attributes,
                LdapOptions options) {
            Debug.Assert(that != null);
            Debug.Assert(connection != null);
            Debug.Assert(cache != null);
            Debug.Assert(attributes != null);
            Debug.Assert(options != null);
            Debug.Assert(options.Mapping != null);

            var groups = that.GetGroups(options);

            var retval = new List<LdapEntry>();
            retval.Capacity = groups.Count();

            foreach (var g in groups) {
                var gg = $"({options.Mapping.DistinguishedNameAttribute}={g})";
                var e = await cache.GetEntry(gg, async f =>
                    (await connection.SearchAsync(f, attributes, options))
                    .SingleOrDefault());
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
                this LdapEntry that,
                LdapOptions options) {
            Debug.Assert(that != null);
            Debug.Assert(options != null);
            var mapping = options.Mapping;
            Debug.Assert(mapping != null);

            var rid = that.GetAttribute(mapping.PrimaryGroupAttribute)
                .GetValue(typeof(string), null) as string;
            if (rid == null) {
                return null;
            }

            var sid = that.GetAttribute(mapping.PrimaryGroupIdentityAttribute)
                .GetValue(typeof(string), SidConverter) as string;
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
        internal static LdapEntry? GetPrimaryGroup(
                this LdapEntry that,
                LdapConnection connection,
                ILdapEntryCache<LdapEntry> cache,
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

            return cache.GetEntry(filter,
                f => connection.Search(f, attributes, options)
                .FirstOrDefault());
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
        internal static async Task<LdapEntry?> GetPrimaryGroupAsync(
                this LdapEntry that,
                LdapConnection connection,
                ILdapEntryCache<LdapEntry> cache,
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

            return await cache.GetEntry(filter, async f => {
                return (await connection.SearchAsync(f, attributes, options))
                    .FirstOrDefault();
            });
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
                this LdapEntry that,
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

        ///// <summary>
        ///// Assigns LDAP attributes to the given target object.
        ///// </summary>
        ///// <remarks>
        ///// This method will only assign annotated properties, but not the
        ///// claims.
        ///// </remarks>
        ///// <param name="that">The entry holding the properties to assign.
        ///// </param>
        ///// <param name="target">The target object to assign the attributes to.
        ///// </param>
        ///// <param name="schema">The LDAP schema determining the names of the
        ///// attributes we search..</param>
        ///// <exception cref="ArgumentNullException">If <paramref name="that"/>
        ///// is <c>null</c>, or <paramref name="target"/> is <c>null</c>.
        ///// </exception>
        //public static void AssignTo(this LdapEntry that, object target,
        //        string schema) {
        //    _ = that ?? throw new ArgumentNullException(nameof(that));
        //    _ = target ?? throw new ArgumentNullException(nameof(target));
        //    var props = LdapAttributeAttribute.GetLdapProperties(
        //        target.GetType(), schema);

        //    foreach (var p in props.Keys) {
        //        try {
        //            var a = props[p];
        //            var v = a.GetValue(that);
        //            p.SetValue(target, v);
        //        } catch (KeyNotFoundException) {
        //            Debug.WriteLine($"LDAP attribute \"{p.Name}\" not found, "
        //                + "ignoring it while assigning properties.");
        //            continue;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Assigns LDAP attributes to the given target object.
        ///// </summary>
        ///// <remarks>
        ///// This method will only assign annotated properties, but not the
        ///// claims.
        ///// </remarks>
        ///// <param name="that">The entry holding the properties to assign.
        ///// </param>
        ///// <param name="target">The target object to assign the attributes to.
        ///// </param>
        ///// <param name="options">The LDAP options determining the schema that
        ///// is used while searching for the LDAP attributes.</param>
        ///// <exception cref="ArgumentNullException">If <paramref name="that"/>
        ///// is <c>null</c>, or <paramref name="target"/> is <c>null</c>.
        ///// </exception>
        //public static void AssignTo(this LdapEntry that, object target,
        //        LdapOptions options)
        //    => that.AssignTo(target, options?.Schema);

        ///// <summary>
        ///// Assigns LDAP attributes to the given target object.
        ///// </summary>
        ///// <remarks>
        ///// This method will only assign annotated properties, but not the
        ///// claims.
        ///// </remarks>
        ///// <param name="that">The entry holding the properties to assign.
        ///// </param>
        ///// <param name="target">The target object to assign the attributes to.
        ///// </param>
        ///// <param name="options">The LDAP options determining the schema that
        ///// is used while searching for the LDAP attributes.</param>
        ///// <exception cref="ArgumentNullException">If <paramref name="that"/>
        ///// is <c>null</c>, or <paramref name="target"/> is <c>null</c>.
        ///// </exception>
        //public static void AssignTo(this LdapEntry that, object target,
        //        IOptions options)
        //    => that.AssignTo(target, options?.Schema);

        ///// <summary>
        ///// Gets the <see cref="Claim"/>s that represent the group memberships
        ///// of <paramref name="that"/>.
        ///// </summary>
        ///// <param name="that">The user entry to get the group claims of.
        ///// </param>
        ///// <param name="mapper">A <see cref="LdapUserMapper{TUser}"/> that
        ///// provides access to the identity attribute of the user object the
        ///// claims are for.</param>
        ///// <param name="connection">An <see cref="LdapConnection"/> to search
        ///// for the details of the groups.</param>
        ///// <param name="options">The <see cref="LdapOptions"/> determining the
        ///// schema and whether group memberships are added recursively.</param>
        ///// <param name="logger">An optional logger to record error messages.
        ///// </param>
        //public static IEnumerable<Claim> GetGroupClaims<TUser>(
        //        this LdapEntry that,
        //        ILdapUserMapper<TUser> mapper,
        //        LdapConnection connection,
        //        LdapOptions options,
        //        ILogger logger = null) {
        //    _ = that
        //        ?? throw new ArgumentNullException(nameof(that));
        //    _ = connection
        //        ?? throw new ArgumentNullException(nameof(connection));
        //    _ = options
        //        ?? throw new ArgumentNullException(nameof(options));

        //    var identity = mapper.GetIdentity(that);
        //    var mapping = options.Mapping;
        //    var retval = new List<Claim>();

        //    try {
        //        var gid = that.GetPrimaryGroup(mapper, mapping);
        //        retval.Add(new Claim(ClaimTypes.PrimaryGroupSid, gid));
        //        retval.Add(new Claim(ClaimTypes.GroupSid, gid));
        //    } catch (Exception ex) {
        //        logger?.LogError(ex,
        //            Resources.ErrorPrimaryGroupClaim,
        //            identity);
        //    }

        //    {
        //        var conv = mapping.GetGroupIdentityConverter();
        //        var groups = options.IsRecursiveGroupMembership
        //            ? that.GetRecursiveGroups(connection, options)
        //            : that.GetGroups(connection, options);

        //        foreach (var g in groups) {
        //            try {
        //                var a = g.GetAttribute(mapping.GroupIdentityAttribute);
        //                var gid = a.ToString(conv);
        //                retval.Add(new Claim(ClaimTypes.GroupSid, gid));
        //            } catch (Exception ex) {
        //                logger?.LogError(ex,
        //                    Resources.ErrorGroupClaim,
        //                    identity);
        //            }
        //        }
        //    }

        //    return retval;
        //}

        ///// <summary>
        ///// Gets the group memberships of the specified (user or group) LDAP
        ///// entry.
        ///// </summary>
        ///// <param name="that">The entry to retrieve the group memberships of.
        ///// </param>
        ///// <param name="connection">An <see cref="LdapConnection"/> to retrieve
        ///// the details about the groups.</param>
        ///// <param name="options">The <see cref="LdapOptions"/> configuring the
        ///// mapping of attributes.</param>
        ///// <returns>The LDAP entries for the groups <paramref name="that"/> is
        ///// member of.</returns>
        ///// <exception cref="ArgumentNullException">If <paramref name="that"/>
        ///// is <c>null</c>.</exception>
        ///// <exception cref="ArgumentNullException">If
        ///// <paramref name="connection"/> is <c>null</c>.</exception>
        ///// <exception cref="ArgumentNullException">If
        ///// <paramref name="options"/> is <c>null</c>.</exception>
        //public static IEnumerable<LdapEntry> GetGroups(
        //        this LdapEntry that,
        //        LdapConnection connection,
        //        IOptions options) {
        //    _ = that
        //        ?? throw new ArgumentNullException(nameof(that));
        //    _ = connection
        //        ?? throw new ArgumentNullException(nameof(connection));
        //    _ = options
        //        ?? throw new ArgumentNullException(nameof(options));

        //    var mapping = options.Mapping;
        //    var groups = (string[]) null;

        //    try {
        //        groups = that.GetAttribute(options.Mapping.GroupsAttribute)
        //            ?.StringValueArray;
        //    } catch (KeyNotFoundException) {
        //        // Entry has no group memberships.
        //        yield break;
        //    }

        //    if (groups != null) {
        //        Debug.WriteLine($"Determining details of {groups.Length} "
        //            + $"groups that \"{that.Dn}\" is member of.");

        //        foreach (var g in groups) {
        //            var q = g.EscapeLdapFilterExpression();

        //            foreach (var b in options.SearchBases) {
        //                var result = connection.Search(
        //                    b.Key,
        //                    LdapConnection.ScopeSub,
        //                    $"({mapping.DistinguishedNameAttribute}={q})",
        //                    mapping.RequiredGroupAttributes,
        //                    false);

        //                if (result.HasMore()) {
        //                    var group = result.NextEntry();
        //                    if (group != null) {
        //                        yield return group;
        //                    }
        //                }
        //            }
        //        } /* foreach (var g in groups) */
        //    } /* if (groups != null) */
        //}

        ///// <summary>
        ///// Gets the identity of the primary group of <paramref name="that"/>.
        ///// </summary>
        ///// <remarks>
        ///// This method fails if <paramref name="that"/> does not have a
        ///// <see cref="LdapMapping.PrimaryGroupAttribute"/>, which is the case
        ///// if <paramref name="that"/> is a group.
        ///// </remarks>
        ///// <param name="that">The entry to retrieve the primary group of.
        ///// </param>
        ///// <param name="mapper">A <see cref="LdapUserMapper{TUser}"/> that
        ///// provides access to the identity attribute <paramref name="that">,
        ///// which is required to obtain the domain part of the primary group SID
        ///// in an Active Directory.</param>
        ///// <param name="mapping">The LDAP mapping configuration, which allows
        ///// the method to determine where the primary group is stored.</param>
        ///// <returns>The primary group of <paramref name="that"/>.</returns>
        ///// <exception cref="ArgumentNullException">If <paramref name="that"/>
        ///// is <c>null</c>, or if <paramref name="mapper"/> is <c>null</c>.
        ///// </exception>
        //public static string GetPrimaryGroup<TUser>(
        //        this LdapEntry that,
        //        ILdapUserMapper<TUser> mapper,
        //        LdapMapping mapping) {
        //    _ = that
        //        ?? throw new ArgumentNullException(nameof(that));
        //    _ = mapper
        //        ?? throw new ArgumentNullException(nameof(mapper));
        //    _ = mapping
        //        ?? throw new ArgumentNullException(nameof(mapping));

        //    var identity = mapper.GetIdentity(that);
        //    var att = that.GetAttribute(mapping.PrimaryGroupAttribute);
        //    var retval = att.ToString((ILdapAttributeConverter) null);

        //    var endOfDomain = identity.LastIndexOf('-');
        //    if (endOfDomain > 0) {
        //        // If we have an actual SID for the user, assume an AD and
        //        // convert the RID of the primary group to a SID using the
        //        // domain part extracted from the user.
        //        var domain = identity.Substring(0, endOfDomain);
        //        retval = $"{domain}-{retval}";
        //    }

        //    return retval;
        //}

        ///// <summary>
        ///// Gets the primary group of <paramref name="that"/>.
        ///// </summary>
        ///// <remarks>
        ///// This method fails if <paramref name="that"/> does not have a
        ///// <see cref="LdapMapping.PrimaryGroupAttribute"/>, which is the case
        ///// if <paramref name="that"/> is a group.
        ///// </remarks>
        ///// <param name="that">The entry to retrieve the primary group of.
        ///// </param>
        ///// <param name="mapper">A <see cref="LdapUserMapper{TUser}"/> that
        ///// provides access to the identity attribute <paramref name="that">,
        ///// which is required to obtain the domain part of the primary group SID
        ///// in an Active Directory.</param>
        ///// <param name="connection">An <see cref="LdapConnection"/> to retrieve
        ///// the details about the primary group, which is stored as an
        ///// identifier in <paramref name="that"/>.</param>
        ///// <param name="options">The <see cref="LdapOptions"/> configuring the
        ///// mapping of attributes.</param>
        ///// <returns>The LDAP entry of the primary group.</returns>
        ///// <exception cref="ArgumentNullException">If <paramref name="that"/>
        ///// is <c>null</c>, or if <paramref name="mapper"/> is <c>null</c>,
        ///// or if <paramref name="connection"/> is <c>null</c>, or if
        ///// <paramref name="options"/> is <c>null</c>.</exception>
        ///// <exception cref="KeyNotFoundException">If <paramref name="that"/>
        ///// has a primary group, but its LDAP entry was not found in the
        ///// configured search path.</exception>
        //public static LdapEntry GetPrimaryGroup<TUser>(
        //        this LdapEntry that,
        //        ILdapUserMapper<TUser> mapper,
        //        LdapConnection connection,
        //        LdapOptions options) {
        //    _ = that
        //        ?? throw new ArgumentNullException(nameof(that));
        //    _ = mapper
        //        ?? throw new ArgumentNullException(nameof(mapper));
        //    _ = connection
        //        ?? throw new ArgumentNullException(nameof(connection));
        //    _ = options
        //        ?? throw new ArgumentNullException(nameof(options));

        //    var mapping = options.Mapping;
        //    var gid = that.GetPrimaryGroup(mapper, mapping);

        //    foreach (var b in options.SearchBases) {
        //        var result = connection.Search(
        //            b.Key,
        //            LdapConnection.ScopeSub,
        //            $"({mapping.GroupIdentityAttribute}={gid})",
        //            mapping.RequiredGroupAttributes,
        //            false);

        //        if (result.HasMore()) {
        //            var group = result.NextEntry();
        //            if (group != null) {
        //                return group;
        //            }
        //        }
        //    }

        //    throw new KeyNotFoundException(
        //        string.Format(Resources.ErrorGroupNotFound, gid));
        //}

        ///// <summary>
        ///// Gets the direct and transitive group memberships of the specified
        ///// (user or group) LDAP entry.
        ///// </summary>
        ///// <param name="that">The entry to retrieve the group memberships of.
        ///// </param>
        ///// <param name="connection">An <see cref="LdapConnection"/> to retrieve
        ///// the details about the groups.</param>
        ///// <param name="options">The <see cref="LdapOptions"/> configuring the
        ///// mapping of attributes.</param>
        ///// <returns>The LDAP entries for the groups <paramref name="that"/> is
        ///// member of.</returns>
        ///// <exception cref="ArgumentNullException">If <paramref name="that"/>
        ///// is <c>null</c>.</exception>
        ///// <exception cref="ArgumentNullException">If
        ///// <paramref name="connection"/> is <c>null</c>.</exception>
        ///// <exception cref="ArgumentNullException">If
        ///// <paramref name="options"/> is <c>null</c>.</exception>
        //public static IEnumerable<LdapEntry> GetRecursiveGroups(
        //        this LdapEntry that,
        //        LdapConnection connection,
        //        IOptions options) {
        //    var stack = new Stack<LdapEntry>();
        //    stack.Push(that);

        //    while (stack.Count > 0) {
        //        foreach (var g in stack.Pop().GetGroups(connection,
        //                options)) {
        //            stack.Push(g);
        //            yield return g;
        //        }
        //    }
        //}
    }
}
