// <copyright file="LdapConnectionService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
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
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Visus.LdapAuthentication.Configuration;
using Visus.LdapAuthentication.Properties;


namespace Visus.LdapAuthentication.Services {

    /// <summary>
    /// Generic LDAP connection service using the server and credentials
    /// specified in <see cref="IOptions"/>.
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
            var retval = this.TryConnect();

            if ((username != null)
                    && !string.IsNullOrWhiteSpace(this._options.DefaultDomain)
                    && !GetUpnRegex().IsMatch(username)) {
                username = $"{username}@{this._options.DefaultDomain}";
            }

            this._logger.LogDebug("User name to bind (possibly expanded by the "
                + "default domain) is {username}.", username);

            if ((username == null) && (password == null)) {
                this._logger.LogInformation(Resources.InfoBindAnonymous);
                retval.Bind(null, null);
                this._logger.LogInformation(Resources.InfoBoundAnonymous);

            } else {
                this._logger.LogInformation(Resources.InfoBindingAsUser,
                    username);
                retval.Bind(username, password);
                this._logger.LogInformation(Resources.InfoBoundAsUser, username);
            }

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

        #region Private methods
        /// <summary>
        /// Block the <paramref name="server"/> at the specified position for
        /// the configured amount of time.
        /// </summary>
        private void Blacklist(int server) {
            Debug.Assert(server < this._options.Servers.Length);

            // Only enter the critical section if a valid timespan was
            // specified that actually needs to expire.
            if (this._options.BlacklistFailedServersFor > TimeSpan.Zero) {
                lock (this._lock) {
                    var until = DateTimeOffset.UtcNow;
                    until += this._options.BlacklistFailedServersFor;
                    this._blacklisted[server] = until;
                    this._logger.LogWarning(Resources.WarnServerBlacklisted,
                        this._options.Servers[server], until);

                    // If the currently active server is the blacklisted one,
                    // make sure that we now select another one in the
                    // critical section we are already in to make the
                    // failover policy basically free.
                    if (this._currentServer == server) {
                        do {
                            ++this._currentServer;
                            this._currentServer %= this._options.Servers.Length;
                        } while (this.IsBlacklistedUnsafe(this._currentServer)
                            && (this._currentServer != server));

                        if (this._currentServer == server) {
                            this._logger.LogWarning(Resources.WarnNoFallback);
                        }
                    }
                }
            } /* if (this._options.BlacklistFailedServersFor > TimeSpan.Zero) */
        }

        /// <summary>
        /// Checks whether <paramref name="server"/> is on the black list, but
        /// does not acquire the <see cref="_lock"/> before doing so. Therefore,
        /// the caller <i>must</i> already hold the lock.
        /// </summary>
        private bool IsBlacklistedUnsafe(int server) {
            var retval = this._blacklisted.TryGetValue(server, out var timeout);

            if (retval) {
                // If on blacklist, check whether it might have expired in the
                // meantime. If so, remove the server from the blacklist.
                retval = (DateTimeOffset.UtcNow < timeout);

                if (!retval) {
                    this._blacklisted.Remove(server);
                }
            }

            return retval;
        }

        /// <summary>
        /// Selects the next server to be used based on the
        /// <see cref="LdapOptions.ServerSelectionPolicy"/>
        /// </summary>
        private int Select() => this._options.ServerSelectionPolicy switch {
            ServerSelectionPolicy.Failover => this.SelectFailover(),
            ServerSelectionPolicy.RoundRobin => this.SelectRoundRobin(),
            _ => throw new NotImplementedException(Resources.ErrorUnknownServerSelectionPolicy)
        };

        /// <summary>
        /// Selects teh next server to be used using the
        /// <see cref="ServerSelectionPolicy.Failover"/>.
        /// </summary>
        private int SelectFailover() => this._currentServer;

        /// <summary>
        /// Selects the next server to be used using the
        /// <see cref="ServerSelectionPolicy.RoundRobin"/>.
        /// </summary>
        private int SelectRoundRobin() {
            Debug.Assert(this._options != null);
            Debug.Assert(this._options.Servers != null);
            Debug.Assert(this._options.Servers.Length > 0);
            Debug.Assert(this._currentServer < this._options.Servers.Length);
            lock (this._lock) {
                var retval = this._currentServer;

                do {
                    ++this._currentServer;
                    this._currentServer %= this._options.Servers.Length;
                } while (this.IsBlacklistedUnsafe(this._currentServer)
                    && (this._currentServer != retval));

                return retval;
            }
        }

        /// <summary>
        /// Try connecting to any of the configured servers.
        /// </summary>
        private LdapConnection TryConnect() {
            Exception? error = null;

            for (int i = 0; i < this._options.Servers.Length; ++i) {
                var selection = this.Select();
                var server = this._options.Servers[selection];

                try {
                    this._logger.LogInformation(Resources.InfoServerSelected,
                        server);
                    return this._options.ToConnection(server, this._logger);
                } catch (Exception ex) {
                    this.Blacklist(selection);
                    error = ex;
                }
            }

            if (error != null) {
                throw error;
            } else {
                throw new InvalidOperationException(Resources.WarnNoFallback);
            }
        }
        #endregion

        #region Private fields
        private readonly Dictionary<int, DateTimeOffset> _blacklisted = new();
        private int _currentServer = 0;
        private readonly object _lock = new();
        private readonly ILogger _logger;
        private readonly LdapOptions _options;
        #endregion
    }
}
