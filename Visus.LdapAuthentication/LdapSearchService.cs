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


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Implementation of <see cref="ILdapSearchService"/> using the search
    /// attributes defined by the active mapping of
    /// <typeparamref name="TUser"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of user to be created for the search
    /// results, which also defines attributes like the unique identity in
    /// combination with the global options from <see cref="IOptions"/>.
    /// </typeparam>
    public sealed class LdapSearchService<TUser> : ILdapSearchService<TUser>
            where TUser : class, ILdapUser, new() {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="mapper">The <see cref="ILdapUserMapper{TUser}"/> to
        /// provide mapping between LDAP attributes and properties of
        /// <typeparamref name="TUser"/>.</param>
        /// <param name="options">The LDAP options that specify how to connect
        /// to the directory server.</param>
        /// <param name="logger">A logger for writing important messages.
        /// </param>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapSearchService(ILdapUserMapper<TUser> mapper,
                IOptions<LdapOptions> options,
                ILogger<LdapSearchService<TUser>> logger) {
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
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

            foreach (var b in this._options.SearchBases) {
                // Perform a paged search (there might be a lot of matching
                // entries which cannot be returned at once).
                var entries = this.Connection.PagedSearch(
                    b.Key,
                    b.Value,
                    filter,
                    Array.Empty<string>(),
                    this._options.PageSize,
                    "CN",
                    this._options.Timeout,
                    this._logger);

                foreach (var e in entries) {
                    yield return e.Dn;
                }
            }
        }

        /// <inheritdoc />
        public TUser GetUserByIdentity(string identity) {
            _ = identity ?? throw new ArgumentNullException(nameof(identity));

            var groupAttribs = this._options.Mapping.RequiredGroupAttributes;
            var retval = new TUser();

            // Determine the ID attribute.
            var idAttribute = LdapAttributeAttribute.GetLdapAttribute<TUser>(
                nameof(LdapUser.Identity), this._options.Schema);

            foreach (var b in this._options.SearchBases) {
                var entries = this.Connection.Search(
                    b,
                    $"{idAttribute.Name}={identity}",
                    retval.RequiredAttributes.Concat(groupAttribs).ToArray(),
                    false);

                if (entries.HasMore()) {
                    var entry = entries.NextEntry(this._logger);
                    if (entry != null) {
                        this._mapper.Assign(retval, entry, this.Connection,
                            this._logger);
                        return retval;
                    }
                }
            }

            this._logger.LogError(Properties.Resources.ErrorEntryNotFound,
                idAttribute.Name, identity);
            return null;
        }

        /// <inheritdoc />
        ILdapUser ILdapSearchService.GetUserByIdentity(string identity)
            => this.GetUserByIdentity(identity);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers()
            => this.GetUsers0(this._options.Mapping.UsersFilter,
                this._options.SearchBases);

        /// <inheritdoc />
        IEnumerable<ILdapUser> ILdapSearchService.GetUsers()
            => this.GetUsers0(this._options.Mapping.UsersFilter,
                this._options.SearchBases);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(string filter)
            => this.GetUsers(this._options.SearchBases, filter);

        /// <inheritdoc />
        IEnumerable<ILdapUser> ILdapSearchService.GetUsers(string filter)
            => this.GetUsers(this._options.SearchBases, filter);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(
                IDictionary<string, SearchScope> searchBases,
                string filter) {
            if (string.IsNullOrWhiteSpace(filter)) {
                filter = this._options.Mapping.UsersFilter;
            } else {
                filter = $"(&{this._options.Mapping.UsersFilter}{filter})";
            }

            return this.GetUsers0(filter,
                searchBases ?? this._options.SearchBases);
        }

        /// <inheritdoc />
        IEnumerable<ILdapUser> ILdapSearchService.GetUsers(
                IDictionary<string, SearchScope> searchBases,
                string filter)
            => this.GetUsers(searchBases, filter);

        /// <inheritdoc />
        public IEnumerable<TUser> GetUsers(string searchBase,
                SearchScope searchScope, string filter) {
            var scopes = new Dictionary<string, SearchScope>() {
                {searchBase, searchScope }
            };
            return this.GetUsers(scopes, filter);
        }

        /// <inheritdoc />
        IEnumerable<ILdapUser> ILdapSearchService.GetUsers(
                string searchBase,
                SearchScope searchScope,
                string filter)
            => this.GetUsers(searchBase, searchScope, filter);

        /// <inheritdoc />
        public IEnumerable<ILdapUser> GetUsers(
                string searchBase,
                int searchScope,
                string filter)
            => this.GetUsers(searchBase, (SearchScope) searchScope, filter);
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
                // cannot be retruned at once).
                var entries = this.Connection.PagedSearch(
                    b.Key,
                    b.Value,
                    filter,
                    user.RequiredAttributes.Concat(groupAttribs).ToArray(),
                    this._options.PageSize,
                    sortAttribute.Name,
                    this._options.Timeout,
                    this._logger);

                // Convert LDAP entries to user objects.
                foreach (var e in entries) {
                    this._mapper.Assign(user, e, this.Connection, this._logger);
                    yield return user;
                    user = new TUser();
                }

            }
        }
        #endregion

        #region Private fields
        private LdapConnection _connection;
        private readonly ILogger _logger;
        private readonly ILdapUserMapper<TUser> _mapper;
        private readonly IOptions _options;
        #endregion
    }
}
