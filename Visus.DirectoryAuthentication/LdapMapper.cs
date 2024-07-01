// <copyright file="LdapUserMapper.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Reflection;
using Visus.DirectoryAuthentication.Properties;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Maps LDAP entries to user objects of type <typeparamref name="TUser"/>
    /// to the attributes retrieved from a <see cref="SearchResultEntry"/>.
    /// </summary>
    /// <remarks>
    /// <para>While the user object can be basically any class, this default
    /// implementation is for <see cref="ILdapUser"/>-derived classes where
    /// the implementation can find out about the LDAP attributes by means
    /// of <see cref="LdapAttributeAttribute"/>s.</para>
    /// </remarks>
    /// <typeparam name="TUser">The user object.</typeparam>
    public sealed class LdapMapper<TUser, TGroup> : ILdapMapper<TUser, TGroup>
            where TGroup : new() {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The options determining where certain
        /// properties are stored on the LDAP server.</param>
        /// <param name="claimsBuilder">A helper that can create
        /// <see cref="System.Security.Claims.Claim"/>s from a user object. This
        /// parameter may be <c>null</c>, in which case the mapper will not
        /// assign any claims to the user object. If
        /// <typeparamref name="TUser" /> is not annotated with
        /// <see cref="ClaimsAttribute"/>, this parameter is irrelevant.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapMapper(LdapOptions options,
                IClaimsBuilder<TUser, TGroup> claimsBuilder = null) {
            this._claimsBuilder = claimsBuilder;
            this._options = options
                ?? throw new ArgumentNullException(nameof(options));

            // Cache reflection access to properties we want to read or write.
            this._claimsProperty = ClaimsAttribute.GetClaims<TUser>();
            // Note: Claims are optional, so we do not check this.

            this._groupsProperty = LdapGroupsAttribute.GetLdapGroups<TUser>();
            // Note: Groups are optional, so we do not check this.

            var groupIdentityProperty = LdapIdentityAttribute.GetLdapIdentity<
                TGroup>();
            if (groupIdentityProperty == null) {
                throw new ArgumentException(string.Format(
                    Resources.ErrorNoIdentity,
                    typeof(TUser).FullName));
            }

            this._groupIdentityAttribute = LdapAttributeAttribute.GetLdapAttribute(
                groupIdentityProperty, this._options.Schema);
            if (this._groupIdentityAttribute == null) {
                throw new ArgumentException(string.Format(
                    Resources.ErrorNoLdapAttribute,
                    groupIdentityProperty.Name));
            }

            this._userIdentityProperty = LdapIdentityAttribute.GetLdapIdentity<
                TUser>();
            if (this._userIdentityProperty == null) {
                throw new ArgumentException(string.Format(
                    Resources.ErrorNoIdentity,
                    typeof(TUser).FullName));
            }

            this._userIdentityAttribute = LdapAttributeAttribute.GetLdapAttribute(
                this._userIdentityProperty, this._options.Schema);
            if (this._userIdentityAttribute == null) {
                throw new ArgumentException(string.Format(
                    Resources.ErrorNoLdapAttribute,
                    this._userIdentityProperty.Name));
            }

            // Get property/attribute mappings for user and group objects.
            this._groupProperties = LdapAttributeAttribute.GetLdapProperties(
                typeof(TGroup), this._options.Schema);
            this._userProperties = LdapAttributeAttribute.GetLdapProperties(
                typeof(TUser), this._options.Schema);

            // Determine which attributes we must load for users/groups.
            this._requiredGroupAttributes = LdapAttributeAttribute
                .GetRequiredAttributes<TGroup>(this._options.Schema).ToArray();
            this._requiredUserAttributes = LdapAttributeAttribute
                .GetRequiredAttributes<TUser>(this._options.Schema).ToArray();

        }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The options determining where certain
        /// properties are stored on the LDAP server.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapMapper(IOptions<LdapOptions> options,
                IClaimsBuilder<TUser, TGroup> claimsBuilder = null)
            : this(options?.Value, claimsBuilder) { }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public IEnumerable<string> RequiredGroupAttributes
            => this._requiredGroupAttributes;

        /// <inheritdoc />
        public IEnumerable<string> RequiredUserAttributes
            => this._requiredUserAttributes;
        #endregion

        #region Public methods
        /// <inheritdoc />
        public void Assign(TUser user,
                SearchResultEntry entry,
                LdapConnection connection,
                ILogger logger) {
            _ = entry
                ?? throw new ArgumentNullException(nameof(entry));
            _ = connection
                ?? throw new ArgumentNullException(nameof(connection));

            // Assign properties from cached mappings.
            Assign(user, entry, this._userProperties, logger);

            // If requested, determine group memberships.
            if (this._groupsProperty != null) {
                var entries = new List<SearchResultEntry> {
                    this.GetPrimaryGroup(entry, connection)
                };

                entries.AddRange(this.GetGroups(entry, connection));

                if (this._options.IsRecursiveGroupMembership) {
                    entries.AddRange(this.GetRecursiveGroups(
                        entries, connection));
                }

                var groups = new List<TGroup>();
                groups.Capacity = entries.Count;

                foreach (var e in entries) {
                    groups.Add(Assign(new TGroup(), e,
                        this._groupProperties, logger));
                }

                this._groupsProperty.SetValue(user, groups);
            }

            // If the user object expects to have claims set, we do so.
            if ((this._claimsBuilder != null)
                    && (this._claimsProperty != null)) {
                var claims = this._claimsBuilder.UseMapper(this).Build(user);
                this._claimsProperty.SetValue(user, claims.ToList());
            }
        }

        /// <inheritdoc />
        public IEnumerable<TGroup> GetGroups(TUser user) {
            _ = user ?? throw new ArgumentNullException(nameof(user));
            if (this._groupsProperty?.GetValue(user)
                    is IEnumerable<TGroup> retval) {
                return retval;
            } else {
                return Enumerable.Empty<TGroup>();
            }
        }

        /// <inheritdoc />
        public string GetIdentity(TUser user)
            => this._userIdentityProperty.GetValue(user) as string;

        /// <inheritdoc />
        public string GetIdentity(SearchResultEntry entry)
            => this._userIdentityAttribute.GetValue(entry) as string;
        #endregion

        #region Private methods
        /// <summary>
        /// Assign the specified <paramref name="properties"/> of
        /// <paramref name="to"/> with attribute values form
        /// <paramref name="from"/>.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="properties"></param>
        /// <param name="logger"></param>
        private static TObject Assign<TObject>(TObject to,
                SearchResultEntry from,
                IDictionary<PropertyInfo, LdapAttributeAttribute> properties,
                ILogger logger) {
            foreach (var p in properties) {
                try {
                    var a = p.Value;
                    var v = a.GetValue(from);
                    p.Key.SetValue(to, v);
                } catch (KeyNotFoundException ex) {
                    logger?.LogError(ex,
                        Resources.ErrorAttributeNotFound,
                        p.Value);
                    continue;
                }
            }

            return to;
        }

        /// <summary>
        /// Gets the group memberships of the specified (user or group) LDAP
        /// entry.
        /// </summary>
        /// <remarks>
        /// Note that this method only retrieves direct group memberships stored
        /// in <see cref="LdapMapping.GroupsAttribute"/>, which does not include
        /// the groups any of these groups are member of nor the primary group,
        /// which is stored in a special attribute of the entry itself.
        /// </remarks>
        /// <param name="entry">The entry to retrieve the group memberships of.
        /// </param>
        /// <param name="connection">An <see cref="LdapConnection"/> to retrieve
        /// the details about the groups.</param>
        /// <returns>The LDAP entries for the groups <paramref name="entry"/> is
        /// member of.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="entry"/>
        /// is <c>null</c>, or if <paramref name="connection"/> is <c>null</c>,
        /// or if <paramref name="options"/> is <c>null</c>.</exception>
        private IEnumerable<SearchResultEntry> GetGroups(
                SearchResultEntry entry,
                LdapConnection connection) {
            Debug.Assert(entry != null);
            Debug.Assert(connection != null);
            var mapping = this._options.Mapping;
            var groups = (string[]) null;

            try {
                var att = entry.GetAttribute(mapping.GroupsAttribute);
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
                    + $"groups that \"{entry.DistinguishedName}\" is member "
                    + "of.");

                foreach (var g in groups) {
                    var q = g.EscapeLdapFilterExpression();

                    foreach (var b in this._options.SearchBases) {
                        var req = new SearchRequest(b.Key,
                            $"({mapping.DistinguishedNameAttribute}={q})",
                            SearchScope.Subtree,
                            this._requiredGroupAttributes);
                        var res = connection.SendRequest(req, this._options);

                        if (res is SearchResponse r) {
                            foreach (SearchResultEntry e in r.Entries) {
                                yield return e;
                            }
                        }
                    }
                } /* foreach (var g in groups) */
            } /* if (groups != null) */
        }

        /// <summary>
        /// Gets the identity of the primary group of <paramref name="entry"/>.
        /// </summary>
        /// <remarks>
        /// This method fails if <paramref name="entry"/> does not have a
        /// <see cref="LdapMapping.PrimaryGroupAttribute"/>, which is the case
        /// if <paramref name="entry"/> is a group.
        /// </remarks>
        /// <param name="entry">The entry to retrieve the primary group of.
        /// </param>
        private string GetPrimaryGroup(SearchResultEntry entry) {
            Debug.Assert(entry != null);
            var identity = this.GetIdentity(entry);
            var mapping = this._options.Mapping;
            var att = entry.GetAttribute(mapping.PrimaryGroupAttribute);
            var retval = att.GetValue((ILdapAttributeConverter) null) as string;

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
        /// Gets the primary group of <paramref name="entry"/>.
        /// </summary>
        /// <remarks>
        /// This method fails if <paramref name="entry"/> does not have a
        /// <see cref="LdapMapping.PrimaryGroupAttribute"/>, which is the case
        /// if <paramref name="entry"/> is a group.
        /// </remarks>
        /// <param name="entry">The entry to retrieve the primary group of.
        /// </param>
        /// <param name="connection">An <see cref="LdapConnection"/> to retrieve
        /// the details about the primary group, which is stored as an
        /// identifier in <paramref name="entry"/>.</param>
        /// <returns>The LDAP entry of the primary group.</returns>
        /// <exception cref="KeyNotFoundException">If <paramref name="entry"/>
        /// has a primary group, but its LDAP entry was not found in the
        /// configured search path.</exception>
        private SearchResultEntry GetPrimaryGroup(SearchResultEntry entry,
                LdapConnection connection) {
            Debug.Assert(entry != null);
            Debug.Assert(connection != null);
            var mapping = this._options.Mapping;
            var gid = this.GetPrimaryGroup(entry);

            foreach (var b in this._options.SearchBases) {
                var req = new SearchRequest(b.Key,
                    $"({this._groupIdentityAttribute.Name}={gid})",
                    SearchScope.Subtree,
                    mapping.RequiredGroupAttributes);
                var res = connection.SendRequest(req, this._options);

                if (res is SearchResponse r) {
                    var entries = r.Entries.Cast<SearchResultEntry>();
                    if (entries.Any()) {
                        return entries.First();
                    }
                }
            }

            throw new KeyNotFoundException(
                string.Format(Resources.ErrorGroupNotFound, gid));
        }

        /// <summary>
        /// Gets the direct and transitive group memberships of the specified
        /// (user or group) LDAP entries.
        /// </summary>
        /// <param name="entries">The entries to retrieve the group memberships
        /// of.</param>
        /// <param name="connection">An <see cref="LdapConnection"/> to retrieve
        /// the details about the groups.</param>
        /// <returns>The LDAP entries for the groups <paramref name="entries"/>
        /// are member of.</returns>
        private IEnumerable<SearchResultEntry> GetRecursiveGroups(
                IEnumerable<SearchResultEntry> entries,
                LdapConnection connection) {
            Debug.Assert(entries != null);
            Debug.Assert(connection != null);
            var stack = new Stack<SearchResultEntry>();

            foreach (var e in entries) {
                stack.Push(e);
            }

            while (stack.Count > 0) {
                foreach (var g in this.GetGroups(stack.Pop(), connection)) {
                    stack.Push(g);
                    yield return g;
                }
            }
        }
        #endregion

        #region Private fields
        private readonly PropertyInfo _claimsProperty;
        private readonly IClaimsBuilder<TUser, TGroup> _claimsBuilder;
        private readonly IDictionary<PropertyInfo, LdapAttributeAttribute>
            _groupProperties;
        private readonly LdapAttributeAttribute _groupIdentityAttribute;
        private readonly PropertyInfo _groupsProperty;
        private readonly LdapOptions _options;
        private readonly string[] _requiredGroupAttributes;
        private readonly string[] _requiredUserAttributes;
        private readonly LdapAttributeAttribute _userIdentityAttribute;
        private readonly PropertyInfo _userIdentityProperty;
        private readonly IDictionary<PropertyInfo, LdapAttributeAttribute>
            _userProperties;
        #endregion
    }
}
