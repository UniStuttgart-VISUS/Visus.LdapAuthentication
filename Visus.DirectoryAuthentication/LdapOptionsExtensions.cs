// <copyright file="LdapOptionsExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Visus.DirectoryAuthentication.Properties;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Extension methods for <see cref="LdapOptions"/>.
    /// </summary>
    public static class LdapOptionsExtensions {

        /// <summary>
        /// Creates a new <see cref="LdapConnection"/> as configured in
        /// <paramref name="that"/> and connects to the configured server.
        /// </summary>
        /// <remarks>
        /// Note that this extension method does not bind to the server!
        /// </remarks>
        /// <param name="that">The LDAP configuration determining the properties
        /// of the connection and the server end point.</param>
        /// <param name="logger">A logger to write messages to.</param>
        /// <returns>An LDAP connection to the configured server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c></exception>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c></exception>
        public static LdapConnection Connect(this LdapOptions that,
                ILogger logger) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            var id = new LdapDirectoryIdentifier(that.Server, that.Port);
            var retval = new LdapConnection(id);
            retval.AuthType = that.AuthenticationType;
            retval.SessionOptions.SecureSocketLayer = that.IsSsl;
            retval.SessionOptions.ProtocolVersion = that.ProtocolVersion;
            retval.SessionOptions.VerifyServerCertificate
                = (con, cert) => that.VerifyServerCertificate(cert, logger);
            // Cf. https://stackoverflow.com/questions/10336553/system-directoryservices-protocols-paged-get-all-users-code-suddenly-stopped-get
            retval.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

            return retval;
        }

        /// <summary>
        /// Creates a new <see cref="LdapConnection"/> as configred in
        /// <paramref name="that"/>, connects to the configured server and binds
        /// using the specified credentials.
        /// </summary>
        /// <param name="that">The LDAP configuration determining the properties
        /// of the connection and the server end point.</param>
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
        /// <param name="authType">The supported authentication </param>
        /// <param name="logger">A logger to write messages to.</param>
        /// <returns>An LDAP connection to the configured server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c></exception>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c></exception>
        public static LdapConnection Connect(this LdapOptions that,
                string username, string password, ILogger logger) {
            var retval = that.Connect(logger);
            Debug.Assert(that != null);
            Debug.Assert(logger != null);

            var rxUpn = new Regex(@".+@.+");
            if ((username != null)
                    && !string.IsNullOrWhiteSpace(that.DefaultDomain)
                    && !rxUpn.IsMatch(username)) {
                username = $"{username}@{that.DefaultDomain}";
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
        /// <param name="that">The configuration according to which the
        /// verification should be performed.</param>
        /// <param name="certificate">The server certificate to be verified.
        /// </param>
        /// <param name="logger">A logger to write any problems and warnings
        /// to.</param>
        /// <returns><c>true</c> if the certificate is acceptable, <c>false</c>
        /// otherwise.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c></exception>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c></exception>
        public static bool VerifyServerCertificate(this LdapOptions that,
                X509Certificate certificate,
                ILogger logger) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            if (that.IsNoCertificateCheck) {
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

            if (that.ServerCertificateIssuer != null) {
                var issuer = that.ServerCertificateIssuer;
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

            if (that.ServerThumbprint?.Any() == true) {
                logger.LogInformation(Resources.InfoCheckCertThumbprint,
                    certificate.Subject,
                    string.Join(", ", that.ServerThumbprint));

                var match = from t in that.ServerThumbprint
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
    }
}
