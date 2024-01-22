// <copyright file="LdapIdentityRoleBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Visus.DirectoryAuthentication;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// The base class for a role object, which provides the default mappings
    /// for ADDS.
    /// </summary>
    /// <remarks>
    /// This class can be used as base class for custom roles that need
    /// additional properties from the directory. Using this annotated base
    /// makes shure that the store can find the necessary LDAP properties.
    /// It is, however, possible to create a all-new class and annotating
    /// it similarly with <see cref="LdapAttributeAttribute"/>s.
    /// </remarks>
    public abstract class LdapIdentityRoleBase {

        /// <summary>
        /// Gets the claims of the role.
        /// </summary>
        public virtual IEnumerable<Claim> Claims { get; internal set; }
            = Enumerable.Empty<Claim>();

        /// <summary>
        /// Gets the name of the group that represents the role.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "sAMAccountName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sAMAccountName")]
        [LdapAttribute(Schema.Rfc2307, "gid")]
        [Claim(ClaimTypes.Name)]
        [Claim(ClaimTypes.WindowsAccountName)]
        public virtual string Name { get; internal set; }

        /// <summary>
        /// Gets the unique identitfier of the role, which is equivalent to the
        /// unqiue ID of the group in this implementation.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "objectSid",
            Converter = typeof(SidConverter))]
        [LdapAttribute(Schema.IdentityManagementForUnix, "gidNumber")]
        [LdapAttribute(Schema.Rfc2307, "gidNumber")]
        [Claim(ClaimTypes.PrimarySid)]
        [Claim(ClaimTypes.Sid)]
        [Claim(ClaimTypes.NameIdentifier)]
        public virtual string ID { get; internal set; }
    }
}
