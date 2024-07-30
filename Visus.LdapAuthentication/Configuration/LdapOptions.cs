// <copyright file="LdapOptions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Visus.Ldap.Configuration;
using Visus.LdapAuthentication.Properties;


namespace Visus.LdapAuthentication.Configuration {

    /// <summary>
    /// Stores the configuration options for the LDAP server.
    /// </summary>
    public sealed class LdapOptions : LdapOptionsBase {

        #region Public properties
        /// <summary>
        /// Gets or sets the timespan for which a server that failed to connect
        /// gets blacklisted before it can be used again by the configured
        /// <see cref="ServerSelectionPolicy"/>.
        /// </summary>
        public TimeSpan BlacklistFailedServersFor {
            get => this._blacklistFailedServersFor;
            set => this._blacklistFailedServersFor = (value >= TimeSpan.Zero)
                ? value
                : -value;
        }

        /// <summary>
        /// Gets or sets the polling interval for asynchronous directory
        /// operations.
        /// </summary>
        public TimeSpan PollingInterval {
            get;
            set;
        } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Gets the thumbprint of the trusted root CA for the LDAP server.
        /// </summary>
        /// <remarks>
        /// <para>If this property is <c>null</c>, the certificate chain will
        /// not be checked, ie any root CA will be acceptable</para>
        /// <para>This property is only relevant if
        /// <see cref="TransportSecurity"/> is enabled.</para>
        /// </remarks>
        public string? RootCaThumbprint { get; set; }

        /// <summary>
        /// Gets the starting point(s) of any directory search.
        /// </summary>
        public IDictionary<string, SearchScope> SearchBases {
            get;
            set;
        } = new Dictionary<string, SearchScope>();

        /// <summary>
        /// Gets or sets how the <see cref="ILdapConnectionService"/> will select
        /// the server to connect to if multiple ones are specified.
        /// </summary>
        public ServerSelectionPolicy ServerSelectionPolicy {
            get;
            set;
        } = ServerSelectionPolicy.Failover;
        #endregion

        #region Internal methods
        /// <summary>
        /// Creates an <see cref="LdapConnection"/> as configured in
        /// <paramref name="that"/>, but do not connect to the server yet.
        /// </summary>
        /// <remarks>
        /// <para>The connection created by this method has registered a server
        /// certification validation callback that checks the policy configured
        /// in <paramref name="that"/>.</para>
        /// </remarks>
        /// <param name="logger">A logger to write certificate verification
        /// issues to.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c>.</exception>
        internal LdapConnection ToConnection(ILogger logger) {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            var options = new LdapConnectionOptions()
                .ConfigureRemoteCertificateValidationCallback((s, c, a, e)
                => this.VerifyServerCertificate(c, a, e, logger));

            if (this.TransportSecurity == TransportSecurity.Ssl) {
                options.UseSsl();
            }

            return new LdapConnection(options);
        }

        /// <summary>
        /// Creates an <see cref="LdapConnection"/> as configured in
        /// <paramref name="that"/> and connect to the given server, which should
        /// be selected by the <see cref="IServerSelectionService"/> from this
        /// options object.
        /// </summary>
        /// <param name="server">The name or IP address of the server to connect
        /// to.</param>
        /// <param name="logger">A logger to write certificate verification
        /// issues to.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="server"/>
        /// is <c>null</c>, or if <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        internal LdapConnection ToConnection(string server, ILogger logger) {
            ArgumentNullException.ThrowIfNull(server, nameof(server));
            var retval = this.ToConnection(logger);
            Debug.Assert(logger != null);

            logger.LogInformation(Resources.InfoConnectingLdap,  server,
                this.Port);
            retval.Connect(server, this.Port);

            return retval;
        }

        /// <summary>
        /// Performs verification of the server certificate.
        /// </summary>
        /// <param name="certificate">The server certificate to be verified.
        /// </param>
        /// <param name="chain">The certificate chain.</param>
        /// <param name="sslPolicyErrors">Any policy errors that have been
        /// discovered.</param>
        /// <param name="logger">A logger to write any problems and warnings
        /// to.</param>
        /// <returns><c>true</c> if the certificate is acceptable, <c>false</c>
        /// otherwise.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c></exception>
        internal bool VerifyServerCertificate(X509Certificate certificate,
                X509Chain chain,
                SslPolicyErrors sslPolicyErrors,
                ILogger logger) {
            // See https://stackoverflow.com/questions/386982/novell-ldap-c-sharp-novell-directory-ldap-has-anybody-made-it-work
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            if (this.IsNoCertificateCheck) {
                logger.LogWarning(Resources.WarnCertCheckDisabled);
                return true;
            }

            if (sslPolicyErrors != SslPolicyErrors.None) {
                logger.LogInformation("LDAP SSL policy errors are: {0}",
                    sslPolicyErrors);
                return false;
            }

            if (this.RootCaThumbprint != null) {
                var thumbprint = this.RootCaThumbprint;
                logger.LogInformation("Checking for LDAP server "
                    + "certificate \"{0}\" being issued by derived from a root "
                    + "CA with thumbprint \"{1}\".", certificate.Subject,
                    thumbprint);

                var ca = chain.ChainElements.Cast<X509ChainElement>().Last();
                if (!string.Equals(ca.Certificate.Thumbprint, thumbprint,
                        StringComparison.InvariantCultureIgnoreCase)) {
                    logger.LogError("The LDAP SSL certificate was issued via "
                        + "the root CA {0}, which is different from the "
                        + "expected one.", ca.Certificate.Thumbprint);
                    return false;
                }
            }

            if (this.ServerThumbprint?.Any() == true) {
                logger.LogInformation("Checking that LDAP server "
                    + "certificate \"{0}\" has one of the following "
                    + "thumbprints: {1}", certificate.Subject,
                    string.Join(", ", this.ServerThumbprint));

                var match = from t in this.ServerThumbprint
                            where string.Equals(t, certificate.GetCertHashString(),
                                StringComparison.InvariantCultureIgnoreCase)
                            select t;

                if (!match.Any()) {
                    logger.LogError("The LDAP SSL certificate has the "
                        + "thumbprint {0}, which is not one of the expected "
                        + "ones.", certificate.GetHashCode());
                    return false;
                }
            }

            // TODO: this.ServerCertificateIssuer

            return true;
        }
        #endregion

        #region Private fields
        private TimeSpan _blacklistFailedServersFor = TimeSpan.FromMinutes(5);
        #endregion
    }
}
