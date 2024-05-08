// <copyright file="LdapEntryExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using Visus.LdapAuthentication.Properties;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Extension methods for <see cref="LdapEntry"/>.
    /// </summary>
    public static class LdapEntryExtensions {

        /// <summary>
        /// Assigns LDAP attributes to the given target object.
        /// </summary>
        /// <remarks>
        /// This method will only assign annotated properties, but not the
        /// claims.
        /// </remarks>
        /// <param name="that">The entry holding the properties to assign.
        /// </param>
        /// <param name="target">The target object to assign the attributes to.
        /// </param>
        /// <param name="schema">The LDAP schema determining the names of the
        /// attributes we search..</param>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or <paramref name="target"/> is <c>null</c>.
        /// </exception>
        public static void AssignTo(this LdapEntry that, object target,
                string schema) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = target ?? throw new ArgumentNullException(nameof(target));
            var props = LdapAttributeAttribute.GetLdapProperties(
                target.GetType(), schema);

            foreach (var p in props.Keys) {
                try {
                    var a = props[p];
                    var v = a.GetValue(that);
                    p.SetValue(target, v);
                } catch (KeyNotFoundException) {
                    Debug.WriteLine($"LDAP attribute \"{p.Name}\" not found, "
                        + "ignoring it while assigning properties.");
                    continue;
                }
            }
        }

        /// <summary>
        /// Assigns LDAP attributes to the given target object.
        /// </summary>
        /// <remarks>
        /// This method will only assign annotated properties, but not the
        /// claims.
        /// </remarks>
        /// <param name="that">The entry holding the properties to assign.
        /// </param>
        /// <param name="target">The target object to assign the attributes to.
        /// </param>
        /// <param name="options">The LDAP options determining the schema that
        /// is used while searching for the LDAP attributes.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or <paramref name="target"/> is <c>null</c>.
        /// </exception>
        public static void AssignTo(this LdapEntry that, object target,
                LdapOptions options)
            => that.AssignTo(target, options?.Schema);

        /// <summary>
        /// Assigns LDAP attributes to the given target object.
        /// </summary>
        /// <remarks>
        /// This method will only assign annotated properties, but not the
        /// claims.
        /// </remarks>
        /// <param name="that">The entry holding the properties to assign.
        /// </param>
        /// <param name="target">The target object to assign the attributes to.
        /// </param>
        /// <param name="options">The LDAP options determining the schema that
        /// is used while searching for the LDAP attributes.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or <paramref name="target"/> is <c>null</c>.
        /// </exception>
        public static void AssignTo(this LdapEntry that, object target,
                IOptions options)
            => that.AssignTo(target, options?.Schema);

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
                this LdapEntry that,
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
                var gid = that.GetPrimaryGroup(mapper, mapping);
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
                    ? that.GetRecursiveGroups(connection, options)
                    : that.GetGroups(connection, options);

                foreach (var g in groups) {
                    try {
                        var a = g.GetAttribute(mapping.GroupIdentityAttribute);
                        var gid = a.ToString(conv);
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
        public static IEnumerable<LdapEntry> GetGroups(
                this LdapEntry that,
                LdapConnection connection,
                IOptions options) {
            _ = that
                ?? throw new ArgumentNullException(nameof(that));
            _ = connection
                ?? throw new ArgumentNullException(nameof(connection));
            _ = options
                ?? throw new ArgumentNullException(nameof(options));

            var mapping = options.Mapping;
            var groups = (string[]) null;

            try {
                groups = that.GetAttribute(options.Mapping.GroupsAttribute)
                    ?.StringValueArray;
            } catch (KeyNotFoundException) {
                // Entry has no group memberships.
                yield break;
            }

            if (groups != null) {
                Debug.WriteLine($"Determining details of {groups.Length} "
                    + $"groups that \"{that.Dn}\" is member of.");

                foreach (var g in groups) {
                    var q = g.EscapeLdapFilterExpression();

                    foreach (var b in options.SearchBases) {
                        var result = connection.Search(
                            b.Key,
                            LdapConnection.ScopeSub,
                            $"({mapping.DistinguishedNameAttribute}={q})",
                            mapping.RequiredGroupAttributes,
                            false);

                        if (result.HasMore()) {
                            var group = result.NextEntry();
                            if (group != null) {
                                yield return group;
                            }
                        }
                    }
                } /* foreach (var g in groups) */
            } /* if (groups != null) */
        }

        /// <summary>
        /// Gets the identity of the primary group of <paramref name="that"/>.
        /// </summary>
        /// <remarks>
        /// This method fails if <paramref name="that"/> does not have a
        /// <see cref="LdapMapping.PrimaryGroupAttribute"/>, which is the case
        /// if <paramref name="that"/> is a group.
        /// </remarks>
        /// <param name="that">The entry to retrieve the primary group of.
        /// </param>
        /// <param name="mapper">A <see cref="LdapUserMapper{TUser}"/> that
        /// provides access to the identity attribute <paramref name="that">,
        /// which is required to obtain the domain part of the primary group SID
        /// in an Active Directory.</param>
        /// <param name="mapping">The LDAP mapping configuration, which allows
        /// the method to determine where the primary group is stored.</param>
        /// <returns>The primary group of <paramref name="that"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or if <paramref name="mapper"/> is <c>null</c>.
        /// </exception>
        public static string GetPrimaryGroup<TUser>(
                this LdapEntry that,
                ILdapUserMapper<TUser> mapper,
                LdapMapping mapping) {
            _ = that
                ?? throw new ArgumentNullException(nameof(that));
            _ = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
            _ = mapping
                ?? throw new ArgumentNullException(nameof(mapping));

            var identity = mapper.GetIdentity(that);
            var att = that.GetAttribute(mapping.PrimaryGroupAttribute);
            var retval = att.ToString((ILdapAttributeConverter) null);

            var endOfDomain = identity.LastIndexOf('-');
            if (endOfDomain > 0) {
                // If we have an actual SID for the user, assume an AD and
                // convert the RID of the primary group to a SID using the
                // domain part extracted from the user.
                var domain = identity.Substring(0, endOfDomain);
                retval = $"{domain}-{retval}";
            }

            return retval;
        }

        /// <summary>
        /// Gets the primary group of <paramref name="that"/>.
        /// </summary>
        /// <remarks>
        /// This method fails if <paramref name="that"/> does not have a
        /// <see cref="LdapMapping.PrimaryGroupAttribute"/>, which is the case
        /// if <paramref name="that"/> is a group.
        /// </remarks>
        /// <param name="that">The entry to retrieve the primary group of.
        /// </param>
        /// <param name="mapper">A <see cref="LdapUserMapper{TUser}"/> that
        /// provides access to the identity attribute <paramref name="that">,
        /// which is required to obtain the domain part of the primary group SID
        /// in an Active Directory.</param>
        /// <param name="connection">An <see cref="LdapConnection"/> to retrieve
        /// the details about the primary group, which is stored as an
        /// identifier in <paramref name="that"/>.</param>
        /// <param name="options">The <see cref="LdapOptions"/> configuring the
        /// mapping of attributes.</param>
        /// <returns>The LDAP entry of the primary group.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or if <paramref name="mapper"/> is <c>null</c>,
        /// or if <paramref name="connection"/> is <c>null</c>, or if
        /// <paramref name="options"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">If <paramref name="that"/>
        /// has a primary group, but its LDAP entry was not found in the
        /// configured search path.</exception>
        public static LdapEntry GetPrimaryGroup<TUser>(
                this LdapEntry that,
                ILdapUserMapper<TUser> mapper,
                LdapConnection connection,
                LdapOptions options) {
            _ = that
                ?? throw new ArgumentNullException(nameof(that));
            _ = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
            _ = connection
                ?? throw new ArgumentNullException(nameof(connection));
            _ = options
                ?? throw new ArgumentNullException(nameof(options));

            var mapping = options.Mapping;
            var gid = that.GetPrimaryGroup(mapper, mapping);

            foreach (var b in options.SearchBases) {
                var result = connection.Search(
                    b.Key,
                    LdapConnection.ScopeSub,
                    $"({mapping.GroupIdentityAttribute}={gid})",
                    mapping.RequiredGroupAttributes,
                    false);

                if (result.HasMore()) {
                    var group = result.NextEntry();
                    if (group != null) {
                        return group;
                    }
                }
            }

            throw new KeyNotFoundException(
                string.Format(Resources.ErrorGroupNotFound, gid));
        }

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
        public static IEnumerable<LdapEntry> GetRecursiveGroups(
                this LdapEntry that,
                LdapConnection connection,
                IOptions options) {
            var stack = new Stack<LdapEntry>();
            stack.Push(that);

            while (stack.Count > 0) {
                foreach (var g in GetGroups(stack.Pop(), connection,
                        options)) {
                    stack.Push(g);
                    yield return g;
                }
            }
        }
    }
}
