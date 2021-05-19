// <copyright file="ILdapOptions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Visus.LdapAuthentication {

    /// <summary>
    /// Extension methods for <see cref="ILdapOptions"/>.
    /// </summary>
    public static class LdapOptionsExtensions {

        /// <summary>
        /// Creates an <see cref="LdapConnection"/> as configured in
        /// <paramref name="that"/>
        /// </summary>
        /// <remarks>
        /// <para>The connection created by this method has registered a server
        /// certification validation callback that checks the policy configured
        /// in <paramref name="that"/>.</para>
        /// </remarks>
        /// <param name="that">The LDAP configuration determining the properties
        /// of the connection.</param>
        /// <param name="logger">A logger to write certificate verification
        /// issues to.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c></exception>
        /// <exception cref="ArgumentNullException">If <paramref name="logger"/>
        /// is <c>null</c></exception>
        public static LdapConnection CreateConnection(this ILdapOptions that,
                ILogger logger) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            using var retval = new LdapConnection() {
                SecureSocketLayer = that.IsSsl,
            };

            retval.UserDefinedServerCertValidationDelegate += (s, c, a, e)
                => that.VerifyServerCertificate(c, a, e, logger);

            return retval;
        }

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
            var retval = that.CreateConnection(logger);
            Debug.Assert(that != null);
            Debug.Assert(logger != null);

            logger.LogInformation(Properties.Resources.InfoConnectingLdap,
                that.Server, that.Port);
            retval.Connect(that.Server, that.Port);

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

            logger.LogInformation(Properties.Resources.InfoBindingAsUser,
                username);
            retval.Bind(username, password);
            logger.LogInformation(Properties.Resources.InfoBoundAsUser,
                username);

            return retval;
        }

        /// <summary>
        /// Converts the <see cref="ILdapOptions.IsSubtree"/> property of
        /// <paramref name="that"/> into a scope constant from Novell LDAP.
        /// </summary>
        /// <param name="that">The options to get the search scope for.</param>
        /// <returns><see cref="LdapConnection.ScopeSub"/> if
        /// <see cref="ILdapOptions.IsSubtree"/> is <c>true</c>,
        /// <see cref="LdapConnection.ScopeBase"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c></exception>
        public static int GetSearchScope(this ILdapOptions that) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            return that.IsSubtree
                ? LdapConnection.ScopeSub
                : LdapConnection.ScopeBase;
        }

        /// <summary>
        /// Performs verification of the server certificate.
        /// </summary>
        /// <param name="that">The configuration according to which the
        /// verification should be performed.</param>
        /// <param name="certificate">The server certificate to be verified.
        /// </param>
        /// <param name="chain">The certificate chain.</param>
        /// <param name="sslPolicyErrors">Any policy errors that have been
        /// discovered.</param>
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
                X509Chain chain,
                SslPolicyErrors sslPolicyErrors,
                ILogger logger) {
            // See https://stackoverflow.com/questions/386982/novell-ldap-c-sharp-novell-directory-ldap-has-anybody-made-it-work
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = logger ?? throw new ArgumentNullException(nameof(logger));

            if (that.IsNoCertificateCheck) {
                logger.LogWarning("LDAP SSL certificate check has been "
                    + "disabled.");
                return true;
            }

            if (sslPolicyErrors != SslPolicyErrors.None) {
                logger.LogInformation("LDAP SSL policy errors are: ",
                    sslPolicyErrors);
                return false;
            }

            if (that.RootCaThumbprint != null) {
                var thumbprint = that.RootCaThumbprint;
                logger.LogInformation("Checking for LDAP server "
                    + "certificate \"{0}\" being issued by derived from a root "
                    + "CA with thumbprint \"{1}\".", certificate.Subject,
                    thumbprint);

                var ca = chain.ChainElements.Cast<X509ChainElement>().Last();
                if (!string.Equals(ca.Certificate.Thumbprint, thumbprint,
                        StringComparison.InvariantCultureIgnoreCase)) {
                    return false;
                }
            }

            if ((that.ServerThumbprint != null) && that.ServerThumbprint.Any()) {
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
