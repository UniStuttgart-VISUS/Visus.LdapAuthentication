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


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Implementation of <see cref="ILdapSearchService"/> using the search
    /// attributes defined by the active mapping of
    /// <typeparamref name="TUser"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of user to be created for the search
    /// results, which also defines attributes like the unique identity in
    /// combination with the global options from <see cref="LdapOptions"/>.
    /// </typeparam>
    public sealed class LdapSearchService<TUser> : ILdapSearchService<TUser>
            where TUser : class, ILdapUser, new() {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP options that specify how to connect
        /// to the directory server.</param>
        /// <param name="logger">A logger for writing important messages.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapSearchService(IOptions<LdapOptions> options,
                ILogger<LdapSearchService<TUser>> logger) {
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetDistinguishedNames(string filter) {
            _ = filter ?? throw new ArgumentNullException(nameof(filter));

            foreach (var b in this._options.SearchBase) {
                // Perform a paged search (there might be a lot of matching
                // entries which cannot be returned at once).
                var entries = this.Connection.PagedSearch(
                    b.Key,
                    b.Value,
                    filter,
                    Array.Empty<string>(),
                    this._options.PageSize,
                    "CN",
                    this._options.Timeout);

                foreach (var e in entries) {
                    yield return e.DistinguishedName;
                }
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetDistinguishedNamesAsync(
                string filter) {
            _ = filter ?? throw new ArgumentNullException(nameof(filter));
            var retval = Enumerable.Empty<string>();

            foreach (var b in this._options.SearchBase) {
                // Perform a paged search (there might be a lot of matching
                // entries which cannot be returned at once).
                var entries = await this.Connection.PagedSearchAsync(
                    b.Key,
                    b.Value,
                    filter,
                    Array.Empty<string>(),
                    this._options.PageSize,
                    "CN",
                    this._options.Timeout).ConfigureAwait(false);

                retval = retval.Concat(
                    entries.Select(e => e.DistinguishedName));
            }

            return retval;
        }

        /// <inheritdoc />
        public TUser GetUserByIdentity(string identity) {
            var retval = new TUser();

            foreach (var b in this._options.SearchBase) {
                var req = this.GetUserByIdentitySearchRequest(retval, identity,
                    b);
                var res = this.Connection.SendRequest(req, this._options);

                if ((res is SearchResponse s) && s.Any()) {
                    retval.Assign(s.Entries[0], this.Connection, this._options);
                    return retval;
                }
            }

            // Not found at this point.
            this.LogEntryNotFound(identity);
            return null;
        }

        /// <inheritdoc />
        ILdapUser ILdapSearchService.GetUserByIdentity(string identity)
            => this.GetUserByIdentity(identity);

        /// <inheritdoc />
        public async Task<TUser> GetUserByIdentityAsync(string identity) {
            var retval = new TUser();

            foreach (var b in this._options.SearchBase) {
                var req = this.GetUserByIdentitySearchRequest(retval, identity,
                    b);
                var res = await this.Connection.SendRequestAsync(req,
                    this._options).ConfigureAwait(false);

                if ((res is SearchResponse s) && s.Any()) {
                    retval.Assign(s.Entries[0], this.Connection, this._options);
                    return retval;
                }
            }

            // Not found at this point.
            this.LogEntryNotFound(identity);
            return null;
        }

        /// <inheritdoc />
        async Task<ILdapUser> ILdapSearchService.GetUserByIdentityAsync(
                string identity)
            => await this.GetUserByIdentityAsync(identity)
                .ConfigureAwait(false);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers()
            => this.GetUsers0(this._options.Mapping.UsersFilter,
                this._options.SearchBase);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync()
            => this.GetUsersAsync0(this._options.Mapping.UsersFilter,
                this._options.SearchBase);

        /// <inheritdoc />
        IEnumerable<ILdapUser> ILdapSearchService.GetUsers()
            => this.GetUsers0(this._options.Mapping.UsersFilter,
                this._options.SearchBase);

        /// <inheritdoc />
        async Task<IEnumerable<ILdapUser>> ILdapSearchService.GetUsersAsync()
            => await this.GetUsersAsync0(this._options.Mapping.UsersFilter,
                this._options.SearchBase).ConfigureAwait(false);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(string filter)
            => this.GetUsers(this._options.SearchBase, filter);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync(string filter)
            => this.GetUsersAsync(this._options.SearchBase, filter);

        /// <inheritdoc />
        IEnumerable<ILdapUser> ILdapSearchService.GetUsers(string filter)
            => this.GetUsers(this._options.SearchBase, filter);

        /// <inheritdoc />
        async Task<IEnumerable<ILdapUser>> ILdapSearchService.GetUsersAsync(
                string filter)
            => await this.GetUsersAsync(this._options.SearchBase, filter)
                .ConfigureAwait(false);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(
                IDictionary<string, SearchScope> searchBases,
                string filter)
            => this.GetUsers0(this.MergeFilter(filter),
                searchBases ?? this._options.SearchBase);

        /// <inheritdoc />
        IEnumerable<ILdapUser> ILdapSearchService.GetUsers(
                IDictionary<string, SearchScope> searchBases,
                string filter)
            => this.GetUsers(searchBases, filter);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync(
                IDictionary<string, SearchScope> searchBases,
                string filter)
            => this.GetUsersAsync0(this.MergeFilter(filter),
                searchBases ?? this._options.SearchBase);

        /// <inheritdoc />
        async Task<IEnumerable<ILdapUser>> ILdapSearchService.GetUsersAsync(
                IDictionary<string, SearchScope> searchBases,
                string filter)
            => await this.GetUsersAsync(searchBases, filter)
                .ConfigureAwait(false);
        #endregion

        #region Private Properties
        /// <summary>
        /// Gets the lazily established connection to the directory service.
        /// </summary>
        private LdapConnection Connection {
            get {
                if (this._connection == null) {
                    this._connection = this._options.Connect(this._options.User,
                        this._options.Password, this._logger);
                }
                return this._connection;
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Merges the given filter with the default filter in
        /// <see cref="_options"/>.
        /// </summary>
        /// <param name="filter">The user-provided filter, which may be
        /// <c>null</c>.</param>
        /// <returns>The actual filter to be used in a query.</returns>
        private string MergeFilter(string filter) {
            if (string.IsNullOrWhiteSpace(filter)) {
                return this._options.Mapping.UsersFilter;
            } else {
                return $"(&{this._options.Mapping.UsersFilter}{filter})";
            }
        }

        /// <summary>
        /// Disposes managed resources if <paramref name="isDisposing"/> is
        /// <c>true</c>.
        /// </summary>
        /// <param name="isDisposing"></param>
        private void Dispose(bool isDisposing) {
            if (isDisposing) {
                if (this._connection != null) {
                    this._connection.Dispose();
                }
            }

            this._connection = null;
        }

        /// <summary>
        /// Gets the <see cref="SearchRequest"/> for retrieving the properties
        /// of <paramref name="user"/> for the user with the given
        /// <paramref name="identity"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="identity"></param>
        /// <param name="searchBase"></param>
        /// <returns></returns>
        private SearchRequest GetUserByIdentitySearchRequest(TUser user,
                string identity, KeyValuePair<string, SearchScope> searchBase) {
            _ = identity ?? throw new ArgumentNullException(nameof(identity));
            Debug.Assert(user != null);

            var groupAttribs = this._options.Mapping.RequiredGroupAttributes;
            var idAttribute = LdapAttributeAttribute.GetLdapAttribute<TUser>(
                nameof(LdapUser.Identity), this._options.Schema);

            return new SearchRequest(searchBase.Key,
                $"({idAttribute.Name}={identity})",
                searchBase.Value,
                user.RequiredAttributes.Concat(groupAttribs).ToArray());
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
            var groupAttribs = this._options.Mapping.RequiredGroupAttributes;
            var user = new TUser();

            // Determine the property to sort the results, which is required
            // as paging LDAP results requires sorting.
            var sortAttribute = LdapAttributeAttribute.GetLdapAttribute<TUser>(
                nameof(LdapUser.Identity), this._options.Schema);

            foreach (var b in searchBases) {
                // Perform a paged search (there might be a lot of users, which
                // cannot be retruned at once.
                var entries = this.Connection.PagedSearch(
                b.Key,
                b.Value,
                filter,
                user.RequiredAttributes.Concat(groupAttribs).ToArray(),
                this._options.PageSize,
                sortAttribute.Name,
                this._options.Timeout);

                // Convert LDAP entries to user objects.
                foreach (var e in entries) {
                    user.Assign(e, this.Connection, this._options);
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
            return Task.Factory.StartNew<IEnumerable<TUser>>(
                () => this.GetUsers0(filter, searchBases));

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
        /// Logs that the entry with the specified Identity was not found.
        /// </summary>
        /// <param name="identity"></param>
        private void LogEntryNotFound(string identity) {
            var att = LdapAttributeAttribute.GetLdapAttribute<TUser>(
                nameof(LdapUser.Identity), this._options.Schema);
            this._logger.LogError(Properties.Resources.ErrorEntryNotFound,
                att.Name, identity);
        }
        #endregion

        #region Private fields
        private LdapConnection _connection;
        private readonly ILogger _logger;
        private readonly LdapOptions _options;
        #endregion
    }
}
