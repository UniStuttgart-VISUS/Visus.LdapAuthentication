// <copyright file="IClaimsMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Reflection;
using Visus.Ldap.Mapping;
using Visus.Ldap.Properties;


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
        IAttributeClaimsBuilder MapAttribute(LdapAttributeAttribute attribute);

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
        INewAttributeClaimsBuilder MapAttribute(string attributeName);

        /// <summary>
        /// Gets a builder for mapping claims to the given
        /// <paramref name="property"/>.
        /// </summary>
        /// <param name="property">The property to map the claims to.</param>
        /// <returns><c>this</c>.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="property"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If
        /// <paramref name="property"/> is not annotated with a
        /// <see cref="LdapAttributeAttribute"/>.</exception>
        IAttributeClaimsBuilder MapProperty(PropertyInfo property);

        /// <summary>
        /// Gets a builder for mapping claims to the property named
        /// <paramref name="propertyName"/> of <typeparamref name="TOwner"/>.
        /// </summary>
        /// <typeparam name="TOwner">The owner type of the property to map.
        /// </typeparam>
        /// <param name="propertyName">The name of the property to map.</param>
        /// <returns><c>this</c>.</returns>
        /// <exception cref="ArgumentException">If the specified property
        /// does not exist in the specified type.</exception>
        IAttributeClaimsBuilder MapProperty<TOwner>(string propertyName) {
            var p = typeof(TOwner).GetProperty(propertyName, PropertyFlags);

            if (p == null) {
                var msg = Resources.ErrorPropertyMissing;
                msg = string.Format(msg, propertyName, typeof(TOwner).Name);
                throw new ArgumentException(msg);
            }

            return this.MapProperty(p);
        }

        #region Private constants
        private const BindingFlags PropertyFlags = BindingFlags.Public
            | BindingFlags.Instance;
        #endregion
    }
}
