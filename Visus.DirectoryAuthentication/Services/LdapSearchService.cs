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
using System.Threading.Tasks;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Extensions;
using Visus.DirectoryAuthentication.Properties;
using Visus.Ldap.Extensions;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Services {

    /// <summary>
    /// Implementation of <see cref="ILdapSearchService"/> using the search
    /// attributes defined by the active mapping of
    /// <typeparamref name="TUser"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of user to be created for the search
    /// results, which also defines attributes like the unique identity in
    /// combination with the global options from <see cref="LdapOptions"/>.
    /// </typeparam>
    /// <typeparam name="TGroup">The type used to represent an LDAP group.
    /// </typeparam>
    public sealed class LdapSearchService<TUser, TGroup>
            : ILdapSearchService<TUser, TGroup>
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
        /// <typeparamref name="TUser"/> that allows the server to retrieve
        /// infromation about the user object.</param>
        /// <param name="logger">A logger for persisting important messages like
        /// failed search requests.</param>
        /// <exception cref="ArgumentNullException">If any of the parameters is
        /// <c>null</c>.</exception>
        public LdapSearchService(IOptions<LdapOptions> options,
                ILdapConnectionService connectionService,
                ILdapMapper<SearchResultEntry, TUser, TGroup> mapper,
                ILdapAttributeMap<TUser> userMap,
                ILogger<LdapSearchService<TUser, TGroup>> logger) {
            ArgumentNullException.ThrowIfNull(connectionService,
                nameof(connectionService));

            this._connection = connectionService.Connect();
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
            this._mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
            this._userMap = userMap
                ?? throw new ArgumentNullException(nameof(userMap));

            Debug.Assert(this._options.Mapping != null);
            this._userAttributes = this._mapper.RequiredUserAttributes
                .Append(this._options.Mapping.PrimaryGroupAttribute)
                .Append(this._options.Mapping.GroupsAttribute)
                .ToArray();
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ///// <inheritdoc />
        //public IEnumerable<string> GetDistinguishedNames(string filter) {
        //    ArgumentNullException.ThrowIfNull(filter, nameof(filter));

        //    foreach (var b in this._options.SearchBases) {
        //        // Perform a paged search (there might be a lot of matching
        //        // entries which cannot be returned at once).
        //        var entries = this.Connection.PagedSearch(
        //            b.Key,
        //            b.Value,
        //            filter,
        //            Array.Empty<string>(),
        //            this._options.PageSize,
        //            "CN",
        //            this._options.Timeout);

        //        foreach (var e in entries) {
        //            yield return e.DistinguishedName;
        //        }
        //    }
        //}

        ///// <inheritdoc />
        //public async Task<IEnumerable<string>> GetDistinguishedNamesAsync(
        //        string filter) {
        //    ArgumentNullException.ThrowIfNull(filter, nameof(filter));
        //    var retval = Enumerable.Empty<string>();

        //    foreach (var b in this._options.SearchBases) {
        //        // Perform a paged search (there might be a lot of matching
        //        // entries which cannot be returned at once).
        //        var entries = await this.Connection.PagedSearchAsync(
        //            b.Key,
        //            b.Value,
        //            filter,
        //            Array.Empty<string>(),
        //            this._options.PageSize,
        //            "CN",
        //            this._options.Timeout).ConfigureAwait(false);

        //        retval = retval.Concat(
        //            entries.Select(e => e.DistinguishedName));
        //    }

        //    return retval;
        //}

        /// <inheritdoc />
        public TUser? GetUserByAccountName(string accountName) {
            var att = this._userMap.AccountNameAttribute?.Name;
            var filter = accountName.EscapeLdapFilterExpression();
            return this.GetUser0($"({att}={filter})");
        }

        /// <inheritdoc />
        public Task<TUser?> GetUserByAccountNameAsync(string accountName) {
            var att = this._userMap.AccountNameAttribute?.Name;
            var filter = accountName.EscapeLdapFilterExpression();
            return this.GetUserAsync0($"({att}={filter})");
        }

        /// <inheritdoc />
        public TUser? GetUserByDistinguishedName(string distinguishedName) {
            var att = this._userMap.DistinguishedNameAttribute!.Name;
            var filter = $"({att}={distinguishedName})";
            return this.GetUser0($"({att}={filter})");
        }

        /// <inheritdoc />
        public Task<TUser?> GetUserByDistinguishedNameAsync(
                string distinguishedName) {
            var att = this._userMap.DistinguishedNameAttribute!.Name;
            var filter = distinguishedName.EscapeLdapFilterExpression();
            return this.GetUserAsync0($"({att}={filter})");
        }

        /// <inheritdoc />
        public TUser? GetUserByIdentity(string identity) {
            var att = this._userMap.IdentityAttribute!.Name;
            var filter = identity.EscapeLdapFilterExpression();
            return this.GetUser0($"({att}={filter})");
        }

        /// <inheritdoc />
        public Task<TUser?> GetUserByIdentityAsync(string identity) {
            var att = this._userMap.IdentityAttribute!.Name;
            var filter = identity.EscapeLdapFilterExpression();
            return this.GetUserAsync0($"({att}={filter})");
        }

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers()
            => GetUsers0(this._options.Mapping!.UsersFilter,
                this._options.SearchBases);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync()
            => GetUsersAsync0(this._options.Mapping!.UsersFilter,
                this._options.SearchBases);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(string filter)
            => GetUsers(this._options.SearchBases, filter);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync(string filter)
            => GetUsersAsync(this._options.SearchBases, filter);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(
                IDictionary<string, SearchScope> searchBases,
                string filter)
            => GetUsers0(MergeFilter(filter),
                searchBases ?? this._options.SearchBases);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync(
                IDictionary<string, SearchScope> searchBases,
                string filter)
            => GetUsersAsync0(MergeFilter(filter),
                searchBases ?? this._options.SearchBases);
        #endregion

        #region Private Properties
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
        /// Disposes managed resources if <paramref name="isDisposing"/> is
        /// <c>true</c>.
        /// </summary>
        /// <param name="isDisposing"></param>
        private void Dispose(bool isDisposing) {
            if (isDisposing) {
                this._connection?.Dispose();
            }

            this._connection = null;
        }

        /// <summary>
        /// Gets ta single user matching the given <paramref name="filter"/>
        /// expression.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private TUser? GetUser0(string filter) {
            var retval = new TUser();

            foreach (var b in this._options.SearchBases) {
                var req = new SearchRequest(b.Key,
                    filter,
                    b.Value,
                    this._userAttributes);
                var res = this.Connection.SendRequest(req, this._options);

                if (res is SearchResponse s) {
                    var entry = s.Entries
                        .Cast<SearchResultEntry>()
                        .SingleOrDefault();
                    if (entry != null) {
                        this._mapper.MapUser(entry, retval);
                        var groups = entry.GetGroups(this.Connection,
                            this._mapper,
                            this._options);
                        this._mapper.SetGroups(retval, groups);
                        return retval;
                    }
                }
            }

            // Not found at this point.
            this._logger.LogWarning(Resources.ErrorEntryNotFound, filter);
            return null;
        }

        /// <summary>
        /// Gets ta single user matching the given <paramref name="filter"/>
        /// expression.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private async Task<TUser?> GetUserAsync0(string filter) {
            var retval = new TUser();

            foreach (var b in this._options.SearchBases) {
                var req = new SearchRequest(b.Key,
                    filter,
                    b.Value,
                    this._userAttributes);
                var res = await this.Connection.SendRequestAsync(req, this._options)
                    .ConfigureAwait(false);

                if (res is SearchResponse s) {
                    var entry = s.Entries
                        .Cast<SearchResultEntry>()
                        .SingleOrDefault();
                    if (entry != null) {
                        this._mapper.MapUser(entry, retval);
                        var groups = entry.GetGroups(this.Connection,
                            this._mapper,
                            this._options);
                        this._mapper.SetGroups(retval, groups);
                        return retval;
                    }
                }
            }

            // Not found at this point.
            this._logger.LogWarning(Resources.ErrorEntryNotFound, filter);
            return null;
        }

        /// <summary>
        /// Retrieves the users matching the given filter.
        /// </summary>
        /// <param name="filter">A filter on the users object.</param>
        /// <param name="searchBases">The base and scope of the LDAP search.
        /// </param>
        /// <returns>The users found at the specified locations in the
        /// directory.</returns>
        private IEnumerable<TUser> GetUsers0(string filter,
                IDictionary<string, SearchScope> searchBases) {
            Debug.Assert(filter != null);
            Debug.Assert(searchBases != null);
            Debug.Assert(this._options != null);
            Debug.Assert(this._options.Mapping != null);
            var user = new TUser();

            // Determine the property to sort the results, which is required
            // as paging LDAP results requires sorting.
            var sortAttribute = this._userMap.IdentityAttribute;
            Debug.Assert(sortAttribute != null);

            foreach (var b in searchBases) {
                // Perform a paged search (there might be a lot of users, which
                // cannot be retruned at once.
                var entries = this.Connection.PagedSearch(
                    b.Key,
                    b.Value,
                    filter,
                    this._userAttributes,
                    this._options.PageSize,
                    sortAttribute.Name,
                    this._options.Timeout);

                // Convert LDAP entries to user objects.
                foreach (var e in entries) {
                    this._mapper.MapUser(e, user);
                    var groups = e.GetGroups(this.Connection,
                        this._mapper,
                        this._options);
                    this._mapper.SetGroups(user, groups);
                    yield return user;
                    user = new TUser();
                }
            }
        }

        /// <summary>
        /// Asychronously retrieves the users matching the given filter.
        /// </summary>
        /// <param name="filter">A filter on the users object.</param>
        /// <param name="searchBases">The base and scope of the LDAP search.
        /// </param>
        /// <returns>The users found at the specified locations in the
        /// directory.</returns>
        private Task<IEnumerable<TUser>> GetUsersAsync0(string filter,
                IDictionary<string, SearchScope> searchBases) {
            return Task.Factory.StartNew(
                () => GetUsers0(filter, searchBases));

            // The following is insanely slow, but IDK why, so we just wrap
            // GetUsers0 in a task instead.
            //Debug.Assert(filter != null);
            //Debug.Assert(searchBases != null);
            //var groupAttribs = this._options.Mapping.RequiredGroupAttributes;
            //var retval = new List<TUser>();
            //var user = new TUser();

            //// Determine the property to sort the results, which is required
            //// as paging LDAP results requires sorting.
            //var sortAttribute = LdapAttributeAttribute.GetLdapAttribute<TUser>(
            //    nameof(LdapUser.Identity), this._options.Schema);

            //foreach (var b in searchBases) {
            //    // Perform a paged search (there might be a lot of users, which
            //    // cannot be retruned at once.
            //    var entries = await this.Connection.PagedSearchAsync(
            //        b.Key,
            //        b.Value,
            //        filter,
            //        user.RequiredAttributes.Concat(groupAttribs).ToArray(),
            //        this._options.PageSize,
            //        sortAttribute.Name,
            //        this._options.Timeout);

            //    // Convert LDAP entries to user objects.
            //    foreach (var e in entries) {
            //        user.Assign(e, this.Connection, this._options);
            //        retval.Add(user);
            //        user = new TUser();
            //    }
            //}

            //return retval;
        }

        /// <summary>
        /// Merges the given filter with the default filter in
        /// <see cref="_options"/>.
        /// </summary>
        /// <param name="filter">The user-provided filter, which may be
        /// <c>null</c>.</param>
        /// <returns>The actual filter to be used in a query.</returns>
        private string MergeFilter(string filter) {
            if (string.IsNullOrWhiteSpace(filter)) {
                return this._options.Mapping!.UsersFilter;
            } else {
                return $"(&{this._options.Mapping!.UsersFilter}{filter})";
            }
        }
        #endregion

        #region Private fields
        private LdapConnection? _connection;
        private readonly ILogger _logger;
        private readonly ILdapMapper<SearchResultEntry, TUser, TGroup> _mapper;
        private readonly LdapOptions _options;
        private readonly string[] _userAttributes;
        private readonly ILdapAttributeMap<TUser> _userMap;
        #endregion
    }
}
