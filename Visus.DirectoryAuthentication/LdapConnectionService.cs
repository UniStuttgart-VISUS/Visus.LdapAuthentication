// <copyright file="LdapConnectionService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Text.RegularExpressions;
using Visus.DirectoryAuthentication.Properties;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Generic LDAP connection service using the server and credentials
    /// specified in <see cref="LdapOptions"/>.
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
        public LdapConnectionService(IOptions<LdapOptions> options,
                ILogger<LdapConnectionService> logger) {
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            this.Options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
        }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public LdapOptions Options { get; }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public LdapConnection Connect() {
            // TODO: Add the option to connect with negotiate on Windows.
            return this.Connect(this.Options.User, this.Options.Password);
        }

        /// <inheritdoc />
        public LdapConnection Connect(string username, string password) {
            var retval = this.Options.ToConnection(this._logger);
            Debug.Assert(retval != null);

            var rxUpn = new Regex(@".+@.+");
            if ((username != null)
                    && !string.IsNullOrWhiteSpace(this.Options.DefaultDomain)
                    && !rxUpn.IsMatch(username)) {
                username = $"{username}@{this.Options.DefaultDomain}";
            }

            this._logger.LogDebug("User name to bind (possibly expanded by the "
                + "default domain) is {username}.", username);

            if ((username == null) && (password == null)) {
                this._logger.LogInformation(Resources.InfoBindCurrent);
                retval.Bind();
                this._logger.LogInformation(Resources.InfoBoundCurrent);

            } else {
                this._logger.LogInformation(Resources.InfoBindingAsUser,
                    username);
                retval.Bind(new NetworkCredential(username, password));
                this._logger.LogInformation(Resources.InfoBoundAsUser, username);
            }

            this._logger.LogDebug("Effective authentication type after bind is "
                + "{authType}. Automatic binding is {autoBind}.",
                retval.AuthType, retval.AutoBind);

            return retval;
        }
        #endregion

        #region Private fields
        private readonly ILogger _logger;
        #endregion
    }
}
