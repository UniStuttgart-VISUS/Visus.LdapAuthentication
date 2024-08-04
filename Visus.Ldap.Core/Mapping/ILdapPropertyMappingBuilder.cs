// <copyright file="ILdapPropertyMappingBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// Represents the mapping configuration of a single property.
    /// </summary>
    public interface ILdapPropertyMappingBuilder {

        /// <summary>
        /// Sets the current property as
        /// <see cref="ILdapAttributeMap{TObject}.AccountNameProperty"/>.
        /// </summary>
        /// <returns><c>this</c>.</returns>
        ILdapPropertyMappingBuilder StoringAccountName();

        /// <summary>
        /// Sets the current property as
        /// <see cref="ILdapAttributeMap{TObject}.DistinguishedNameProperty"/>.
        /// </summary>
        /// <returns><c>this</c>.</returns>
        ILdapPropertyMappingBuilder StoringDistinguishedName();

        /// <summary>
        /// Sets the current property as
        /// <see cref="ILdapAttributeMap{TObject}.GroupMembershipsProperty"/>.
        /// </summary>
        /// <returns><c>this</c>.</returns>
        ILdapPropertyMappingBuilder StoringGroupMemberships();

        /// <summary>
        /// Sets the current property as
        /// <see cref="ILdapAttributeMap{TObject}.IdentityProperty"/>.
        /// </summary>
        /// <returns><c>this</c>.</returns>
        ILdapPropertyMappingBuilder StoringIdentity();

        /// <summary>
        /// Sets the current property as
        /// <see cref="ILdapAttributeMap{TObject}.IsPrimaryGroupProperty"/>.
        /// </summary>
        /// <returns><c>this</c>.</returns>
        ILdapPropertyMappingBuilder StoringPrimaryGroupFlag();

        /// <summary>
        /// Maps the property configured by this builder according to its
        /// annotation with <see cref="LdapAttributeAttribute"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the property does
        /// not have a <see cref="LdapAttributeAttribute"/> for the current
        /// schema.</exception>
        void ToAnnotatedAttribute();

        /// <summary>
        /// Maps the property configured by this builder to the LDAP attribute
        /// with the specified name.
        /// </summary>
        /// <param name="attributeName">The name of the LDAP attribute.</param>
        /// <returns>A builder for customising the mapping further.</returns>
        /// <exception cref="ArgumentException">If
        /// <paramref name="attributeName"/> is <c>null</c> or empty.
        /// </exception>
        ILdapAttributeMappingBuilder ToAttribute(string attributeName);

        /// <summary>
        /// Maps the property confgured by this builder to the given
        /// <paramref name="attribute"/>
        /// </summary>
        /// <param name="attribue">Specifies the name of the LDAP property and
        /// optionally the converter to be used.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="attribute"/> is <c>null</c>.</exception>
        void ToAttribute(LdapAttributeAttribute attribute);
    }
}
