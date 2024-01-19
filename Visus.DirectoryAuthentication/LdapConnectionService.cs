// <copyright file="LdapConnectionService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using System;
using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Generic LDAP connection service using the server and credentials
    /// specified in <see cref="ILdapOptions"/>.
    /// </summary>
    /// <remarks>
    /// Use this service if you need access to the LDAP directory to compute any
    /// special claims that cannot be derived via the user classes provided by
    /// the library.
    /// </remarks>
    public sealed class LdapConnectionService : ILdapConnectionService {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public LdapConnectionService(ILdapOptions options,
                ILogger<LdapConnectionService> logger) {
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this.Options = options
                ?? throw new ArgumentNullException(nameof(options));
        }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public ILdapOptions Options { get; }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public LdapConnection Connect() {
            // TODO: Add the option to connect with negotiate on Windows.
            return this.Options.Connect(this.Options.User,
                this.Options.Password,
                this._logger);
        }

        /// <inheritdoc />
        public LdapConnection Connect(string username, string password) {
            return this.Options.Connect(username, password, this._logger);
        }
        #endregion

        #region Private fields
        private readonly ILogger _logger;
        #endregion
    }
}
