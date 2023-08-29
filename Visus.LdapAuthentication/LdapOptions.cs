// <copyright file="LdapOptions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;


namespace Visus.LdapAuthentication {

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
        public bool IsSubtree { get; set; } = true;

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
                    LdapAuthentication.Schema.ActiveDirectory,
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
                    LdapAuthentication.Schema.IdentityManagementForUnix,
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
                    LdapAuthentication.Schema.Rfc2307,
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
        public string RootCaThumbprint { get; set; }

        /// <inheritdoc />
        public string Schema { get; set; }

        /// <inheritdoc />
        public string SearchBase { get; set; }

        /// <inheritdoc />
        public string Server { get; set; }

        /// <inheritdoc />
        public string[] ServerThumbprint {
            get;
            set;
        } = Array.Empty<string>();

        /// <inheritdoc />
        public int Timeout {
            get => this._timeout;
            set => this._timeout = Math.Max(0, value);
        }

        /// <inheritdoc />
        public string User { get; set; }
        #endregion

        #region Private fields
        private LdapMapping _mapping;
        private int _pageSize;
        private int _timeout;
        #endregion
    }
}
