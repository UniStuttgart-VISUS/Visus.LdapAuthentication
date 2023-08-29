// <copyright file="LdapOptionsExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2023 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Extension methods for <see cref="ILdapOptions"/>.
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
        public static LdapConnection Connect(this ILdapOptions that,
                ILogger logger) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            var id = new LdapDirectoryIdentifier(that.Server, that.Port);
            var retval = new LdapConnection(id);
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
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="logger">A logger to write messages to.</param>
        /// <returns>An LDAP connection to the configured server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c></exception>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c></exception>
        public static LdapConnection Connect(this ILdapOptions that,
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                retval.AuthType = AuthType.Negotiate;
            } else {
                retval.AuthType = AuthType.Basic;
            }

            if (that.Password == null) {
                logger.LogInformation(Properties.Resources.InfoBindCurrent);
                retval.Bind();
            } else {
                logger.LogInformation(Properties.Resources.InfoBindingAsUser,
                    username);
                retval.Bind(new NetworkCredential(that.User, that.Password));
            }

            logger.LogInformation(Properties.Resources.InfoBoundAsUser,
                username);

            return retval;
        }

        /// <summary>
        /// Converts the <see cref="ILdapOptions.IsSubtree"/> property of
        /// <paramref name="that"/> into a <see cref="SearchScope"/>.
        /// </summary>
        /// <param name="that">The options to get the search scope for.</param>
        /// <returns><see cref="SearchScope.Subtree"/> if
        /// <see cref="ILdapOptions.IsSubtree"/> is <c>true</c>,
        /// <see cref="SearchScope.Base"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c></exception>
        public static SearchScope GetSearchScope(this ILdapOptions that) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            return that.IsSubtree ? SearchScope.Subtree : SearchScope.Base;
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
        public static bool VerifyServerCertificate(this ILdapOptions that,
                X509Certificate certificate,
                ILogger logger) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            if (that.IsNoCertificateCheck) {
                logger.LogWarning("LDAP SSL certificate check has been "
                    + "disabled.");
                return true;
            }

            if (that.ServerCertificateIssuer != null) {
                var issuer = that.ServerCertificateIssuer;
                logger.LogInformation("Checking for LDAP server "
                    + "certificate \"{0}\" being issued by \"{1}\".",
                    certificate.Subject, issuer);
                if (!string.Equals(certificate.Issuer, issuer,
                        StringComparison.InvariantCultureIgnoreCase)) {
                    return false;
                }
            }

            if (that.ServerThumbprint?.Any() == true) {
                logger.LogInformation("Checking that LDAP server "
                    + "certificate \"{0}\" has one of the following "
                    + "thumbprints: {1}", certificate.Subject,
                    string.Join(", ", that.ServerThumbprint));

                var match = from t in that.ServerThumbprint
                            where string.Equals(t, certificate.GetCertHashString(),
                                StringComparison.InvariantCultureIgnoreCase)
                            select t;

                if (!match.Any()) {
                    return false;
                }
            }

            return true;
        }
    }
}
