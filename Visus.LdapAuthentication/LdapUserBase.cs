// <copyright file="LdapUserBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json.Serialization;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Base class for implementing custom users.
    /// </summary>
    public abstract class LdapUserBase : ILdapUser {

        #region Public properties
        /// <inheritdoc />
        [LdapAttribute(Schema.ActiveDirectory, "sAMAccountName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sAMAccountName")]
        [LdapAttribute(Schema.Rfc2307, "uid")]
        [Claim(ClaimTypes.Name)]
        [Claim(ClaimTypes.WindowsAccountName)]
        public virtual string AccountName { get; internal set; }

        /// <inheritdoc />
        [LdapAttribute(Schema.ActiveDirectory, "givenName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "givenName")]
        [LdapAttribute(Schema.Rfc2307, "givenName")]
        [Claim(ClaimTypes.GivenName)]
        public virtual string ChristianName { get; internal set; }

        /// <inheritdoc />
        public virtual IEnumerable<Claim> Claims { get; } = new List<Claim>();

        /// <inheritdoc />
        [LdapAttribute(Schema.ActiveDirectory, "displayName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "displayName")]
        [LdapAttribute(Schema.Rfc2307, "displayName")]
        public virtual string DisplayName { get; internal set; }

        /// <inheritdoc />
        [LdapAttribute(Schema.ActiveDirectory, "mail")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "mail")]
        [LdapAttribute(Schema.Rfc2307, "mail")]
        [Claim(ClaimTypes.Email)]
        public virtual string EmailAddress { get; internal set; }

        /// <inheritdoc />
        [LdapAttribute(Schema.ActiveDirectory, "objectSid",
            Converter = typeof(SidConverter))]
        [LdapAttribute(Schema.IdentityManagementForUnix, "uidNumber")]
        [LdapAttribute(Schema.Rfc2307, "uidNumber")]
        [Claim(ClaimTypes.PrimarySid)]
        [Claim(ClaimTypes.Sid)]
        [Claim(ClaimTypes.NameIdentifier)]
        public virtual string Identity { get; internal set; }

        /// <inheritdoc />
        [JsonIgnore]
        public virtual IEnumerable<string> RequiredAttributes {
            get {
                var retval = (from p in this.GetType().GetProperties()
                              let a = p.GetCustomAttributes<LdapAttributeAttribute>()
                              where (a != null) && a.Any()
                              select a.Select(aa => aa.Name))
                              .SelectMany(a => a);
                return retval;
            }
        }

        /// <inheritdoc />
        [LdapAttribute(Schema.ActiveDirectory, "sn")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sn")]
        [LdapAttribute(Schema.Rfc2307, "sn")]
        [Claim(ClaimTypes.Surname)]
        public virtual string Surname { get; internal set; }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public void Assign(LdapEntry entry, LdapConnection connection,
                ILdapOptions options) {
            var props = LdapAttributeAttribute.GetLdapProperties(this.GetType(),
                options?.Schema);

            foreach (var p in props.Keys) {
                try {
                    var a = props[p];
                    var v = a.GetValue(entry);
                    p.SetValue(this, v);
                } catch (KeyNotFoundException) {
                    Debug.WriteLine("LDAP Attribute not found.");
                    continue;
                }
            }

            this.AddGroupClaims(entry, connection, options);
            this.AddPropertyClaims(entry, connection, options);

        }
        #endregion

        #region Protected class methods
        /// <summary>
        /// Gets the group memberships of the specified (user or group) LDAP
        /// entry.
        /// </summary>
        /// <param name="entry">The entry to retrieve the group memberships of.
        /// </param>
        /// <param name="connection">An <see cref="LdapConnection"/> to retrieve
        /// the details about the groups.</param>
        /// <param name="options">The <see cref="ILdapOptions"/> configuring the
        /// mapping of attributes.</param>
        /// <returns>The LDAP entries for the groups <paramref name="entry"/> is
        /// member of.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="entry"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="connection"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        protected static IEnumerable<LdapEntry> GetGroupMemberships(
                LdapEntry entry,
                LdapConnection connection,
                ILdapOptions options) {
            _ = entry
                ?? throw new ArgumentNullException(nameof(entry));
            _ = connection
                ?? throw new ArgumentNullException(nameof(connection));
            _ = options
                ?? throw new ArgumentNullException(nameof(options));

            var mapping = options.Mapping;
            var groups = (string[]) null;

            try {
                groups = entry.GetAttribute(options.Mapping.GroupsAttribute)
                    ?.StringValueArray;
            } catch (KeyNotFoundException) {
                // Entry has no group memberships.
                yield break;
            }

            if (groups != null) {
                Debug.WriteLine($"Determining details of {groups.Length} "
                    + $"groups that \"{entry.Dn}\" is member of.");

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
                }
            } /* end if (groups != null) */
        }

        /// <summary>
        /// Gets the direct and transitive group memberships of the specified
        /// (user or group) LDAP entry.
        /// </summary>
        /// <param name="entry">The entry to retrieve the group memberships of.
        /// </param>
        /// <param name="connection">An <see cref="LdapConnection"/> to retrieve
        /// the details about the groups.</param>
        /// <param name="options">The <see cref="ILdapOptions"/> configuring the
        /// mapping of attributes.</param>
        /// <returns>The LDAP entries for the groups <paramref name="entry"/> is
        /// member of.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="entry"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="connection"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        private IEnumerable<LdapEntry> GetRecursiveGroupMemberships(
                LdapEntry entry,
                LdapConnection connection,
                ILdapOptions options) {
            var stack = new Stack<LdapEntry>();
            stack.Push(entry);

            while (stack.Count > 0) {
                foreach (var g in GetGroupMemberships(stack.Pop(), connection,
                        options)) {
                    stack.Push(g);
                    yield return g;
                }
            }
        }
        #endregion

        #region Protected methods

        /// <summary>
        /// Adds <see cref="Claims"/> from group memberships of
        /// <paramref name="entry"/>.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="connection"></param>
        /// <param name="schema"></param>
        protected virtual void AddGroupClaims(LdapEntry entry,
                LdapConnection connection, ILdapOptions options) {
            _ = entry
                ?? throw new ArgumentNullException(nameof(entry));
            _ = connection
                ?? throw new ArgumentNullException(nameof(connection));
            _ = options
                ?? throw new ArgumentNullException(nameof(options));

            var claims = (IList<Claim>) this.Claims;
            var mapping = options.Mapping;

            try {
                var a = entry.GetAttribute(mapping.PrimaryGroupAttribute);
                var gid = a.ToString((ILdapAttributeConverter) null);

                var endOfDomain = this.Identity.LastIndexOf('-');
                if (endOfDomain > 0) {
                    // If we have an actual SID for the user, assume an AD and
                    // convert the RID of the primary group to a SID using the
                    // domain part extracted from the user.
                    var domain = this.Identity.Substring(0, endOfDomain);
                    gid = $"{domain}-{gid}";
                }

                claims.Add(new Claim(ClaimTypes.PrimaryGroupSid, gid));
                claims.Add(new Claim(ClaimTypes.GroupSid, gid));
            } catch {
                // Ignore missing group, just set no claim then.
                Debug.WriteLine("Could not set primary group claims.");
            }

            {
                var conv = mapping.GetGroupIdentityConverter();
                var groups = options.IsRecursiveGroupMembership
                    ? GetRecursiveGroupMemberships(entry, connection, options)
                    : GetGroupMemberships(entry, connection, options);

                foreach (var g in groups) {
                    try {
                        var a = g.GetAttribute(mapping.GroupIdentityAttribute);
                        var gid = a.ToString(conv);
                        claims.Add(new Claim(ClaimTypes.GroupSid, gid));
                    } catch {
                        // Ignore issue, just set no claim.
                        Debug.WriteLine("Could not set group claims.");
                    }
                }
            }
        }

        /// <summary>
        /// Adds <see cref="Claims"/> from the properties annotated with a
        /// <see cref="ClaimAttribute"/>.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="connection"></param>
        /// <param name="schema"></param>
        protected virtual void AddPropertyClaims(LdapEntry entry,
                LdapConnection connection, ILdapOptions options) {
            _ = entry
                ?? throw new ArgumentNullException(nameof(entry));
            _ = connection
                ?? throw new ArgumentNullException(nameof(connection));
            _ = options
                ?? throw new ArgumentNullException(nameof(options));

            var claims = (IList<Claim>) this.Claims;
            var mapped = from p in this.GetType().GetProperties()
                         let a = p.GetCustomAttributes<ClaimAttribute>()
                         where (p.PropertyType == typeof(string)) && (a != null) && a.Any()
                         select new {
                             Claims = a.Select(aa => aa.Name),
                             Value = (string) p.GetValue(this)
                         };
            foreach (var m in mapped) {
                foreach (var c in m.Claims) {
                    if (m.Value != null) {
                        Debug.WriteLine($"Adding {c} claim as \"{m.Value}\".");
                        claims.Add(new Claim(c, m.Value));
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets the name of the LDAP attribute where the given property is
        /// stored.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected string GetLdapAttribute(string schema,
                [CallerMemberName] string propertyName = null) {
            var prop = this.GetType().GetProperty(propertyName);

            if (prop == null) {
                var msg = Properties.Resources.ErrorPropertyNotFound;
                msg = string.Format(msg, propertyName);
                throw new ArgumentException(msg, nameof(propertyName));
            }

            var att = (from a in prop.GetCustomAttributes<LdapAttributeAttribute>()
                       where (a.Schema == schema)
                       select a).FirstOrDefault();

            if (att == null) {
                var msg = Properties.Resources.ErrorPropertyNotMapped;
                msg = string.Format(msg, propertyName, schema);
                throw new ArgumentException(msg, nameof(propertyName));
            }

            return att.Name;
        }

        #endregion
    }
}
