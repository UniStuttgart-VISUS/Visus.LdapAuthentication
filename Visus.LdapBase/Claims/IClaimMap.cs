// <copyright file="IClaimMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;
using Visus.Ldap.Mapping;
using ClaimMapping = System.Collections.Generic.KeyValuePair<
    Visus.Ldap.Mapping.LdapAttributeAttribute,
    System.Collections.Generic.IEnumerable<Visus.Ldap.Claims.ClaimAttribute>>;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// Provides mapping information for mapping the values of
    /// <see cref="LdapAttributeAttribute"/> to <see cref="ClaimAttribute"/>s.
    /// </summary>
    /// <typeparam name="TObject">The object to be reflected.</typeparam>
    public interface IClaimMap : IEnumerable<ClaimMapping> {

        /// <summary>
        /// Gets the distinct names of all <see cref="Attributes"/>.
        /// </summary>
        IEnumerable<string> AttributeNames { get; }

        /// <summary>
        /// Gets all LDAP attributes that generate at least one claim.
        /// </summary>
        IEnumerable<LdapAttributeAttribute> Attributes { get; }

        /// <summary>
        /// Gets, if any, the <see cref="ClaimAttribute"/>s for the given
        /// <see cref="LdapAttributeAttribute"/>.
        /// </summary>
        /// <param name="attribute">The LDAP attributes to get the
        /// claims for.</param>
        /// <returns>The list of cliams the attribute is mapped to or
        /// an empty enumeration if the attribute does not represent any
        /// claim.</returns>
        IEnumerable<ClaimAttribute> this[LdapAttributeAttribute attribute] {
            get;
        }
    }
}
