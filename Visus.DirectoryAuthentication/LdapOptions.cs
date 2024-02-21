// <copyright file="LdapOptions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Runtime.InteropServices;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Stores the configuration options for the LDAP server.
    /// </summary>
    public sealed class LdapOptions {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        public LdapOptions() {
            this.AuthenticationType = 
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? AuthType.Negotiate
                : AuthType.Basic;
            this.PageSize = 1000;   // Reasonable default for AD.
        }
        #endregion

        #region Public properties
        /// <summary>
        /// The authentication type used to bind to the LDAP server.
        /// </summary>
        public AuthType AuthenticationType { get; set; }

        /// <summary>
        /// Gets the default domain appended to a user name.
        /// </summary>
        /// <remarks>
        /// Certain LDAP servers (like AD) might require the UPN instead of the
        /// account name to be used for binding. If this property is set, the
        /// <see cref="ILdapAuthenticationService.Login(string, string)"/>
        /// method will check for plain account names and append this domain
        /// to the user name. This will allow users to logon with their short
        /// account name.
        /// </remarks>
        public string DefaultDomain { get; set; }

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
        /// Gets whether group memberships should be looked up recursively.
        /// </summary>
        /// <remarks>
        /// <para>If you can express your login policies using only the primary
        /// group and any direct group memberships of the user accounts, you
        /// can and should disable this flag. Enabling the flag causes the login
        /// process to recursively accumulate all transitive group memberships,
        /// which causes a significant increase in LDAP queries that need to be
        /// performed for each login.</para>
        /// </remarks>
        public bool IsRecursiveGroupMembership { get; set; }

        /// <summary>
        /// Get whether the LDAP connection uses SSL or not.
        /// </summary>
        public bool IsSsl { get; set; }

        /// <summary>
        /// Gets the global LDAP mapping for the selected schema.
        /// </summary>
        /// <remarks>
        /// <para>This property mostly specifies the behaviour of the group
        /// whereas the attribute mapping of the user is specified via
        /// <see cref="ILdapUser.RequiredAttributes"/></para>.
        /// </remarks>
        public LdapMapping Mapping {
            get {
                var retval = this._mapping;

                if ((retval == null) && (this.Schema != null)) {
                    this.Mappings.TryGetValue(this.Schema, out retval);
                }

                return retval;
            }
            set => this._mapping = value;
        }

        /// <summary>
        /// Gets the per-schema global LDAP mappings, which are used to retrieve
        /// group memberships etc.
        /// </summary>
        /// <remarks>
        /// <para>This property is mostly used to specify built-in mappings. If
        /// you want to provide a custom mapping, it suffices overriding the
        /// <see cref="Mapping"/> property</para>.
        /// </remarks>
        public Dictionary<string, LdapMapping> Mappings { get; set; }
            = new Dictionary<string, LdapMapping>() {
                {
                    DirectoryAuthentication.Schema.ActiveDirectory,
                    new LdapMapping() {
                        DistinguishedNameAttribute = "distinguishedName",
                        GroupIdentityAttribute = "objectSid",
                        GroupIdentityConverter = typeof(SidConverter).FullName,
                        GroupsAttribute = "memberOf",
                        PrimaryGroupAttribute = "primaryGroupID",
                        UserFilter = "(|(sAMAccountName={0})(userPrincipalName={0}))",
                        UsersFilter = "(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))"
                    }
                },
                {
                    DirectoryAuthentication.Schema.IdentityManagementForUnix,
                    new LdapMapping() {
                        DistinguishedNameAttribute = "distinguishedName",
                        GroupIdentityAttribute = "gidNumber",
                        GroupsAttribute = "memberOf",
                        PrimaryGroupAttribute = "gidNumber",
                        UserFilter = "(|(sAMAccountName={0})(userPrincipalName={0}))",
                        UsersFilter = "(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))"
                    }
                },
                {
                    DirectoryAuthentication.Schema.Rfc2307,
                    new LdapMapping() {
                        DistinguishedNameAttribute = "distinguishedName",
                        GroupIdentityAttribute = "gidNumber",
                        GroupsAttribute = "memberOf",
                        PrimaryGroupAttribute = "gidNumber",
                        UserFilter = "(&(objectClass=posixAccount)(entryDN={0}))",
                        UsersFilter = "(&(objectClass=posixAccount)(objectClass=person))"
                    }
                }
            };

        /// <summary>
        /// Gets the maximum number of results the LDAP client should request.
        /// </summary>
        /// <remarks>
        /// <para>This is currently not used.</para>
        /// </remarks>
        public int PageSize {
            get => this._pageSize;
            set => this._pageSize = Math.Max(1, value);
        }

        /// <summary>
        /// Gets the password used to connect to the LDAP server.
        /// </summary>
        /// <remarks>
        /// <para>This password is only used when performing additional
        /// queries using <see cref="ILdapConnectionService"/>. All login
        /// requests in <see cref="ILdapAuthenticationService"/> are performed
        /// by binding the user that is trying to log in. The user therefore
        /// must be able to read his/her own LDAP entry and the groups in
        /// order to fill <see cref="ILdapUser"/>.</para>
        /// </remarks>
        public string Password { get; set; }

        /// <summary>
        /// Gets the port of the LDAP server.
        /// </summary>
        public int Port { get; set; } = 389;

        /// <summary>
        /// Gets the version of the LDAP protocol to request from the server.
        /// </summary>
        public int ProtocolVersion { get; set; } = 3;

        /// <summary>
        /// Gets the name of the LDAP schema which is used by
        /// <see cref="LdapUserBase"/> to automatically retrieve the required
        /// properties.
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Gets the starting point(s) of any directory search.
        /// </summary>
        public IDictionary<string, SearchScope> SearchBase {
            get;
            set;
        } = new Dictionary<string, SearchScope>();

        /// <summary>
        /// Gets the host name or IP of the LDAP server.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Gets the acceptable issuer of the server certificate.
        /// </summary>
        /// <remarks>
        /// <para>If this property is <c>null</c>, any issuer will be considered
        /// acceptable.</para>
        /// <para>This property is only relevant if <see cref="UseSsl"/> is
        /// enabled.</para>
        /// </remarks>
        public string ServerCertificateIssuer { get; set; }

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
        public string[] ServerThumbprint {
            get;
            set;
        } = Array.Empty<string>();

        /// <summary>
        /// Gets the timeout for LDAP queries.
        /// </summary>
        /// <remarks>
        /// <para>This is currently not used.</para>
        /// <para>A value of <see cref="TimeSpan.Zero"/> indicates an infinite
        /// timeout.</para></remarks>
        public TimeSpan Timeout {
            get => this._timeout;
            set => this._timeout = (value < TimeSpan.Zero)
                ? TimeSpan.Zero
                : value;
        }

        /// <summary>
        /// Gets the LDAP user used to connect to the directory.
        /// </summary>
        /// <remarks>
        /// This is only used by <see cref="ILdapConnectionService"/>. See
        /// <see cref="Password"/> for more details.
        /// </remarks>
        public string User { get; set; }
        #endregion

        #region Private fields
        private LdapMapping _mapping;
        private int _pageSize;
        private TimeSpan _timeout;
        #endregion
    }
}
