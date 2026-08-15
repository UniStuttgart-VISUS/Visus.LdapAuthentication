// <copyright file="LdapMapping.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// Stores global LDAP mappings for a specific schema, which allows the
    /// system mostly to derive group memberships. All other per-user and
    /// per-group attributes callers might be interested in are configured
    /// via the user and group objects.
    /// </summary>
    public sealed class LdapMapping {

        /// <summary>
        /// Gets or sets the name of the attribute holding the distinguished
        /// name, which is used to obtain group objects.
        /// </summary>
        /// <remarks>
        /// This property defaults to &quot;distinguishedName&quot; and there
        /// should be little need to change this.
        /// </remarks>
        public string DistinguishedNameAttribute {
            get;
            set;
        } = "distinguishedName";

        /// <summary>
        /// Gets or sets the name of the attribute holding the members of a
        /// group.
        /// </summary>
        /// <remarks>
        /// The implementation expects this attribute to hold the DNs of the
        /// members of a group object.
        /// </remarks>
        public string GroupMemberAttribute {
            get;
            set;
        } = "member";

        /// <summary>
        /// Gets or sets the attribute where (non-primary) groups are stored.
        /// </summary>
        /// <remarks>
        /// <para>For an Active Directory, this is typically something like
        /// &quot;memberOf&quot;.</para>
        /// </remarks>
        public string GroupsAttribute { get; set; } = null!;

        /// <summary>
        /// Gets or sets the filter used to identify group entries in the
        /// directory.
        /// </summary>
        /// <remarks>
        /// For an Active Directory, this is typically something like
        /// &quot;(objectClass=group)&quot;
        /// </remarks>
        public string GroupsFilter { get; set; } = null!;

        /// <summary>
        /// Gets or sets the attribute where the primary group identity is
        /// stored.
        /// </summary>
        /// <remarks>
        /// <para>For an Active Directory, this is typically something like
        /// &quot;primaryGroupID&quot;</para>.
        /// </remarks>
        public string PrimaryGroupAttribute { get; set; } = null!;

        /// <summary>
        /// Gets or sets the attribute where the user or machine password is
        /// stored.
        /// </summary>
        /// <remarks>
        /// This attribute is required by the <see cref="ILdapPasswordService"/>
        /// and can be left blank for other services.
        /// </remarks>
        public string? PasswordAttribute { get; set; }

        /// <summary>
        /// Gets or sets the attribute where the identity of the primary group
        /// is stored.
        /// </summary>
        /// <remarks>
        /// This information is require to search for the directory entry of the
        /// primary group given we only know its ID. For both, Active Directory
        /// and OpenLDAP, primary groups are not stored using DNs, but by their
        /// ID, so we need to be able to perform a search for the ID in order
        /// the resolve the primary group. For Active Directory, this attribute
        /// is typically &quot;objectSid&quot;.
        /// </remarks>
        public string PrimaryGroupIdentityAttribute { get; set; } = null!;

        /// <summary>
        /// Gets or sets the filter to identify a single user by the user name in
        /// the authentication services.
        /// </summary>
        /// <remarks>
        /// <para>This must include a format string &qout;{0}&quot; where the
        /// user name should be inserted. For an Active Directory, this is
        /// typically something like
        /// &quot;(|(sAMAccountName={0})(userPrincipalName={0}))&quot;.</para>
        /// <para>Note that we cannot reflect on the account name of the
        /// user object in the authenication service, because as for the AD
        /// case above, we might have different attribute to match like
        /// the NTLM name (sAMAccountName) and the UPN. This cannot be achieved
        /// by the property mapping in the user object.</para>
        /// </remarks>
        public string UserFilter { get; set; } = null!;

        /// <summary>
        /// Gets or sets the filter used to identify user entries in the
        /// directory.
        /// </summary>
        /// <remarks>
        /// For an Active Directory, this is typically something like
        /// &quot;(&amp;(objectClass=user)(objectClass=person)(!(objectClass=computer)))&quot;
        /// </remarks>
        public string UsersFilter { get; set; } = null!;
    }
}
