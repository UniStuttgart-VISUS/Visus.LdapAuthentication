﻿// <copyright file="LdapSearchService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
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
    /// combination with the global options from <see cref="ILdapOptions"/>.
    /// </typeparam>
    public sealed class LdapSearchService<TUser>
            : ILdapSearchService, IDisposable where TUser : ILdapUser, new() {

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
        public LdapSearchService(ILdapOptions options,
                ILogger<LdapSearchService<TUser>> logger) {
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this._options = options
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
        public ILdapUser GetUserByIdentity(string identity) {
            _ = identity ?? throw new ArgumentNullException(nameof(identity));

            var groupAttribs = this._options.Mapping.RequiredGroupAttributes;
            var retval = new LdapUser();

            // Determine the ID attribute.
            var idAttribute = LdapAttributeAttribute.GetLdapAttribute<TUser>(
                nameof(LdapUser.Identity), this._options.Schema);

            var entries = this.Connection.Search(
                this._options.SearchBase,
                this._options.GetSearchScope(),
                $"{idAttribute.Name}={identity}",
                retval.RequiredAttributes.Concat(groupAttribs).ToArray(),
                false);

            if (entries.HasMore()) {
                retval.Assign(entries.Next(), this.Connection, this._options);
                return retval;

            } else {
                this._logger.LogError(Properties.Resources.ErrorEntryNotFound,
                    idAttribute.Name, identity);
                return null;
            }
        }

        /// <inheritdoc />
        public IEnumerable<ILdapUser> GetUsers() {
            return this.GetUsers0(this._options.Mapping.UsersFilter);
        }

        /// <inheritdoc />
        public IEnumerable<ILdapUser> GetUsers(string filter) {
            if (string.IsNullOrWhiteSpace(filter)) {
                return this.GetUsers();

            } else {
                filter = $"(&{this._options.Mapping.UsersFilter}{filter})";
                return this.GetUsers0(filter);
            }
        }
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
        /// <param name="filter"></param>
        /// <returns></returns>
        private IEnumerable<ILdapUser> GetUsers0(string filter) {
            Debug.Assert(filter != null);
            var groupAttribs = this._options.Mapping.RequiredGroupAttributes;
            var user = new TUser();

            // Determine the property to sort the results, which is required
            // as paging LDAP results requires sorting.
            var sortAttribute = LdapAttributeAttribute.GetLdapAttribute<TUser>(
                nameof(LdapUser.Identity), this._options.Schema);

            // Perform a paged search (there might be a lot of users, which
            // cannot be retruned at once.
            var entries = this.Connection.PagedSearch(
                this._options.SearchBase,
                this._options.GetSearchScope(),
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
        #endregion

        #region Private fields
        private LdapConnection _connection;
        private readonly ILogger _logger;
        private readonly ILdapOptions _options;
        #endregion
    }
}
