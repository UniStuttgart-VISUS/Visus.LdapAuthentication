// <copyright file="LdapOptionsBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Visus.Ldap.Mapping;
using Schemas = Visus.Ldap.Mapping.Schema;


namespace Visus.Ldap.Configuration {

    /// <summary>
    /// Base class storing all shared properties used by the two libraries to
    /// make connections to an LDAP server.
    /// </summary>
    public abstract class LdapOptionsBase {

        #region Public constants
        /// <summary>
        /// The suggested name of the LDAP configuration section.
        /// </summary>
        public const string SectionName = "LdapOptions";
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the default domain appended to a user name.
        /// </summary>
        /// <remarks>
        /// Certain LDAP servers (like AD) might require the UPN instead of the
        /// account name to be used for binding. If this property is set, the
        /// connection services can append this information to the user name as
        /// necessary and therefore take the burden from the user of knowing
        /// what the correct format is. If the property is <c>null</c>, user
        /// names will be passed to the server as they are.
        /// </remarks>
        public string? DefaultDomain { get; set; }

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
        /// Gets the global LDAP mapping for the selected schema.
        /// </summary>
        /// <remarks>
        /// <para>This property mostly specifies the behaviour of the group
        /// retrieval whereas the assignment of user and group properties is
        /// determined by the user and group objects or the registered mapper.
        /// </para>
        /// <para>If this property has not been set, the method tries to retrieve
        /// the instance in <see cref="Mappings"/> that matches the configured
        /// <see cref="Schema"/>.
        /// </para>
        /// </remarks>
        public LdapMapping? Mapping {
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
                    Schemas.ActiveDirectory,
                    new LdapMapping() {
                        GroupsAttribute = "memberOf",
                        PrimaryGroupAttribute = "primaryGroupID",
                        PrimaryGroupIdentityAttribute = "objectSid",
                        UserFilter = "(|(sAMAccountName={0})(userPrincipalName={0}))",
                        UsersFilter = "(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))"
                    }
                },
                {
                    Schemas.IdentityManagementForUnix,
                    new LdapMapping() {
                        GroupsAttribute = "memberOf",
                        PrimaryGroupAttribute = "gidNumber",
                        PrimaryGroupIdentityAttribute = "gidNumber",
                        UserFilter = "(|(sAMAccountName={0})(userPrincipalName={0}))",
                        UsersFilter = "(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))"
                    }
                },
                {
                    Schemas.Rfc2307,
                    new LdapMapping() {
                        GroupsAttribute = "memberOf",
                        PrimaryGroupAttribute = "gidNumber",
                        PrimaryGroupIdentityAttribute = "gidNumber",
                        UserFilter = "(&(objectClass=posixAccount)(entryDN={0}))",
                        UsersFilter = "(&(objectClass=posixAccount)(objectClass=person))"
                    }
                }
            };

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
        /// Gets the password used to connect to the LDAP server.
        /// </summary>
        /// <remarks>
        /// <para>This password is used when performing queries using a service
        /// account. All login requests in are performed by binding the user
        /// that is trying to log in. The user therefore must be able to read
        /// his own LDAP entry and the groups in order to fill the user object
        /// the API will return to the caller.</para>
        /// <para>The password can only be <c>null</c> if the <see cref="User"/>
        /// is <c>null</c> as well.</para>
        /// </remarks>
        public string? Password { get; set; }

        /// <summary>
        /// Gets the port of the LDAP server.
        /// </summary>
        public int Port { get; set; } = 389;

        /// <summary>
        /// Gets the version of the LDAP protocol to request from the server.
        /// </summary>
        public int ProtocolVersion { get; set; } = 3;

        /// <summary>
        /// Gets or sets the claim to be created for the identity of the primary
        /// group of a user.
        /// </summary>
        public string? PrimaryGroupIdentityClaim {
            get;
            set;
        } = ClaimTypes.PrimaryGroupSid;

        /// <summary>
        /// Gets the name of the LDAP schema which is used by
        /// <see cref="LdapUserBase"/> to automatically retrieve the required
        /// properties.
        /// </summary>
        public string Schema { get; set; } = null!;

        ///// <summary>
        ///// Gets the starting point(s) of any directory search.
        ///// </summary>
        //public IDictionary<string, SearchScope> SearchBases {
        //    get;
        //    set;
        //} = new Dictionary<string, SearchScope>();

        /// <summary>
        /// Gets or sets the servers to connect to.
        /// </summary>
        public string[] Servers { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets the acceptable issuer of the server certificate.
        /// </summary>
        /// <remarks>
        /// <para>If this property is <c>null</c>, which is the default, any
        /// issuer will be considered acceptable.</para>
        /// <para>This property is only relevant if
        /// <see cref="TransportSecurity"/> is enabled.</para>
        /// </remarks>
        public string? ServerCertificateIssuer { get; set; }

        /// <summary>
        /// Gets the certificate thumbprints for the LDAP servers that are
        /// accepted during certificate validation.
        /// </summary>
        /// <remarks>
        /// <para>If this array is empty, any server certificate will be
        /// accepted. Note that if <see cref="ServerCertificateIssuer"/> is set
        /// as well, the server certificate must have been issued by the
        /// specified issuer, too.</para>
        /// <para>This property is only relevant if
        /// <see cref="TransportSecurity"/> is enabled.</para>
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
        /// Gets or sets the transport-level encryption that should be used.
        /// </summary>
        public TransportSecurity TransportSecurity {
            get;
            set;
        } = TransportSecurity.None;

        /// <summary>
        /// Gets the LDAP user used to connect to the directory.
        /// </summary>
        /// <remarks>
        /// This is only used by <see cref="ILdapConnectionService"/>. See
        /// <see cref="Password"/> for more details.
        /// </remarks>
        public string? User { get; set; }
        #endregion

        #region Private fields
        private LdapMapping? _mapping;
        private int _pageSize = 1000;
        private TimeSpan _timeout;
        #endregion
    }
}
