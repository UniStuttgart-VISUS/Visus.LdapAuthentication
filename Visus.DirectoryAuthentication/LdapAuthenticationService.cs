// <copyright file="LdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
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
using System.Security.Claims;
using System.Threading.Tasks;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Implements an <see cref="IAuthenticationService"/> using
    /// <see cref="System.DirectoryServices.Protocols"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of the user object to be returned on
    /// login. This is typically something derived from
    /// <see cref="LdapUserBase"/> to avoid implementing a custom mapper.
    /// </typeparam>
    public sealed class LdapAuthenticationService<TUser, TGroup>
            : ILdapAuthenticationService<TUser>
            where TUser : class, new()
            where TGroup : class, new() {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="mapper">The <see cref="ILdapMapper{TUser, TGroup}"/> to
        /// provide mapping between LDAP attributes and properties of
        /// <typeparamref name="TUser"/> and <typeparamref name="TGroup"/>, 
        /// respectively.</param>
        /// <param name="options">The LDAP options that specify how to connect
        /// to the directory server.</param>
        /// <param name="logger">A logger for presisting important messages like
        /// login failures.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapAuthenticationService(ILdapMapper<TUser, TGroup> mapper,
                IOptions<LdapOptions> options,
                ILogger<LdapAuthenticationService<TUser, TGroup>> logger) {
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
        public TUser Login(string username, string password) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._options.Connect(username ?? string.Empty,
                password ?? string.Empty, this._logger);

            var retval = new TUser();

            foreach (var b in this._options.SearchBases) {
                var req = this.GetRequest(this._mapper, username, b);
                var res = connection.SendRequest(req, this._options);
                if ((res is SearchResponse s) && s.Any()) {
                    this._mapper.Assign(retval, s.Entries[0], connection,
                        this._logger);
                    return retval;
                }
            }

            // Not found ad this point.
            this._logger.LogError(Properties.Resources.ErrorUserNotFound,
                username);
            return null;
        }

        /// <inheritdoc />
        public async Task<TUser> LoginAsync(string username,
                string password) {
            // Note: It is important to pass a non-null password to make sure
            // that end users do not authenticate as the server process.
            var connection = this._options.Connect(username ?? string.Empty,
                    password ?? string.Empty, this._logger);

            var retval = new TUser();

            foreach (var b in this._options.SearchBases) {
                var req = this.GetRequest(this._mapper, username, b);
                var res = await connection.SendRequestAsync(req, this._options)
                    .ConfigureAwait(false);

                if ((res is SearchResponse s) && s.Any()) {
                    this._mapper.Assign(retval, s.Entries[0], connection,
                        this._logger);
                    return retval;
                }
            }

            // Not found ad this point.
            this._logger.LogError(Properties.Resources.ErrorUserNotFound,
                username);
            return null;
        }
        #endregion

        #region Private methods
        private SearchRequest GetRequest(ILdapMapper<TUser, TGroup> mapper,
                string username,
                string searchBase,
                SearchScope scope) {
            Debug.Assert(searchBase != null);
            var groupAttribs = this._options.Mapping.RequiredGroupAttributes;
            var filter = string.Format(this._options.Mapping.UserFilter,
                username);
            var retval = new SearchRequest(searchBase,
                filter,
                scope,
                mapper.RequiredUserAttributes.Concat(groupAttribs).ToArray());
            return retval;
        }

        private SearchRequest GetRequest(ILdapMapper<TUser, TGroup> mapper,
                string username,
                KeyValuePair<string, SearchScope> searchBase)
            => this.GetRequest(mapper, username, searchBase.Key,
                searchBase.Value);
        #endregion

        #region Private fields
        private readonly ILogger _logger;
        private readonly ILdapMapper<TUser, TGroup> _mapper;
        private readonly LdapOptions _options;
        #endregion
    }
}
