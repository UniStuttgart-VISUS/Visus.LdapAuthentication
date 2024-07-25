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
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Extensions;


namespace Visus.DirectoryAuthentication.Services
{

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
            _claimsBuilder = claimsBuilder
                ?? throw new ArgumentNullException(nameof(claimsBuilder));
            _connectionService = connectionService
                ?? throw new ArgumentNullException(nameof(connectionService));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetDistinguishedNames(string filter) {
            _ = filter ?? throw new ArgumentNullException(nameof(filter));

            foreach (var b in Options.SearchBases) {
                // Perform a paged search (there might be a lot of matching
                // entries which cannot be returned at once).
                var entries = Connection.PagedSearch(
                    b.Key,
                    b.Value,
                    filter,
                    Array.Empty<string>(),
                    Options.PageSize,
                    "CN",
                    Options.Timeout);

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

            foreach (var b in Options.SearchBases) {
                // Perform a paged search (there might be a lot of matching
                // entries which cannot be returned at once).
                var entries = await Connection.PagedSearchAsync(
                    b.Key,
                    b.Value,
                    filter,
                    Array.Empty<string>(),
                    Options.PageSize,
                    "CN",
                    Options.Timeout).ConfigureAwait(false);

                retval = retval.Concat(
                    entries.Select(e => e.DistinguishedName));
            }

            return retval;
        }

        /// <inheritdoc />
        public TUser GetUserByIdentity(string identity) {
            var retval = new TUser();

            foreach (var b in Options.SearchBases) {
                var req = GetUserByIdentitySearchRequest(retval, identity,
                    b);
                var res = Connection.SendRequest(req, Options);

                if (res is SearchResponse s && s.Any()) {
                    _mapper.Assign(s.Entries[0], Connection, retval);
                    _claimsBuilder.AddClaims(retval);
                    return retval;
                }
            }

            // Not found at this point.
            LogEntryNotFound(identity);
            return null;
        }

        /// <inheritdoc />
        public async Task<TUser> GetUserByIdentityAsync(string identity) {
            var retval = new TUser();

            foreach (var b in Options.SearchBases) {
                var req = GetUserByIdentitySearchRequest(retval, identity,
                    b);
                var res = await Connection.SendRequestAsync(req,
                    Options).ConfigureAwait(false);

                if (res is SearchResponse s && s.Any()) {
                    _mapper.Assign(s.Entries[0], Connection, retval);
                    _claimsBuilder.AddClaims(retval);
                    return retval;
                }
            }

            // Not found at this point.
            LogEntryNotFound(identity);
            return null;
        }

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers()
            => GetUsers0(Options.Mapping.UsersFilter,
                Options.SearchBases);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync()
            => GetUsersAsync0(Options.Mapping.UsersFilter,
                Options.SearchBases);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(string filter)
            => GetUsers(Options.SearchBases, filter);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync(string filter)
            => GetUsersAsync(Options.SearchBases, filter);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(
                IDictionary<string, SearchScope> searchBases,
                string filter)
            => GetUsers0(MergeFilter(filter),
                searchBases ?? Options.SearchBases);

        /// <inheritdoc />
        public Task<IEnumerable<TUser>> GetUsersAsync(
                IDictionary<string, SearchScope> searchBases,
                string filter)
            => GetUsersAsync0(MergeFilter(filter),
                searchBases ?? Options.SearchBases);
        #endregion

        #region Private Properties
        /// <summary>
        /// Gets the lazily established connection to the directory service.
        /// </summary>
        private LdapConnection Connection {
            get {
                if (_connection == null) {
                    _connection = _connectionService.Connect();
                }
                return _connection;
            }
        }

        /// <summary>
        /// Gets the <see cref="LdapOptions"/> via the connection service.
        /// </summary>
        private LdapOptions Options => _connectionService.Options;
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
                return Options.Mapping.UsersFilter;
            } else {
                return $"(&{Options.Mapping.UsersFilter}{filter})";
            }
        }

        /// <summary>
        /// Disposes managed resources if <paramref name="isDisposing"/> is
        /// <c>true</c>.
        /// </summary>
        /// <param name="isDisposing"></param>
        private void Dispose(bool isDisposing) {
            if (isDisposing) {
                if (_connection != null) {
                    _connection.Dispose();
                }
            }

            _connection = null;
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
                nameof(LdapUser.Identity), Options.Schema);

            return new SearchRequest(searchBase.Key,
                $"({idAttribute.Name}={identity})",
                searchBase.Value,
                _mapper.RequiredUserAttributes.ToArray());
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
                nameof(LdapUser.Identity), Options.Schema);

            foreach (var b in searchBases) {
                // Perform a paged search (there might be a lot of users, which
                // cannot be retruned at once.
                var entries = Connection.PagedSearch(
                b.Key,
                b.Value,
                filter,
                _mapper.RequiredUserAttributes.ToArray(),
                Options.PageSize,
                sortAttribute.Name,
                Options.Timeout);

                // Convert LDAP entries to user objects.
                foreach (var e in entries) {
                    _mapper.Assign(e, Connection, user);
                    yield return _claimsBuilder.AddClaims(user);
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
        /// Logs that the entry with the specified Identity was not found.
        /// </summary>
        /// <param name="identity"></param>
        private void LogEntryNotFound(string identity) {
            var att = LdapAttributeAttribute.GetLdapAttribute<TUser>(
                nameof(LdapUser.Identity), Options.Schema);
            _logger.LogError(Properties.Resources.ErrorEntryNotFound,
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
