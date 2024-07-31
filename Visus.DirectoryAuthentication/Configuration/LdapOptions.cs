// <copyright file="LdapOptions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;
using Visus.DirectoryAuthentication.Properties;
using Visus.Ldap.Configuration;


namespace Visus.DirectoryAuthentication.Configuration {

    /// <summary>
    /// Stores the configuration options for the LDAP server.
    /// </summary>
    public sealed class LdapOptions : LdapOptionsBase {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        public LdapOptions() {
            this.AuthenticationType
                = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? AuthType.Negotiate
                : AuthType.Basic;
        }
        #endregion

        #region Public properties
        /// <summary>
        /// The authentication type used to bind to the LDAP server.
        /// </summary>
        public AuthType AuthenticationType { get; set; }

        /// <summary>
        /// Gets the starting point(s) of any directory search.
        /// </summary>
        public IDictionary<string, SearchScope> SearchBases {
            get;
            set;
        } = new Dictionary<string, SearchScope>();
        #endregion

        #region Internal methods
        /// <summary>
        /// Populate a new <see cref="LdapConnection"/> with the parameters
        /// configured in this object.
        /// </summary>
        /// <param name="logger">A logger that is being installed in the
        /// certificate verification callback.</param>
        /// <returns>A new connection object that is configured, but not yet
        /// bound.</returns>
        internal LdapConnection ToConnection(ILogger logger) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    && this.IsNoCertificateCheck) {
                // Note: On Linux, the verification callback does not work, so
                // we need to tell OpenLDAP to skip the verification via setting
                // an environment variable. This must be done using glibc
                // according to https://github.com/dotnet/runtime/issues/60972.
                logger.LogWarning(Resources.WarnCertCheckDisabled);
                setenv("LDAPTLS_REQCERT", "never");
            }

            var id = new LdapDirectoryIdentifier(this.Servers, this.Port, false,
                false);
            var retval = new LdapConnection(id);
            retval.AuthType = this.AuthenticationType;

            try {
                retval.SessionOptions.SecureSocketLayer
                    = (this.TransportSecurity == TransportSecurity.Ssl);
            } catch (Exception ex) {
                // Cf. https://github.com/dotnet/runtime/issues/43890
                logger.LogWarning(ex, Resources.WarnNoSsl);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                // This API is only supported on Windows.
                // Cf. https://github.com/dotnet/runtime/issues/60972
                retval.SessionOptions.VerifyServerCertificate
                    = (con, cert) => VerifyCertificate(cert, logger);
            }

            retval.SessionOptions.ProtocolVersion = this.ProtocolVersion;
            // Cf. https://stackoverflow.com/questions/10336553/system-directoryservices-protocols-paged-get-all-users-code-suddenly-stopped-get
            retval.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

            logger.LogDebug("LdapConnection created for server(s) {server} "
                + "using port {port}, authentication type {authType} and "
                + "protocol version {protocol}. Referral chasing is "
                + "{referralChasing}.",
                string.Join(", ", Servers),
                Port,
                retval.AuthType,
                retval.SessionOptions.ProtocolVersion,
                retval.SessionOptions.ReferralChasing);

            //retval.SessionOptions.StartTransportLayerSecurity(null);

            return retval;
        }

        /// <summary>
        /// Performs verification of the server certificate.
        /// </summary>
        /// <param name="certificate">The server certificate to be verified.
        /// </param>
        /// <param name="logger">A logger to write any problems and warnings
        /// to.</param>
        /// <returns><c>true</c> if the certificate is acceptable, <c>false</c>
        /// otherwise.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c></exception>
        internal bool VerifyCertificate(X509Certificate certificate,
                ILogger logger) {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            if (this.IsNoCertificateCheck) {
                logger.LogWarning(Resources.WarnCertCheckDisabled);
                return true;
            }

            // Convert to X509Certificate2, because it gives us access to the
            // parsed dates.
            var cert = new X509Certificate2(certificate);

            if (DateTime.Now < cert.NotBefore) {
                logger.LogError(Resources.ErrorCertificateNotYetValid,
                    cert.NotBefore);
                return false;
            }

            if (DateTime.Now > cert.NotAfter) {
                logger.LogError(Resources.ErrorCertificateExpired,
                    cert.NotBefore);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(this.ServerCertificateIssuer)) {
                logger.LogInformation(Resources.InfoCheckCertIssuer,
                    certificate.Subject,
                    this.ServerCertificateIssuer);
                if (!string.Equals(certificate.Issuer,
                        this.ServerCertificateIssuer,
                        StringComparison.InvariantCultureIgnoreCase)) {
                    logger.LogError(Resources.ErrorCertIssuerMismatch,
                        issuer,
                        certificate.Issuer);
                    return false;
                }
            }

            if (this.ServerThumbprint?.Any() == true) {
                logger.LogInformation(Resources.InfoCheckCertThumbprint,
                    certificate.Subject,
                    string.Join(", ", ServerThumbprint));

                var match = from t in this.ServerThumbprint
                            where string.Equals(t, certificate.GetCertHashString(),
                                StringComparison.InvariantCultureIgnoreCase)
                            select t;

                if (!match.Any()) {
                    logger.LogError(Resources.ErrorCertThumbprintMismatch,
                        certificate.GetHashCode());
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Private class methods
        /// <summary>
        /// Provide access to the native <c>setenv</c> on Linux.
        /// </summary>
        [SupportedOSPlatform("Linux")]
        [DllImport("libc")]
        private static extern void setenv(string name, string value);
        #endregion
    }
}
