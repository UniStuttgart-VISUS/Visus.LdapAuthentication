// <copyright file="LdapMapper.cs" company="Visualisierungsinstitut der Universität Stuttgart">
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
using System.Security.Claims;
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
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapMapper(LdapOptions options,
                ILogger<LdapMapper<TUser, TGroup>> logger) {
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._options = options
                ?? throw new ArgumentNullException(nameof(options));

            // Cache the property of TUser holding the claims.
            this._claimsProperty = ClaimsAttribute.GetClaims<TUser>();
            // Note: Claims are optional, so we do not check this.

            // Cache the property of TUser holding the groups.
            this._groupsProperty = LdapGroupsAttribute.GetLdapGroups<TUser>();
            // Note: Groups are optional, so we do not check this.

            // Get the property where the identity of a TGroup is stored. We need
            // this property, because the annotated attribute is required to
            // retrieve the LDAP entry of the primrary group.
            var groupIdentityProperty = LdapIdentityAttribute.GetProperty<
                TGroup>();
            if (groupIdentityProperty == null) {
                throw new ArgumentException(string.Format(
                    Resources.ErrorNoIdentity,
                    typeof(TUser).FullName));
            }

            // Get the LDAP attribute holding the identity of a group.
            this._groupIdentityAttribute = LdapAttributeAttribute.GetLdapAttribute(
                groupIdentityProperty, this._options.Schema);
            if (this._groupIdentityAttribute == null) {
                throw new ArgumentException(string.Format(
                    Resources.ErrorNoLdapAttribute,
                    groupIdentityProperty.Name));
            }

            // Get the property where the identity of a TUser is stored. This
            // property is required for implementing GetIdentity().
            this._userIdentityProperty = LdapIdentityAttribute.GetProperty<
                TUser>();
            if (this._userIdentityProperty == null) {
                throw new ArgumentException(string.Format(
                    Resources.ErrorNoIdentity,
                    typeof(TUser).FullName));
            }

            // Get the LDAP attribute which represents the identity of a user.
            this._userIdentityAttribute = LdapAttributeAttribute.GetLdapAttribute(
                this._userIdentityProperty, this._options.Schema);
            if (this._userIdentityAttribute == null) {
                throw new ArgumentException(string.Format(
                    Resources.ErrorNoLdapAttribute,
                    this._userIdentityProperty.Name));
            }

            // Check whether the group wants to know whether it is the primary
            // group of the user.
            this._primaryGroupFlagProperty = PrimaryGroupFlagAttribute
                .GetProperty<TGroup>();

            // Get property/attribute mappings for user and group objects.
            this._groupProperties = LdapAttributeAttribute.GetLdapProperties(
                typeof(TGroup), this._options.Schema);
            this._userProperties = LdapAttributeAttribute.GetLdapProperties(
                typeof(TUser), this._options.Schema);

            // Determine which attributes we must load for users/groups. Note
            // that we need some globally defined attributes for to determine
            // group membership (recursively) as well.
            var groupMembershipAttributes = new[] {
                this._options.Mapping.DistinguishedNameAttribute,
                this._options.Mapping.GroupsAttribute,
                this._options.Mapping.PrimaryGroupAttribute
            };

            this.RequiredGroupAttributes = LdapAttributeAttribute
                .GetRequiredAttributes<TGroup>(this._options.Schema)
                .Concat(groupMembershipAttributes)
                .Distinct()
                .ToArray();
            this.RequiredUserAttributes = LdapAttributeAttribute
                .GetRequiredAttributes<TUser>(this._options.Schema)
                .Concat(groupMembershipAttributes)
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The options determining where certain
        /// properties are stored on the LDAP server.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapMapper(IOptions<LdapOptions> options,
                ILogger<LdapMapper<TUser, TGroup>> logger)
            : this(options?.Value, logger) { }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public string[] RequiredGroupAttributes { get; }

        /// <inheritdoc />
        public string[] RequiredUserAttributes { get; }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public TUser Assign(SearchResultEntry entry,
                LdapConnection connection,
                TUser user) {
            _ = user
                ?? throw new ArgumentNullException(nameof(user));
            _ = entry
                ?? throw new ArgumentNullException(nameof(entry));
            _ = connection
                ?? throw new ArgumentNullException(nameof(connection));

            // Assign properties from cached mappings.
            this.Assign(entry, this._userProperties, user);

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
                var isPrimary = true;

                foreach (var e in entries) {
                    var g = this.Assign(e, this._groupProperties, new TGroup());

                    if (this._primaryGroupFlagProperty != null) {
                        this._primaryGroupFlagProperty.SetValue(g, isPrimary);
                        isPrimary = false;
                    }

                    groups.Add(g);
                }

                this._groupsProperty.SetValue(user, groups);
            }

            return user;
        }

        /// <inheritdoc />
        public TGroup Assign(SearchResultEntry entry,
                LdapConnection connection,
                TGroup group) {
            _ = group
                ?? throw new ArgumentNullException(nameof(group));
            _ = entry
                ?? throw new ArgumentNullException(nameof(entry));
            _ = connection
                ?? throw new ArgumentNullException(nameof(connection));
            return this.Assign(entry, this._groupProperties, group);
        }

        /// <inheritdoc />
        public TUser Assign(IEnumerable<Claim> claims, TUser user) {
            _ = user
                ?? throw new ArgumentNullException(nameof(user));
            if ((this._claimsProperty != null) && (claims!= null)) {
                // Note: The claims builder may return an enumerator that
                // creates the claims dynamically. We want to force the
                // evaluation at this point to prevent potential access
                // to disposed sources later on.
                this._claimsProperty.SetValue(user, claims.ToList());
            }

            return user;
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
        /// <param name="from"></param>
        /// <param name="properties"></param>
        /// <param name="to"></param>
        /// <returns><paramref name="to"/>.</returns>
        private TObject Assign<TObject>(SearchResultEntry from,
                IDictionary<PropertyInfo, LdapAttributeAttribute> properties,
                TObject to) {
            foreach (var p in properties) {
                try {
                    var a = p.Value;
                    var v = a.GetValue(from);
                    p.Key.SetValue(to, v);
                } catch (KeyNotFoundException ex) {
                    this._logger.LogError(ex,
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
                            this.RequiredGroupAttributes);
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
            var gid = this.GetPrimaryGroup(entry);

            foreach (var b in this._options.SearchBases) {
                var req = new SearchRequest(b.Key,
                    $"({this._groupIdentityAttribute.Name}={gid})",
                    SearchScope.Subtree,
                    this.RequiredGroupAttributes);
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
        private readonly IDictionary<PropertyInfo, LdapAttributeAttribute>
            _groupProperties;
        private readonly LdapAttributeAttribute _groupIdentityAttribute;
        private readonly ILogger _logger;
        private readonly PropertyInfo _groupsProperty;
        private readonly LdapOptions _options;
        private readonly PropertyInfo _primaryGroupFlagProperty;
        private readonly LdapAttributeAttribute _userIdentityAttribute;
        private readonly PropertyInfo _userIdentityProperty;
        private readonly IDictionary<PropertyInfo, LdapAttributeAttribute>
            _userProperties;
        #endregion
    }
}
