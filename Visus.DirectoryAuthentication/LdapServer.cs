// <copyright file="LdapServer.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Visus.DirectoryAuthentication.Properties;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Represents the information for connecting to a single LDAP service in
    /// <see cref="LdapOptions"/>.
    /// </summary>
    public sealed class LdapServer {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        public LdapServer() {
            this.AuthenticationType =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? AuthType.Negotiate
                : AuthType.Basic;
            this.PageSize = 1000;   // Reasonable default for AD.
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the host name or IP of the LDAP server.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The authentication type used to bind to the LDAP server.
        /// </summary>
        public AuthType AuthenticationType { get; set; }

        /// <summary>
        /// Gets the acceptable issuer of the server certificate.
        /// </summary>
        /// <remarks>
        /// <para>If this property is <c>null</c>, any issuer will be considered
        /// acceptable.</para>
        /// <para>This property is only relevant if <see cref="UseSsl"/> is
        /// enabled.</para>
        /// </remarks>
        public string CertificateIssuer { get; set; }

        /// <summary>
        /// Gets the certificate thumbprints for the LDAP servers that are
        /// accepted during certificate validation.
        /// </summary>
        /// <remarks>
        /// <para>If this array is empty, any server certificate will be
        /// accepted. Note that if <see cref="ServerCertificateIssuer"/> is set
        /// as well, the server certificate must have been issued by the
        /// specified issuer, too.</para>
        /// <para>This property is only relevant if <see cref="UseSsl"/> is
        /// enabled.</para>
        /// </remarks>
        public string[] CertificateThumbprint {
            get;
            set;
        } = Array.Empty<string>();

        /// <summary>
        /// Gets whether the certificate check is disabled for accessing the
        /// LDAP server.
        /// </summary>
        /// <remarks>
        /// <para>You should not do that for production code. This is only
        /// intended for development setups where you are working with
        /// self-signed certificates.</para>
        /// </remarks>
        public bool IsNoCertificateCheck { get; set; }

        /// <summary>
        /// Get whether the LDAP connection uses SSL or not.
        /// </summary>
        public bool IsSsl { get; set; }

        /// <summary>
        /// Gets the maximum number of results the LDAP client should request.
        /// </summary>
        /// <remarks>
        /// <para>This property is initialised to 1000, which is a reasonable
        /// default for Active Directory servers.</para>
        /// <para>If this property is zero (or negative), the implementation
        /// will not perform any paging. Results might be truncated in this
        /// case.</para>
        /// </remarks>
        public int PageSize {
            get => this._pageSize;
            set => this._pageSize = Math.Max(0, value);
        }

        /// <summary>
        /// Gets the port of the LDAP server.
        /// </summary>
        public int Port { get; set; } = 389;

        /// <summary>
        /// Gets the version of the LDAP protocol to request from the server.
        /// </summary>
        public int ProtocolVersion { get; set; } = 3;
        #endregion

        #region Internal methods
        /// <summary>
        /// Creates a new <see cref="LdapConnection"/> as configured in
        /// by this object.
        /// </summary>
        /// <remarks>
        /// Note that this extension method does not bind to the server!
        /// </remarks>
        /// <param name="logger">A logger to write messages to.</param>
        /// <returns>An LDAP connection to the configured server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c></exception>
        internal LdapConnection Connect(ILogger logger) {
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            var id = new LdapDirectoryIdentifier(this.Address, this.Port);
            var retval = new LdapConnection(id);
            retval.AuthType = this.AuthenticationType;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                // SSL is not supported on Linux atm.
                // Cf. https://github.com/dotnet/runtime/issues/43890
                retval.SessionOptions.SecureSocketLayer = this.IsSsl;
                retval.SessionOptions.VerifyServerCertificate
                    = (con, cert) => this.VerifyCertificate(cert, logger);
            }
            retval.SessionOptions.ProtocolVersion = this.ProtocolVersion;
            // Cf. https://stackoverflow.com/questions/10336553/system-directoryservices-protocols-paged-get-all-users-code-suddenly-stopped-get
            retval.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

            return retval;
        }

        /// <summary>
        /// Creates a new <see cref="LdapConnection"/> as configred in
        /// this pbject, connects to the configured server and binds
        /// using the specified credentials.
        /// </summary>
        /// <param name="username">The user name to logon with. Note that is
        /// both, <paramref name="username"/> and <paramref name="password"/>
        /// are <c>null</c>, the method tries to bind as the current user, which
        /// on Windows typically is the service or machine account the code is
        /// running as.</param>
        /// <param name="password">The password of the user. Note that is
        /// both, <paramref name="username"/> and <paramref name="password"/>
        /// are <c>null</c>, the method tries to bind as the current user, which
        /// on Windows typically is the service or machine account the code is
        /// running as.</param>
        /// <param name="defaultDomain">If not <c>null</c> or empty, add this
        /// domain to the user name if <paramref name="username"/> does not match
        /// the domain pattern.</param>
        /// <param name="logger">A logger to write messages to.</param>
        /// <returns>An LDAP connection to the configured server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c></exception>
        internal LdapConnection Connect(string username,
                string password,
                string defaultDomain,
                ILogger logger) {
            var retval = this.Connect(logger);
            Debug.Assert(this != null);
            Debug.Assert(logger != null);

            var rxUpn = new Regex(@".+@.+");
            if ((username != null)
                    && !string.IsNullOrWhiteSpace(defaultDomain)
                    && !rxUpn.IsMatch(username)) {
                username = $"{username}@{defaultDomain}";
            }

            if ((username == null) && (password == null)) {
                logger.LogInformation(Resources.InfoBindCurrent);
                retval.Bind();
                logger.LogInformation(Resources.InfoBoundCurrent);

            } else {
                logger.LogInformation(Resources.InfoBindingAsUser,
                    username);
                retval.Bind(new NetworkCredential(username, password));
                logger.LogInformation(Resources.InfoBoundAsUser, username);
            }

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
                logger.LogWarning("LDAP SSL certificate check has been "
                    + "disabled.");
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

            if (this.CertificateIssuer != null) {
                var issuer = this.CertificateIssuer;
                logger.LogInformation(Resources.InfoCheckCertIssuer,
                    certificate.Subject,
                    issuer);
                if (!string.Equals(certificate.Issuer, issuer,
                        StringComparison.InvariantCultureIgnoreCase)) {
                    logger.LogError(Resources.ErrorCertIssuerMismatch,
                        issuer,
                        certificate.Issuer);
                    return false;
                }
            }

            if (this.CertificateThumbprint?.Any() == true) {
                logger.LogInformation(Resources.InfoCheckCertThumbprint,
                    certificate.Subject,
                    string.Join(", ", this.CertificateThumbprint));

                var match = from t in this.CertificateThumbprint
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

        #region Private fields
        private int _pageSize;
        #endregion
    }
}
