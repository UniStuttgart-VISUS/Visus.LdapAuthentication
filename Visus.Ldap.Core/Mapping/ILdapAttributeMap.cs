// <copyright file="ILdapAttributeMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;
using System.Reflection;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// Provides mapping information for the properties of
    /// <typeparamref name="TObject"/> to LDAP attributes.
    /// </summary>
    /// <typeparam name="TObject">The object to be reflected.</typeparam>
    public interface ILdapAttributeMap<TObject>
            : IEnumerable<KeyValuePair<PropertyInfo, LdapAttributeAttribute>> {

        /// <summary>
        /// Gets, if any, the property marked with
        /// <see cref="Mapping.AccountNameAttribute"/>.
        /// </summary>
        PropertyInfo? AccountNameProperty { get; }

        /// <summary>
        /// Gets, if any, the LDAP attribute of the property marked with
        /// <see cref="Mapping.AccountNameAttribute"/>.
        /// </summary>
        LdapAttributeAttribute? AccountNameAttribute
            => (this.AccountNameProperty != null)
            ? this[this.AccountNameProperty]
            : null;

        /// <summary>
        /// Gets the distinct names of all <see cref="Attributes"/>.
        /// </summary>
        IEnumerable<string> AttributeNames { get; }

        /// <summary>
        /// Gets all LDAP attributes mapped to any of the properties of
        /// <typeparamref name="TObject"/>.
        /// </summary>
        IEnumerable<LdapAttributeAttribute> Attributes { get; }

        /// <summary>
        /// Gets, if any, the property marked with
        /// <see cref="Mapping.DistinguishedNameAttribute"/>.
        /// </summary>
        PropertyInfo? DistinguishedNameProperty { get; }

        /// <summary>
        /// Gets, if any, the LDAP attribute of the property marked with
        /// <see cref="Mapping.DistinguishedNameAttribute"/>.
        /// </summary>
        LdapAttributeAttribute? DistinguishedNameAttribute
            => (this.DistinguishedNameProperty != null)
            ? this[this.DistinguishedNameProperty]
            : null;

        /// <summary>
        /// Gets, if any, the property marked with
        /// <see cref="GroupMembershipsAttribute"/>.
        /// </summary>
        PropertyInfo? GroupMembershipsProperty { get; }

        /// <summary>
        /// Gets, if any, the property marked with
        /// <see cref="Mapping.IdentityAttribute"/>.
        /// </summary>
        PropertyInfo? IdentityProperty { get; }

        /// <summary>
        /// Gets, if any, the LDAP attribute of the property marked with
        /// <see cref="Mapping.IdentityAttribute"/>.
        /// </summary>
        LdapAttributeAttribute? IdentityAttribute
            => (this.IdentityProperty != null)
            ? this[this.IdentityProperty]
            : null;

        /// <summary>
        /// Gets, if any, the property marked with
        /// <see cref="IsPrimaryGroupProperty"/>.
        /// </summary>
        PropertyInfo? IsPrimaryGroupProperty { get; }

        /// <summary>
        /// Gets all properties of <typeparamref name="TObject"/> that are
        /// annotated with <see cref="LdapAttributeAttribute"/>.
        /// </summary>
        IEnumerable<PropertyInfo> Properties { get; }

        /// <summary>
        /// Gets, if any, the <see cref="LdapAttributeAttribute"/> for the given
        /// property.
        /// </summary>
        /// <param name="property">The property to get the attribute for.</param>
        /// <returns>The LDAP attribute or <c>null</c> if the property does not
        /// have an <see cref="LdapAttributeAttribute"/>.</returns>
        LdapAttributeAttribute? this[PropertyInfo property] { get; }

        /// <summary>
        /// Gets, if any, the <see cref="LdapAttributeAttribute"/> for the given
        /// property name.
        /// </summary>
        /// <param name="propertyName">The name of the property to get the
        /// attribute for.</param>
        /// <returns>The LDAP attribute or <c>null</c> if the property does not
        /// have an <see cref="LdapAttributeAttribute"/>.</returns>
        //LdapAttributeAttribute? this[string propertyName] { get; }
    }
}
