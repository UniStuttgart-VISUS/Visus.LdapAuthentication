// <copyright file="LdapOptions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2023 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Linq;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Stores the configuration options for the LDAP server.
    /// </summary>
    public sealed class LdapOptions : IOptions {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        public LdapOptions() {
            this.PageSize = 1000;   // Reasonable default for AD.
#pragma warning disable CS0618
            this.IsSubtree = true;  // Backward compatibility.
#pragma warning restore CS0618
        }
        #endregion

        #region Public properties
        /// <inheritdoc />
        public string DefaultDomain { get; set; }

        /// <inheritdoc />
        public bool IsNoCertificateCheck { get; set; }

        /// <summary>
        /// Gets whether a legacy single search base has been configured.
        /// </summary>
        public bool IsLegacySearchBase => !string.IsNullOrEmpty(
            this._searchBase);

        /// <inheritdoc />
        public bool IsRecursiveGroupMembership { get; set; }

        /// <inheritdoc />
        public bool IsSsl { get; set; }

        /// <summary>
        /// Gets or sets the legacy search scope setting.
        /// </summary>
        [Obsolete("Use SearchBases instead.")]
        public bool IsSubtree {
            get => (this._searchScope == SearchScope.Subtree);
            set => this._searchScope = value
                ? SearchScope.Subtree
                : SearchScope.Base;
        }

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
                        UserFilter = "(&(objectClass=posixAccount)(entryDN={0}))",
                        UsersFilter = "(&(objectClass=posixAccount)(objectClass=person))"
                    }
                }
            };

        /// <inheritdoc />
        public int PageSize {
            get => this._pageSize;
            set => this._pageSize = Math.Max(0, value);
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
        [Obsolete("Use SearchBases instead.")]
        public string SearchBase {
            get => this._searchBase;
            set => this._searchBase = value;
        }

        /// <inheritdoc />
        public IDictionary<string, SearchScope> SearchBases {
            get {
                // This wild construct is for making sure that all deployments
                // that have appsettings using the single SearchBase continue
                // to work and new deployments can use multip SearchBases by
                // setting the new property.
                var haveCurrent = (this._searchBases?.Count > 0);
                var haveLegacy = this.IsLegacySearchBase;

                if (haveCurrent) {
                    return this._searchBases;

                } else if (haveLegacy) {
                    return new Dictionary<string, SearchScope>() {
                        { this._searchBase, this._searchScope }
                    };

                } else {
                    return new Dictionary<string, SearchScope>();
                }
            }
            set {
                // This is a wild hack that prevents the single SearchBase form
                // being applied to the array during bind. Unfortunately,
                // ASP.NET Core iterates over all bindable properties, retrieves
                // their default value and re-assigns it, which would break the
                // getter implementation above.
                if ((value?.Count != 1)
                        || (value.Keys?.First() != this._searchBase)) {
                    this._searchBases = value;
                }
            }
        }

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
        private string _searchBase;
        private IDictionary<string, SearchScope> _searchBases;
        private SearchScope _searchScope;
        private int _timeout;
        #endregion
    }
}
