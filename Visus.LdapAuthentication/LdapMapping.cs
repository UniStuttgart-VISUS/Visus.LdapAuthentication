// <copyright file="LdapMapping.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Stores global LDAP mappings for a specific schema.
    /// </summary>
    public sealed class LdapMapping {

        /// <summary>
        /// Gets or sets the name of the attribute where the distinguished
        /// name of an object is stored.
        /// </summary>
        public string DistinguishedNameAttribute { get; set; }

        /// <summary>
        /// Gets or sets the attribute where (non-primary) groups are stored.
        /// </summary>
        /// <remarks>
        /// <para>For an Active Directory, this is typically something like
        /// &quot;GroupAttribute&quot;.</para>
        /// </remarks>
        public string GroupsAttribute { get; set; }

        /// <summary>
        /// Gets or sets the name of the attribute where the unique identity of
        /// a group is stored.
        /// </summary>
        /// <remarks>
        /// <para>For an Active Directory, this is typically the SID stored in
        /// &quot;objectSid&quot;.</para>
        /// </remarks>
        public string GroupIdentityAttribute { get; set; }

        /// <summary>
        /// Getr or sets the type name of a value converter that is used to
        /// convert the <see cref="GroupIdentityAttribute"/>.
        /// </summary>
        public string GroupIdentityConverter { get; set; }

        /// <summary>
        /// Gets or sets the attribute where the primary group identity is
        /// stored.
        /// </summary>
        /// <remarks>
        /// <para>For an Active Directory, this is typically something like
        /// &quot;primaryGroupID&quot;</para>.
        /// </remarks>
        public string PrimaryGroupAttribute { get; set; }

        /// <summary>
        /// Gets or sets the attributes that are required to retrieve
        /// (transitive) group memberships.
        /// </summary>
        public string[] RequiredGroupAttributes {
            get => this._requiredGroupAttributes ?? new[] {
                this.DistinguishedNameAttribute,
                this.GroupIdentityAttribute,
                this.GroupsAttribute,
                this.PrimaryGroupAttribute
            };
            set => this._requiredGroupAttributes = value;
        }

        /// <summary>
        /// Gets or sets the filter to identify a single user by the user name.
        /// </summary>
        /// <remarks>
        /// <para>This must include a format string &qout;{0}&quot; where the
        /// user name should be inserted.</para>
        /// <para>For an Active Directory, this is typically something like
        /// &quot;(sAMAccountName={0})&quot;</para>
        /// </remarks>
        public string UserFilter { get; set; }

        /// <summary>
        /// Gets or sets the filter used to identify user entries in directory
        /// </summary>
        /// <remarks>
        /// For an Active Directory, this is typically something like
        /// &quot;(&amp;(objectClass=user)(objectClass=person)(!(objectClass=computer)))&quot;
        /// </remarks>
        public string UsersFilter { get; set; }

        #region Public methods
        /// <summary>
        /// Gets an instance of <see cref="GroupIdentityConverter"/>.
        /// </summary>
        /// <returns>The converter if a valid one was configured, <c>null</c>
        /// otherwise.</returns>
        internal ILdapAttributeConverter GetGroupIdentityConverter() {
            try {
                var t = Type.GetType(this.GroupIdentityConverter);
                return Activator.CreateInstance(t) as ILdapAttributeConverter;
            } catch {
                return null;
            }
        }
        #endregion

        #region Private fields
        private string[] _requiredGroupAttributes;
        #endregion
    }
}
