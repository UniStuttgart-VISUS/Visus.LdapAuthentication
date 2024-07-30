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
using System.Threading.Tasks;
using Visus.Ldap.Extensions;
using Visus.Ldap.Mapping;
using Visus.LdapAuthentication.Configuration;
using Visus.LdapAuthentication.Extensions;
using Visus.LdapAuthentication.Properties;


namespace Visus.LdapAuthentication.Services {

    /// <summary>
    /// Implementation of <see cref="ILdapSearchService"/> using the search
    /// attributes defined by the active mapping of
    /// <typeparamref name="TUser"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of user to be created for the search
    /// results, which also defines attributes like the unique identity in
    /// combination with the global options from <see cref="IOptions"/>.
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
                ILdapMapper<LdapEntry, TUser, TGroup> mapper,
                ILdapAttributeMap<TUser> userMap,
                ILogger<LdapSearchService<TUser, TGroup>> logger) {
            ArgumentNullException.ThrowIfNull(connectionService,
                nameof(connectionService));

            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
            this._mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
            this._userMap = userMap
                ?? throw new ArgumentNullException(nameof(userMap));

            this._connection = connectionService.Connect(
                this._options.User, this._options.Password);

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
                var entry = this.Connection.Search(b, filter, this._userAttributes)
                    .FirstOrDefault();
                if (entry != null) {
                    this._mapper.MapUser(entry, retval);
                    var groups = entry.GetGroups(this.Connection,
                        this._mapper,
                        this._options);
                    this._mapper.SetGroups(retval, groups);
                    return retval;
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
                var entry = (await this.Connection.SearchAsync(b, filter,
                    this._userAttributes, this._options.PollingInterval))
                    .FirstOrDefault();
                if (entry != null) {
                    this._mapper.MapUser(entry, retval);
                    var groups = entry.GetGroups(this.Connection,
                        this._mapper,
                        this._options);
                    this._mapper.SetGroups(retval, groups);
                    return retval;
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
                // cannot be retruned at once).
                var entries = Connection.PagedSearch(
                    b.Key,
                    b.Value,
                    filter,
                    this._userAttributes,
                    _options.PageSize,
                    sortAttribute.Name,
                    this._options.Timeout,
                    this._logger);

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
                IDictionary<string, SearchScope> searchBases)
            => Task.Factory.StartNew(() => GetUsers0(filter, searchBases));

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
        private readonly ILdapMapper<LdapEntry, TUser, TGroup> _mapper;
        private readonly LdapOptions _options;
        private readonly string[] _userAttributes;
        private readonly ILdapAttributeMap<TUser> _userMap;

        #endregion
    }
}
