// <copyright file="IClaimsMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using Visus.Ldap.Mapping;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// The interface of a builder class that allows for fluently building
    /// mappings of LDAP attributes to
    /// <see cref="System.Security.Claims.Claim"/>s.
    /// </summary>
    public interface IClaimsMapBuilder {

        /// <summary>
        /// Gets a builder for mapping claims to the given
        /// <paramref name="attribute"/>.
        /// </summary>
        /// <param name="attribute">The LDAP attribute to map the claims to.
        /// </param>
        /// <returns><c>this</c>.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="attribute"/> if <c>null</c>.</exception>
        IAttributeClaimsMapBuilder MapAttribute(
            LdapAttributeAttribute attribute);

        /// <summary>
        /// Gets a builder for mapping claims to the LDAP attribute with the
        /// given <paramref name="attributeName"/>.
        /// </summary>
        /// <param name="attributeName">The name of the attribute to map.
        /// </param>
        /// <returns><c>this</c>.</returns>
        /// <exception cref="ArgumentException">If
        /// <paramref name="attributeName"/> is <c>null</c> or empty.
        /// </exception>
        INewAttributeClaimsMapBuilder MapAttribute(string attributeName);

        /// <summary>
        /// Gets a builder for mapping claims to the property named
        /// <paramref name="propertyName"/> of the type that is being mapped.
        /// </summary>
        /// <param name="propertyName">The name of the property to map.</param>
        /// <returns><c>this</c>.</returns>
        /// <exception cref="ArgumentException">If the specified property
        /// does not exist in the mapped type.</exception>
        IAttributeClaimsMapBuilder MapProperty(string propertyName);
    }
}
