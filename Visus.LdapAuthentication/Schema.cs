// <copyright file="Schema.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Lists well-known LDAP schemas.
    /// </summary>
    public static class Schema {

        /// <summary>
        /// Identifies an Active Directory schema.
        /// </summary>
        public const string ActiveDirectory = "Active Directory";

        /// <summary>
        /// Identifies an Active Directory schema with Identity Management for
        /// Unix installed and where the Unix attributes are used instead of
        /// SIDs.
        /// </summary>
        public const string IdentityManagementForUnix = "IDMU";

        /// <summary>
        /// Identifies an OpenLDAP schema.
        /// </summary>
        public const string OpenLdap = "OpenLDAP";

        /// <summary>
        /// Idenfities an IETF RFC 2256 schema.
        /// </summary>
        public const string Rfc2256 = "RFC 2256";
    }
}
