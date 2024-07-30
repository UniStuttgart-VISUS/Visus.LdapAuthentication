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
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryAuthentication.Properties;


namespace Visus.DirectoryAuthentication.Services {

    /// <summary>
    /// Generic LDAP connection service using the server and credentials
    /// specified in <see cref="LdapOptions"/>.
    /// </summary>
    /// <remarks>
    /// Use this service if you need access to the LDAP directory to compute any
    /// special claims that cannot be derived via the user classes provided by
    /// the library.
    /// </remarks>
    public sealed partial class LdapConnectionService : ILdapConnectionService {

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
            this._options = options?.Value
                ?? throw new ArgumentNullException(nameof(options));
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public LdapConnection Connect(string? username, string? password) {
            var retval = this._options.ToConnection(_logger);
            Debug.Assert(retval != null);

            if ((username != null)
                    && !string.IsNullOrWhiteSpace(this._options.DefaultDomain)
                    && !GetUpnRegex().IsMatch(username)) {
                username = $"{username}@{this._options.DefaultDomain}";
            }

            this._logger.LogDebug("User name to bind (possibly expanded by the "
                + "default domain) is {username}.", username);

            if ((username == null) && (password == null)) {
                this._logger.LogInformation(Resources.InfoBindAnonymous);
                retval.Bind();
                this._logger.LogInformation(Resources.InfoBoundAnonymous);

            } else {
                this._logger.LogInformation(Resources.InfoBindingAsUser,
                    username);
                retval.Bind(new NetworkCredential(username, password));
                this._logger.LogInformation(Resources.InfoBoundAsUser, username);
            }

            this._logger.LogDebug("Effectively connected to {server} after bind "
                + "as {user} using authentication type {authType} and protocol "
                + "version {version}. Automatic binding is {autoBind}.",
                username,
                retval.SessionOptions.HostName,
                retval.AuthType,
                retval.SessionOptions.ProtocolVersion,
                retval.AutoBind);

            return retval;
        }
        #endregion

        #region Private class methods
        /// <summary>
        /// Gets a regular expression for detecting whether the user name is a
        /// UPN.
        /// </summary>
        /// <returns></returns>
        [GeneratedRegex(@".+@.+")]
        private static partial Regex GetUpnRegex();
        #endregion

        #region Private fields
        private readonly ILogger _logger;
        private readonly LdapOptions _options;
        #endregion
    }
}
