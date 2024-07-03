// <copyright file="LdapServer.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;
using System.Runtime.InteropServices;


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
        [Required]
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

        #region Private fields
        private int _pageSize;
        #endregion
    }
}
