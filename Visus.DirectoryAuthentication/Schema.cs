// <copyright file="Schema.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 -2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.DirectoryAuthentication {

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
        /// Idenfities an IETF RFC 2307 schema (&quot;An Approach for Using LDAP
        /// as a Network Information Service&quot;).
        /// </summary>
        /// <remarks>
        /// This schema has been created from the RFC and has not been tested by
        /// the authors.
        /// </remarks>
        public const string Rfc2307 = "RFC 2307";
    }
}
