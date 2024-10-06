// <copyright file="LdapSearchServiceBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Visus.Ldap;
using Visus.Ldap.Configuration;
using Visus.Ldap.Extensions;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Services {

    /// <summary>
    /// The base implementation of
    /// <see cref="ILdapSearchServiceBase{TUser, TGroup, TSearchScope}"/>, which
    /// maps the majority of the interface to a few methods performing the
    /// actual search.
    /// </summary>
    /// <typeparam name="TUser">The type of user that is to be retrieved from
    /// the directory.</typeparam>
    /// <typeparam name="TGroup">The type used to represent group memberships
    /// of <typeparamref name="TUser"/>.</typeparam>
    /// <typeparam name="TSearchScope">The type used to represent the search
    /// scope in the underlying library.</typeparam>
    /// <typeparam name="TOptions">The actual type of the
    /// <see cref="LdapOptionsBase"/> being used.</typeparam>
    public abstract class LdapSearchServiceBase<TUser, TGroup,
            TSearchScope, TOptions>
        : ILdapSearchServiceBase<TUser, TGroup, TSearchScope>
            where TUser : class, new()
            where TGroup : class, new()
            where TSearchScope : struct, Enum
            where TOptions : LdapOptionsBase {

        #region Public methods
        /// <inheritdoc />
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public TGroup? GetGroupByDistinguishedName(string distinguishedName) {
            var att = this.GroupMap.DistinguishedNameAttribute!.Name;
            var filter = $"({att}={distinguishedName})";
            return this.GetGroupEntry($"({att}={filter})", null);
        }

        /// <inheritdoc />
        public Task<TGroup?> GetGroupByDistinguishedNameAsync(
                string distinguishedName) {
            var att = this.GroupMap.DistinguishedNameAttribute!.Name;
            var filter = $"({att}={distinguishedName})";
            return this.GetGroupEntryAsync($"({att}={filter})", null);
        }

        /// <inheritdoc />
        public TGroup? GetGroupByIdentity(string identity) {
            var att = this.GroupMap.IdentityAttribute!.Name;
            var filter = identity.EscapeLdapFilterExpression();
            return this.GetGroupEntry($"({att}={filter})", null);
        }

        /// <inheritdoc />
        public Task<TGroup?> GetGroupByIdentityAsync(string identity) {
            var att = this.GroupMap.IdentityAttribute!.Name;
            var filter = identity.EscapeLdapFilterExpression();
            return this.GetGroupEntryAsync($"({att}={filter})", null);
        }

        /// <inheritdoc />
        public TGroup? GetGroupByName(string name) {
            var att = this.GroupMap.AccountNameAttribute?.Name;
            var filter = name.EscapeLdapFilterExpression();
            return this.GetGroupEntry($"({att}={filter})", null);
        }

        /// <inheritdoc />
        public Task<TGroup?> GetGroupByNameAsync(string name) {
            var att = this.GroupMap.AccountNameAttribute?.Name;
            var filter = name.EscapeLdapFilterExpression();
            return this.GetGroupEntryAsync($"({att}={filter})", null);
        }

        /// <inheritdoc />
        public TUser? GetUserByAccountName(string accountName) {
            var att = this.UserMap.AccountNameAttribute?.Name;
            var filter = accountName.EscapeLdapFilterExpression();
            return this.GetUserEntry($"({att}={filter})", null);
        }

        /// <inheritdoc />
        public abstract IEnumerable<TUser> GetGroupMembers(TGroup group);

        /// <inheritdoc />
        public virtual Task<IEnumerable<TUser>> GetGroupMembersAsync(
                TGroup group,
                CancellationToken cancellationToken)
            => Task.FromResult(this.GetGroupMembers(group));

        /// <inheritdoc />
        public IEnumerable<TGroup> GetGroups()
            => this.GetGroupEntries(this.Mapping!.GroupsFilter, null, default);

        /// <inheritdoc />
        public Task<IEnumerable<TGroup>> GetGroupsAsync(
                CancellationToken cancellationToken)
            => this.GetGroupEntriesAsync(this.Mapping!.GroupsFilter, null,
                cancellationToken);

        /// <inheritdoc />
        public IEnumerable<TGroup> GetGroups(string filter)
            => this.GetGroupEntries(this.MergeGroupFilter(filter),
                null, default);

        /// <inheritdoc />
        public Task<IEnumerable<TGroup>> GetGroupsAsync(string filter,
                CancellationToken cancellationToken)
            => this.GetGroupEntriesAsync(this.MergeGroupFilter(filter),
                null, cancellationToken);

        /// <inheritdoc />
        public IEnumerable<TGroup> GetGroups(
                IDictionary<string, TSearchScope> searchBases,
                string filter)
            => this.GetGroupEntries(this.MergeGroupFilter(filter),
                searchBases, default);

        /// <inheritdoc />
        public Task<IEnumerable<TGroup>> GetGroupsAsync(
                IDictionary<string, TSearchScope> searchBases,
                string filter,
                CancellationToken cancellationToken)
            => this.GetGroupEntriesAsync(this.MergeGroupFilter(filter),
                searchBases, cancellationToken);

        /// <inheritdoc />
        public Task<TUser?> GetUserByAccountNameAsync(string accountName) {
            var att = this.UserMap.AccountNameAttribute?.Name;
            var filter = accountName.EscapeLdapFilterExpression();
            return this.GetUserEntryAsync($"({att}={filter})", null);
        }

        /// <inheritdoc />
        public TUser? GetUserByDistinguishedName(string distinguishedName) {
            var att = this.UserMap.DistinguishedNameAttribute!.Name;
            var filter = $"({att}={distinguishedName})";
            return this.GetUserEntry($"({att}={filter})", null);
        }

        /// <inheritdoc />
        public Task<TUser?> GetUserByDistinguishedNameAsync(
                string distinguishedName) {
            var att = this.UserMap.DistinguishedNameAttribute!.Name;
            var filter = distinguishedName.EscapeLdapFilterExpression();
            return this.GetUserEntryAsync($"({att}={filter})", null);
        }

        /// <inheritdoc />
        public TUser? GetUserByIdentity(string identity) {
            var att = this.UserMap.IdentityAttribute!.Name;
            var filter = identity.EscapeLdapFilterExpression();
            return this.GetUserEntry($"({att}={filter})", null);
        }

        /// <inheritdoc />
        public Task<TUser?> GetUserByIdentityAsync(string identity) {
            var att = this.UserMap.IdentityAttribute!.Name;
            var filter = identity.EscapeLdapFilterExpression();
            return this.GetUserEntryAsync($"({att}={filter})", null);
        }

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers()
            => this.GetUserEntries(this.Mapping!.UsersFilter, null, default);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync(
                CancellationToken cancellationToken)
            => this.GetUserEntriesAsync(this.Mapping!.UsersFilter, null,
                cancellationToken);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(string filter)
            => this.GetUserEntries(this.MergeUserFilter(filter),
                null, default);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync(string filter,
                CancellationToken cancellationToken)
            => this.GetUserEntriesAsync(this.MergeUserFilter(filter),
                null, cancellationToken);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(
                IDictionary<string, TSearchScope> searchBases,
                string filter)
            => this.GetUserEntries(this.MergeUserFilter(filter),
                searchBases, default);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync(
                IDictionary<string, TSearchScope> searchBases,
                string filter,
                CancellationToken cancellationToken)
            => this.GetUserEntriesAsync(this.MergeUserFilter(filter),
                searchBases, cancellationToken);
        #endregion

        #region Protected constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP options specifying mapping options to
        /// use.</param>
        /// <param name="userMap">An LDAP property map for
        /// <typeparamref name="TUser"/> that allows the service to retrieve
        /// infromation about the user object.</param>
        /// <param name="groupMap">An LDAP property map for
        /// <typeparamref name="TGroup"/> that allows the service to retrieve
        /// infromation about the group object.</param>
        /// <exception cref="ArgumentNullException">If any of the parameters is
        /// <c>null</c>.</exception>
        protected LdapSearchServiceBase(IOptions<TOptions> options,
                ILdapAttributeMap<TUser> userMap,
                ILdapAttributeMap<TGroup> groupMap) {
            this.GroupMap = groupMap
                ?? throw new ArgumentNullException(nameof(groupMap));
            this.Options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
            this.UserMap = userMap
                ?? throw new ArgumentNullException(nameof(userMap));

            this.GroupAttributes = this.GroupMap.IsGroupMember
                ? this.GroupMap.AttributeNames
                    .Append(this.Mapping.GroupsAttribute)
                    .ToArray()
                : this.GroupMap.AttributeNames.ToArray();
            this.UserAttributes = this.UserMap.IsGroupMember
                ? this.UserMap.AttributeNames
                    .Append(this.Mapping.PrimaryGroupAttribute)
                    .Append(this.Mapping.GroupsAttribute)
                    .ToArray()
                : this.UserMap.AttributeNames.ToArray();
            this.GroupMemberAttributes = this.UserAttributes
                .Append(this.Mapping.GroupMemberAttribute)
                .ToArray();
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// Gets the attributes that must be loaded for a group entry to
        /// populate all properties of <typeparamref name="TGroup"/>.
        /// </summary>
        protected string[] GroupAttributes { get; }

        /// <summary>
        /// Gets the mapping between LDAP attributes and the properties of
        /// <typeparamref name="TGroup"/>.
        /// </summary>
        protected ILdapAttributeMap<TGroup> GroupMap { get; }

        /// <summary>
        /// Gets the attributes that must be loaded to recursively find all
        /// members of a group.
        /// </summary>
        protected string[] GroupMemberAttributes { get; }

        /// <summary>
        /// Gets the active LDAP mapping from <see cref="Options"/>.
        /// </summary>
        protected LdapMapping Mapping => this.Options.Mapping!;

        /// <summary>
        /// Gets the LDAP server configuration.
        /// </summary>
        protected TOptions Options { get; }

        /// <summary>
        /// Gets the attributes that must be loaded for a user entry to
        /// populate all properties of <typeparamref name="TUser"/>.
        /// </summary>
        protected string[] UserAttributes { get; }

        /// <summary>
        /// Gets the mapping between LDAP attributes and the properties of
        /// <typeparamref name="TUser"/>.
        /// </summary>
        protected ILdapAttributeMap<TUser> UserMap { get; }
        #endregion

        #region Protected methods
        /// <summary>
        /// Disposes managed resources if <paramref name="isDisposing"/> is
        /// <c>true</c>.
        /// </summary>
        /// <param name="isDisposing">If <c>true</c>, the method should dispose
        /// managed and unmanaged resources; if <c>false</c>, it should only
        /// dispose unmanaged ones.</param>
        protected abstract void Dispose(bool isDisposing);

        /// <summary>
        /// Retrieves the groups matching the given filter.
        /// </summary>
        /// <param name="filter">A filter the LDAP entries must match.</param>
        /// <param name="searchBases">The search bases to look in. If this
        /// parameter is <c>null</c>, the method looks in the search bases
        /// configured in the <see cref="Options"/>.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// a paged search.</param>
        /// <returns>The entries found at the specified locations in the
        /// directory.</returns>
        protected abstract IEnumerable<TGroup> GetGroupEntries(string filter,
            IDictionary<string, TSearchScope>? searchBases,
            CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the groups matching the given filter.
        /// </summary>
        /// <param name="filter">A filter the LDAP entries must match.</param>
        /// <param name="searchBases">The search bases to look in. If this
        /// parameter is <c>null</c>, the method looks in the search bases
        /// configured in the <see cref="Options"/>.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns>The entries found at the specified locations in the
        /// directory.</returns>
        protected virtual Task<IEnumerable<TGroup>> GetGroupEntriesAsync(
                string filter,
                IDictionary<string, TSearchScope>? searchBases,
                CancellationToken cancellationToken)
            => Task.Factory.StartNew(() => this.GetGroupEntries(filter,
                searchBases, cancellationToken));

        /// <summary>
        /// Gets a single group matching the given <paramref name="filter"/>
        /// expression.
        /// </summary>
        /// <remarks>
        /// Implementors should make use of the <see cref="ObjectCache"/> before
        /// performing this lookup.
        /// </remarks>
        /// <param name="filter">The filter uniquely identifying the group we
        /// are looking for.</param>
        /// <param name="searchBases">The search bases to look in. If this
        /// parameter is <c>null</c>, the method looks in the search bases
        /// configured in the <see cref="Options"/>.</param>
        /// <returns>The mapped group or <c>null</c> if the filter did not match
        /// a unique entry.</returns>
        protected abstract TGroup? GetGroupEntry(string filter,
            IDictionary<string, TSearchScope>? searchBases);

        /// <summary>
        /// Gets a single group matching the given <paramref name="filter"/>
        /// expression.
        /// </summary>
        /// <remarks>
        /// Implementors should make use of the <see cref="ObjectCache"/> before
        /// performing this lookup.
        /// </remarks>
        /// <param name="filter">The filter uniquely identifying the group we
        /// are looking for.</param>
        /// <param name="searchBases">The search bases to look in. If this
        /// parameter is <c>null</c>, the method looks in the search bases
        /// configured in the <see cref="Options"/>.</param>
        /// <returns>The mapped group or <c>null</c> if the filter did not match
        /// a unique entry.</returns>
        protected abstract Task<TGroup?> GetGroupEntryAsync(string filter,
            IDictionary<string, TSearchScope>? searchBases);

        /// <summary>
        /// Retrieves the users matching the given filter.
        /// </summary>
        /// <param name="filter">A filter the LDAP entries must match.</param>
        /// <param name="searchBases">The search bases to look in. If this
        /// parameter is <c>null</c>, the method looks in the search bases
        /// configured in the <see cref="Options"/>.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// a paged search.</param>
        /// <returns>The entries found at the specified locations in the
        /// directory.</returns>
        protected abstract IEnumerable<TUser> GetUserEntries(string filter,
            IDictionary<string, TSearchScope>? searchBases,
            CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the users matching the given filter.
        /// </summary>
        /// <param name="filter">A filter the LDAP entries must match.</param>
        /// <param name="searchBases">The search bases to look in. If this
        /// parameter is <c>null</c>, the method looks in the search bases
        /// configured in the <see cref="Options"/>.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns>The entries found at the specified locations in the
        /// directory.</returns>
        protected virtual Task<IEnumerable<TUser>> GetUserEntriesAsync(
                string filter,
                IDictionary<string, TSearchScope>? searchBases,
                CancellationToken cancellationToken)
            => Task.Factory.StartNew(() => this.GetUserEntries(filter,
                searchBases, cancellationToken));

        /// <summary>
        /// Gets a single user matching the given <paramref name="filter"/>
        /// expression.
        /// </summary>
        /// <param name="filter">The filter uniquely identifying the user we
        /// are looking for.</param>
        /// <param name="searchBases">The search bases to look in. If this
        /// parameter is <c>null</c>, the method looks in the search bases
        /// configured in the <see cref="Options"/>.</param>
        /// <returns>The mapped user or <c>null</c> if the filter did not match
        /// a unique entry.</returns>
        protected abstract TUser? GetUserEntry(string filter,
            IDictionary<string, TSearchScope>? searchBases);

        /// <summary>
        /// Gets a single group matching the given <paramref name="filter"/>
        /// expression.
        /// </summary>
        /// <param name="filter">The filter uniquely identifying the group we
        /// are looking for.</param>
        /// <param name="searchBases">The search bases to look in. If this
        /// parameter is <c>null</c>, the method looks in the search bases
        /// configured in the <see cref="Options"/>.</param>
        /// <returns>The mapped user or <c>null</c> if the filter did not match
        /// a unique entry.</returns>
        protected abstract Task<TUser?> GetUserEntryAsync(string filter,
            IDictionary<string, TSearchScope>? searchBases);

        /// <summary>
        /// Merges the given filter with the default group filter in
        /// <see cref="Options"/>.
        /// </summary>
        /// <param name="filter">The user-provided filter, which may be
        /// <c>null</c>.</param>
        /// <returns>The actual filter to be used in a query.</returns>
        protected string MergeGroupFilter(string filter) {
            if (string.IsNullOrWhiteSpace(filter)) {
                return this.Mapping!.GroupsFilter;
            } else {
                return $"(&{this.Mapping!.GroupsFilter}{filter})";
            }
        }

        /// <summary>
        /// Merges the given filter with the default user filter in
        /// <see cref="Options"/>.
        /// </summary>
        /// <param name="filter">The user-provided filter, which may be
        /// <c>null</c>.</param>
        /// <returns>The actual filter to be used in a query.</returns>
        protected string MergeUserFilter(string filter) {
            if (string.IsNullOrWhiteSpace(filter)) {
                return this.Mapping!.UsersFilter;
            } else {
                return $"(&{this.Mapping!.UsersFilter}{filter})";
            }
        }
        #endregion

    }
}
