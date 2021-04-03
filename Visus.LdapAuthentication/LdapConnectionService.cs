// <copyright file="LdapConnectionService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using System;


namespace Visus.LdapAuthentication {

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
            return this.Options.Connect(this.Options.User, this.Options.Password,
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
