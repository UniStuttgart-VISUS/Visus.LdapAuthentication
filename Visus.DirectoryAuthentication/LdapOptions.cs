// <copyright file="LdapOptions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Stores the configuration options for the LDAP server.
    /// </summary>
    public sealed class LdapOptions : ILdapOptions {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        public LdapOptions() {
            this.PageSize = 1000;   // Reasonable default for AD.
        }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public string DefaultDomain { get; set; }

        /// <inheritdoc />
        public bool IsNoCertificateCheck { get; set; }

        /// <inheritdoc />
        public bool IsRecursiveGroupMembership { get; set; }

        /// <inheritdoc />
        public bool IsSsl { get; set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
                        UserFilter = "(&(objectClass=posixAccount)(uid={0}))",
                        UsersFilter = "(&(objectClass=posixAccount)(objectClass=person))"
                    }
                }
            };

        /// <inheritdoc />
        public int PageSize {
            get => this._pageSize;
            set => this._pageSize = Math.Max(1, value);
        }

        /// <inheritdoc />
        public string Password { get; set; }

        /// <inheritdoc />
        public int Port { get; set; } = 389;

        /// <inheritdoc />
        public int ProtocolVersion { get; set; } = 3;

        /// <inheritdoc />
        public string Schema { get; set; }

        /// <inheritdoc />
        public IDictionary<string, SearchScope> SearchBase {
            get;
            set;
        } = new Dictionary<string, SearchScope>();

        /// <inheritdoc />
        public string Server { get; set; }

        /// <inheritdoc />
        public string ServerCertificateIssuer { get; set; }

        /// <inheritdoc />
        public string[] ServerThumbprint {
            get;
            set;
        } = Array.Empty<string>();

        /// <inheritdoc />
        public TimeSpan Timeout {
            get => this._timeout;
            set => this._timeout = (value < TimeSpan.Zero)
                ? TimeSpan.Zero
                : value;
        }

        /// <inheritdoc />
        public string User { get; set; }
        #endregion

        #region Private fields
        private LdapMapping _mapping;
        private int _pageSize;
        private TimeSpan _timeout;
        #endregion
    }
}
