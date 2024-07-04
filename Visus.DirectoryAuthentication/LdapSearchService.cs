// <copyright file="LdapSearchService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
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
        /// <param name="connectionService">The connection service providing the
        /// LDAP connections along with the options.</param>
        /// <param name="mapper">A <see cref="ILdapMapper{TUser, TGroup}"/> that
        /// provides a mapping between LDAP attributes and properties of
        /// <typeparamref name="TUser"/>.</param>
        /// <param name="claimsBuilder">A helper that creates
        /// <see cref="Claim"/>s from a user object.</param>
        /// <param name="logger">A logger for persisting important messages like
        /// failed search requests.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapSearchService(ILdapConnectionService connectionService,
                ILdapMapper<TUser, TGroup> mapper,
                IClaimsBuilder<TUser, TGroup> claimsBuilder,
                ILogger<LdapSearchService<TUser, TGroup>> logger) {
            this._claimsBuilder = claimsBuilder
                ?? throw new ArgumentNullException(nameof(claimsBuilder));
            this._connectionService = connectionService
                ?? throw new ArgumentNullException(nameof(connectionService));
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
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

            foreach (var b in this.Options.SearchBases) {
                // Perform a paged search (there might be a lot of matching
                // entries which cannot be returned at once).
                var entries = this.Connection.PagedSearch(
                    b.Key,
                    b.Value,
                    filter,
                    Array.Empty<string>(),
                    this.Options.Servers.First().PageSize,  // TODO: fix this if fallback available
                    "CN",
                    this.Options.Timeout);

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

            foreach (var b in this.Options.SearchBases) {
                // Perform a paged search (there might be a lot of matching
                // entries which cannot be returned at once).
                var entries = await this.Connection.PagedSearchAsync(
                    b.Key,
                    b.Value,
                    filter,
                    Array.Empty<string>(),
                    this.Options.Servers.First().PageSize,  // TODO: fix this if fallback available
                    "CN",
                    this.Options.Timeout).ConfigureAwait(false);

                retval = retval.Concat(
                    entries.Select(e => e.DistinguishedName));
            }

            return retval;
        }

        /// <inheritdoc />
        public TUser GetUserByIdentity(string identity) {
            var retval = new TUser();

            foreach (var b in this.Options.SearchBases) {
                var req = this.GetUserByIdentitySearchRequest(retval, identity,
                    b);
                var res = this.Connection.SendRequest(req, this.Options);

                if ((res is SearchResponse s) && s.Any()) {
                    this._mapper.Assign(s.Entries[0], this.Connection, retval);
                    this._claimsBuilder.AddClaims(retval);
                    return retval;
                }
            }

            // Not found at this point.
            this.LogEntryNotFound(identity);
            return null;
        }

        /// <inheritdoc />
        public async Task<TUser> GetUserByIdentityAsync(string identity) {
            var retval = new TUser();

            foreach (var b in this.Options.SearchBases) {
                var req = this.GetUserByIdentitySearchRequest(retval, identity,
                    b);
                var res = await this.Connection.SendRequestAsync(req,
                    this.Options).ConfigureAwait(false);

                if ((res is SearchResponse s) && s.Any()) {
                    this._mapper.Assign(s.Entries[0], this.Connection, retval);
                    this._claimsBuilder.AddClaims(retval);
                    return retval;
                }
            }

            // Not found at this point.
            this.LogEntryNotFound(identity);
            return null;
        }

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers()
            => this.GetUsers0(this.Options.Mapping.UsersFilter,
                this.Options.SearchBases);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync()
            => this.GetUsersAsync0(this.Options.Mapping.UsersFilter,
                this.Options.SearchBases);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(string filter)
            => this.GetUsers(this.Options.SearchBases, filter);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync(string filter)
            => this.GetUsersAsync(this.Options.SearchBases, filter);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(
                IDictionary<string, SearchScope> searchBases,
                string filter)
            => this.GetUsers0(this.MergeFilter(filter),
                searchBases ?? this.Options.SearchBases);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync(
                IDictionary<string, SearchScope> searchBases,
                string filter)
            => this.GetUsersAsync0(this.MergeFilter(filter),
                searchBases ?? this.Options.SearchBases);
        #endregion

        #region Private Properties
        /// <summary>
        /// Gets the lazily established connection to the directory service.
        /// </summary>
        private LdapConnection Connection {
            get {
                if (this._connection == null) {
                    this._connection = this._connectionService.Connect();
                }
                return this._connection;
            }
        }

        /// <summary>
        /// Gets the <see cref="LdapOptions"/> via the connection service.
        /// </summary>
        private LdapOptions Options => this._connectionService.Options;
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
                return this.Options.Mapping.UsersFilter;
            } else {
                return $"(&{this.Options.Mapping.UsersFilter}{filter})";
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

            var idAttribute = LdapAttributeAttribute.GetLdapAttribute<TUser>(
                nameof(LdapUser.Identity), this.Options.Schema);

            return new SearchRequest(searchBase.Key,
                $"({idAttribute.Name}={identity})",
                searchBase.Value,
                this._mapper.RequiredUserAttributes.ToArray());
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
            var user = new TUser();

            // Determine the property to sort the results, which is required
            // as paging LDAP results requires sorting.
            var sortAttribute = LdapAttributeAttribute.GetLdapAttribute<TUser>(
                nameof(LdapUser.Identity), this.Options.Schema);

            foreach (var b in searchBases) {
                // Perform a paged search (there might be a lot of users, which
                // cannot be retruned at once.
                var entries = this.Connection.PagedSearch(
                b.Key,
                b.Value,
                filter,
                this._mapper.RequiredUserAttributes.ToArray(),
                this.Options.Servers.First().PageSize,  // TODO: fix this if fallback available
                sortAttribute.Name,
                this.Options.Timeout);

                // Convert LDAP entries to user objects.
                foreach (var e in entries) {
                    this._mapper.Assign(e, this.Connection, user);
                    yield return this._claimsBuilder.AddClaims(user);
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
                nameof(LdapUser.Identity), this.Options.Schema);
            this._logger.LogError(Properties.Resources.ErrorEntryNotFound,
                att.Name, identity);
        }
        #endregion

        #region Private fields
        private readonly IClaimsBuilder<TUser, TGroup> _claimsBuilder;
        private readonly ILdapConnectionService _connectionService;
        private LdapConnection _connection;
        private readonly ILogger _logger;
        private readonly ILdapMapper<TUser, TGroup> _mapper;
        #endregion
    }
}
