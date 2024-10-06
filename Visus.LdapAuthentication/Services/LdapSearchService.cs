// <copyright file="LdapSearchService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication.Services;
using Visus.Ldap;
using Visus.Ldap.Extensions;
using Visus.Ldap.Mapping;
using Visus.LdapAuthentication.Configuration;
using Visus.LdapAuthentication.Extensions;


namespace Visus.LdapAuthentication.Services {

    /// <summary>
    /// Implementation of <see cref="ILdapSearchService"/> using the search
    /// attributes defined by the active mapping of
    /// <typeparamref name="TUser"/> and <typeparamref name="TGroup"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of user to be created for the search
    /// results, which also defines attributes like the unique identity in
    /// combination with the global options from <see cref="LdapOptions"/>.
    /// </typeparam>
    /// <typeparam name="TGroup">The type used to represent an LDAP group.
    /// </typeparam>
    public sealed class LdapSearchService<TUser, TGroup>
            : LdapSearchServiceBase<TUser, TGroup, SearchScope, LdapOptions>,
            ILdapSearchService<TUser, TGroup>
            where TUser : class, new()
            where TGroup : class, new() {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP options specifying the server and
        /// the credentials to use.</param>
        /// <param name="connectionService">The connection service providing the
        /// LDAP connections along with the options.</param>
        /// <param name="mapper">An LDAP mapper that can fill
        /// <typeparamref name="TUser"/> and <see cref="TGroup"/> from an
        /// LDAP entry.</param>
        /// <param name="userMap">An LDAP property map for
        /// <typeparamref name="TUser"/> that allows the service to retrieve
        /// infromation about the user object.</param>
        /// <param name="groupMap">An LDAP property map for
        /// <typeparamref name="TGroup"/> that allows the service to retrieve
        /// infromation about the group object.</param>
        /// <param name="cache">A cache for raw LDAP entries.</param>
        /// <param name="logger">A logger for persisting important messages like
        /// failed search requests.</param>
        /// <exception cref="ArgumentNullException">If any of the parameters is
        /// <c>null</c>.</exception>
        public LdapSearchService(IOptions<LdapOptions> options,
                ILdapConnectionService connectionService,
                ILdapMapper<LdapEntry, TUser, TGroup> mapper,
                ILdapAttributeMap<TUser> userMap,
                ILdapAttributeMap<TGroup> groupMap,
                ILdapCache<LdapEntry> cache,
                ILogger<LdapSearchService<TUser, TGroup>> logger)
                : base(options, userMap, groupMap) {
            ArgumentNullException.ThrowIfNull(connectionService);

            this._cache = cache
                ?? throw new ArgumentNullException(nameof(cache));
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));

            this._connection = connectionService.Connect(
                this.Options.User, this.Options.Password);
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public override IEnumerable<TUser> GetGroupMembers(TGroup group) {
            ArgumentNullException.ThrowIfNull(group);
            Debug.Assert(this.Options != null);
            Debug.Assert(this.Mapping != null);

            var stack = new Stack<string>();
            stack.Push(this._mapper.GetDistinguishedName(group)
                .ToLdapFilter(this.Mapping.DistinguishedNameAttribute));

            while (stack.TryPop(out var filter)) {
                var entry = this.Connection.Search(filter,
                    this.GroupMemberAttributes,
                    this.Options)
                    .SingleOrDefault();
                if (entry == null) {
                    continue;
                }

                var members = entry.TryGetAttribute(
                    this.Mapping.GroupMemberAttribute);
                if (members == null) {
                    continue;
                }

                var memberFilter = members.GetValues<string>()
                    .ToLdapFilter(this.Mapping.DistinguishedNameAttribute);

                foreach (var u in this.GetUsers(memberFilter)) {
                    yield return u;
                }

                if (this.Options.IsRecursiveGroupMembership) {
                    var groupFilter = this.MergeGroupFilter(memberFilter);
#if WITH_LDAP_CACHE
                    var groups = this._cache.GetOrAdd(groupFilter,
                        this.GroupMemberAttributes,
                        () => this.Connection.Search(groupFilter,
                            this.GroupMemberAttributes,
                            this.Options));
#else // WITH_LDAP_CACHE
                    var groups = this.Connection.Search(groupFilter,
                        this.GroupMemberAttributes,
                        this.Options);
#endif // WITH_LDAP_CACHE

                    foreach (var g in groups) {
                        stack.Push(g.GetFilter(this.Options));
                    }
                }
            }
        }
        #endregion

        #region Protected methods
        /// <inheritdoc />
        protected override void Dispose(bool isDisposing) {
            if (isDisposing) {
                this._connection?.Dispose();
            }

            this._connection = null;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IEnumerable<TGroup> GetGroupEntries(
                string filter,
                IDictionary<string, SearchScope>? searchBases,
                CancellationToken cancellationToken)
            => this.GetEntries<TGroup>(filter,
                searchBases,
                this.GroupAttributes,
                this.GroupMap.IdentityAttribute!.Name,
                this.MapGroup,
                cancellationToken);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TGroup? GetGroupEntry(
                string filter,
                IDictionary<string, SearchScope>? searchBases)
            => this.GetEntry<TGroup>(filter,
                searchBases,
                this.GroupAttributes,
                this.MapGroup);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Task<TGroup?> GetGroupEntryAsync(
                string filter,
                IDictionary<string, SearchScope>? searchBases)
            => this.GetEntryAsync<TGroup>(filter,
                searchBases,
                this.GroupAttributes,
                this.MapGroup);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override IEnumerable<TUser> GetUserEntries(
                string filter,
                IDictionary<string, SearchScope>? searchBases,
                CancellationToken cancellationToken)
            => this.GetEntries<TUser>(filter,
                searchBases,
                this.UserAttributes,
                this.UserMap.IdentityAttribute!.Name,
                this.MapUser,
                cancellationToken);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TUser? GetUserEntry(string filter,
                IDictionary<string, SearchScope>? searchBases)
            => this.GetEntry<TUser>(filter,
                searchBases,
                this.UserAttributes,
                this.MapUser);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Task<TUser?> GetUserEntryAsync(string filter,
                IDictionary<string, SearchScope>? searchBases)
            => this.GetEntryAsync<TUser>(filter,
                searchBases,
                this.UserAttributes,
                this.MapUser);
        #endregion

        #region Private Properties
        /// <summary>
        /// Gets the LDAP connection or throws
        /// <see cref="ObjectDisposedException"/>.
        /// </summary>
        private LdapConnection Connection {
            get {
                ObjectDisposedException.ThrowIf(this._connection == null, this);
                return this._connection;
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets all entries matching <paramref name="filter"/> and maps them
        /// to <typeparamref name="TObject"/>s using <paramref name="map"/> and
        /// the specified <paramref name="attributes"/>.
        /// </summary>
        private IEnumerable<TObject> GetEntries<TObject>(string filter,
                IDictionary<string, SearchScope>? searchBases,
                string[] attributes,
                string sortAttribute,
                Func<LdapEntry, TObject, TObject> map,
                CancellationToken cancellationToken)
                where TObject : class, new() {
            Debug.Assert(attributes != null);
            Debug.Assert(sortAttribute != null);
            Debug.Assert(map != null);
            ArgumentException.ThrowIfNullOrEmpty(filter, nameof(filter));

            if (searchBases == null) {
                searchBases = this.Options.SearchBases;
                this._logger.LogDebug("No search bases specified, using "
                    + "{SearchBases} from the LDAP options.",
                    string.Join(", ", searchBases.Keys));
            }

            foreach (var b in searchBases) {
                // Perform a paged search (there might be a lot of users, which
                // cannot be retruned at once.
                var entries = this.Connection.PagedSearch(
                    b.Key,
                    b.Value,
                    filter,
                    attributes,
                    this.Options.PageSize,
                    sortAttribute,
                    this.Options.Timeout,
                    this._logger,
                    cancellationToken);

                // Convert LDAP entries to objects.
                foreach (var e in entries) {
                    yield return map(e, new TObject());
                }
            }
        }

        /// <summary>
        /// Gets a single entry matching <paramref name="filter"/> and maps it
        /// to <typeparamref name="TObject"/> using <paramref name="map"/> and
        /// the specified <paramref name="attributes"/>.
        /// </summary>
        private TObject? GetEntry<TObject>(string filter,
                IDictionary<string, SearchScope>? searchBases,
                string[] attributes,
                Func<LdapEntry, TObject, TObject> map)
                where TObject : class, new() {
            Debug.Assert(attributes != null);
            Debug.Assert(map != null);
            ArgumentException.ThrowIfNullOrEmpty(filter, nameof(filter));

            if (searchBases == null) {
                searchBases = this.Options.SearchBases;
                this._logger.LogDebug("No search bases specified, using "
                    + "{SearchBases} from the LDAP options.",
                    string.Join(", ", searchBases.Keys));
            }

            foreach (var b in searchBases) {
                var res = this.Connection.Search(b, filter, attributes);
                var entry = res.NextEntry();
                if (entry != null) {
                    return map(entry, new TObject());
                }
            }

            // Not found at this point.
            this._logger.LogWarning("An entry matching {Filter} does not exist "
                + "in the directory in the specified search locations "
                + "{SearcBases}.", filter, string.Join(", ", searchBases.Keys));
            return null;
        }

        /// <summary>
        /// Gets a single entry matching <paramref name="filter"/> and maps it
        /// to <typeparamref name="TObject"/> using <paramref name="map"/> and
        /// the specified <paramref name="attributes"/>.
        /// </summary>
        private async Task<TObject?> GetEntryAsync<TObject>(string filter,
                IDictionary<string, SearchScope>? searchBases,
                string[] attributes,
                Func<LdapEntry, TObject, TObject> map)
                where TObject : class, new() {
            Debug.Assert(attributes != null);
            Debug.Assert(map != null);
            ArgumentException.ThrowIfNullOrEmpty(filter, nameof(filter));

            if (searchBases == null) {
                searchBases = this.Options.SearchBases;
                this._logger.LogDebug("No search bases specified, using "
                    + "{SearchBases} from the LDAP options.",
                    string.Join(", ", searchBases.Keys));
            }

            foreach (var b in searchBases) {
                var entry = (await this.Connection.SearchAsync(
                    b, filter, attributes, this.Options.PollingInterval)
                    .ConfigureAwait(false))
                    .FirstOrDefault();
                if (entry != null) {
                    return map(entry, new TObject());
                }
            }

            // Not found at this point.
            this._logger.LogWarning("An entry matching {Filter} does not exist "
                + "in the directory in the specified search locations "
                + "{SearcBases}.", filter, string.Join(", ", searchBases.Keys));
            return null;
        }

        /// <summary>
        /// Maps the given <paramref name="entry"/> to the given
        /// <paramref name="group"/>, adding group memberships as well should
        /// they be configured.
        /// </summary>
        private TGroup MapGroup(LdapEntry entry, TGroup group) {
            Debug.Assert(entry != null);
            Debug.Assert(group != null);

            this._mapper.MapGroup(entry, group);

            if (this._mapper.GroupIsGroupMember) {
                var groups = entry.GetGroups(this.Connection,
                    this._cache,
                    this._mapper,
                    this.Options);
                this._mapper.SetGroups(group, groups);
            }

            return group;
        }

        /// <summary>
        /// Maps the given <paramref name="entry"/> to the given
        /// <paramref name="user"/>, adding group memberships as well should
        /// they be configured.
        /// </summary>
        private TUser MapUser(LdapEntry entry, TUser user) {
            Debug.Assert(entry != null);
            Debug.Assert(user != null);

            this._mapper.MapUser(entry, user);

            if (this._mapper.UserIsGroupMember) {
                var groups = entry.GetGroups(this.Connection,
                    this._cache,
                    this._mapper,
                    this.Options);
                this._mapper.SetGroups(user, groups);
            }

            return user;
        }
        #endregion

        #region Private fields
        private readonly ILdapCache<LdapEntry> _cache;
        private LdapConnection? _connection;
        private readonly ILogger _logger;
        private readonly ILdapMapper<LdapEntry, TUser, TGroup> _mapper;
        #endregion
    }
}
