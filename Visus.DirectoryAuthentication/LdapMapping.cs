// <copyright file="LdapMapping.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;


namespace Visus.DirectoryAuthentication {

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

        #region Private fields
        private string[] _requiredGroupAttributes;
        #endregion
    }
}
