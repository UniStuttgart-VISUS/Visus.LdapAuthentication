// <copyright file="LdapSearchService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Extensions;
using Visus.Ldap;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Services {

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
                ILdapMapper<SearchResultEntry, TUser, TGroup> mapper,
                ILdapAttributeMap<TUser> userMap,
                ILdapAttributeMap<TGroup> groupMap,
                ILdapCache<SearchResultEntry> cache,
                ILogger<LdapSearchService<TUser, TGroup>> logger)
                : base(options, userMap, groupMap) {
            ArgumentNullException.ThrowIfNull(connectionService);

            this._cache = cache
                ?? throw new ArgumentNullException(nameof(cache));
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
            this._mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));

            this._connection = connectionService.Connect(
                this._options.User, this._options.Password);
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

        #region Private properties
        /// <summary>
        /// Gets the lazily established connection to the directory service.
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
                Func<SearchResultEntry, TObject, TObject> map,
                CancellationToken cancellationToken)
                where TObject : class, new () {
            Debug.Assert(attributes != null);
            Debug.Assert(sortAttribute != null);
            Debug.Assert(map != null);
            ArgumentException.ThrowIfNullOrEmpty(filter, nameof(filter));

            if (searchBases == null) {
                searchBases = this._options.SearchBases;
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
                    this._options.PageSize,
                    sortAttribute,
                    this._options.Timeout,
                    cancellationToken);

                // Convert LDAP entries to objects.
                foreach (var e in entries) {
                    yield return map(e, new TObject());
                }
            }
        }

        /// <summary>
        /// Gets all entries matching <paramref name="filter"/> and maps them
        /// to <typeparamref name="TObject"/>s using <paramref name="map"/> and
        /// the specified <paramref name="attributes"/>.
        /// </summary>
        /// <remarks>
        /// We do not use this method atm, because it is insanely slow in
        /// contrast to wrapping the synchronous method in a task.
        /// </remarks>
        private async Task<IEnumerable<TObject>> GetEntriesAsync<TObject>(
            string filter,
                IDictionary<string, SearchScope>? searchBases,
                string[] attributes,
                string sortAttribute,
                Func<SearchResultEntry, TObject, TObject> map,
                CancellationToken cancellationToken)
                where TObject : class, new() {
            Debug.Assert(attributes != null);
            Debug.Assert(sortAttribute != null);
            Debug.Assert(map != null);
            ArgumentException.ThrowIfNullOrEmpty(filter, nameof(filter));

            if (searchBases == null) {
                searchBases = this._options.SearchBases;
                this._logger.LogDebug("No search bases specified, using "
                    + "{SearchBases} from the LDAP options.",
                    string.Join(", ", searchBases.Keys));
            }

            var retval = new List<TObject>();

            foreach (var b in searchBases) {
                // Perform a paged search (there might be a lot of users, which
                // cannot be retruned at once.
                var entries = await this.Connection.PagedSearchAsync(
                    b.Key,
                    b.Value,
                    filter,
                    attributes,
                    this._options.PageSize,
                    sortAttribute,
                    this._options.Timeout,
                    cancellationToken)
                    .ConfigureAwait(false);

                // Convert LDAP entries to objects.
                foreach (var e in entries) {
                    retval.Add(map(e, new TObject()));
                }
            }

            return retval;
        }

        /// <summary>
        /// Gets a single entry matching <paramref name="filter"/> and maps it
        /// to <typeparamref name="TObject"/> using <paramref name="map"/> and
        /// the specified <paramref name="attributes"/>.
        /// </summary>
        private TObject? GetEntry<TObject>(string filter,
                IDictionary<string, SearchScope>? searchBases,
                string[] attributes,
                Func<SearchResultEntry, TObject, TObject> map)
                where TObject : class, new() {
            Debug.Assert(attributes != null);
            Debug.Assert(map != null);
            ArgumentException.ThrowIfNullOrEmpty(filter, nameof(filter));

            if (searchBases == null) {
                searchBases = this._options.SearchBases;
                this._logger.LogDebug("No search bases specified, using "
                    + "{SearchBases} from the LDAP options.",
                    string.Join(", ", searchBases.Keys));
            }

            foreach (var b in searchBases) {
                var req = new SearchRequest(b.Key,
                    filter,
                    b.Value,
                    attributes);
                var res = this.Connection.SendRequest(req, this._options);

                if (res is SearchResponse s) {
                    var entry = s.Entries
                        .Cast<SearchResultEntry>()
                        .SingleOrDefault();
                    if (entry != null) {
                        return map(entry, new TObject());
                    }
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
                Func<SearchResultEntry, TObject, TObject> map)
                where TObject : class, new() {
            Debug.Assert(attributes != null);
            Debug.Assert(map != null);
            ArgumentException.ThrowIfNullOrEmpty(filter, nameof(filter));

            if (searchBases == null) {
                searchBases = this._options.SearchBases;
                this._logger.LogDebug("No search bases specified, using "
                    + "{SearchBases} from the LDAP options.",
                    string.Join(", ", searchBases.Keys));
            }

            foreach (var b in searchBases) {
                var req = new SearchRequest(b.Key,
                    filter,
                    b.Value,
                    attributes);
                var res = await this.Connection.SendRequestAsync(
                    req, this._options).ConfigureAwait(false);

                if (res is SearchResponse s) {
                    var entry = s.Entries
                        .Cast<SearchResultEntry>()
                        .SingleOrDefault();
                    if (entry != null) {
                        return map(entry, new TObject());
                    }
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
        private TGroup MapGroup(SearchResultEntry entry, TGroup group) {
            Debug.Assert(entry != null);
            Debug.Assert(group != null);

            this._mapper.MapGroup(entry, group);

            if (this._mapper.GroupIsGroupMember) {
                var groups = entry.GetGroups(this.Connection,
                    this._cache,
                    this._mapper,
                    this._options);
                this._mapper.SetGroups(group, groups);
            }

            return group;
        }

        /// <summary>
        /// Maps the given <paramref name="entry"/> to the given
        /// <paramref name="user"/>, adding group memberships as well should
        /// they be configured.
        /// </summary>
        private TUser MapUser(SearchResultEntry entry, TUser user) {
            Debug.Assert(entry != null);
            Debug.Assert(user != null);

            this._mapper.MapUser(entry, user);

            if (this._mapper.UserIsGroupMember) {
                var groups = entry.GetGroups(this.Connection,
                    this._cache,
                    this._mapper,
                    this._options);
                this._mapper.SetGroups(user, groups);
            }

            return user;
        }
        #endregion

        #region Private fields
        private readonly ILdapCache<SearchResultEntry> _cache;
        private LdapConnection? _connection;
        private readonly ILogger _logger;
        private readonly ILdapMapper<SearchResultEntry, TUser, TGroup> _mapper;
        private readonly LdapOptions _options;
        #endregion
    }
}
