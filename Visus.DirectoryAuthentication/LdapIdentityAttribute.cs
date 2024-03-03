// <copyright file="LdapIdentityAttribute.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Linq;
using System.Reflection;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Annotates a property of a fully custom LDAP user object as the identity
    /// property which can be used to retrieve the LDAP entry for the object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class LdapIdentityAttribute : Attribute {

        #region Public class methods
        /// <summary>
        /// Gets the only property in <paramref name="type"/> that is annotated
        /// with <see cref="LdapIdentityAttribute"/>.
        /// </summary>
        /// <remarks>
        /// The method will return nothing if multiple properties are annotated
        /// as identity.
        /// </remarks>
        /// <param name="type">The type to get the identity for.</param>
        /// <returns>The property annotated as identity or <c>null</c> if no
        /// unique identity was found.</returns>
        public static PropertyInfo GetLdapIdentity(Type type) {
            if (type == null) {
                return null;
            }

            return (from p in type.GetProperties()
                    where IsLdapIdentity(p)
                    select p).SingleOrDefault();
        }

        /// <summary>
        /// Answer whether <paramref name="property"/> is annotated as LDAP
        /// identity.
        /// </summary>
        /// <param name="property">The property to check. It is safe to pass
        /// <c>null</c>, in which case the result will always be <c>false</c>.
        /// </param>
        /// <returns><c>true</c> if <paramref name="property"/> is annotated as
        /// identity property.</returns>
        public static bool IsLdapIdentity(PropertyInfo property) {
            if (property == null) {
                return false;
            }

            return property.GetCustomAttribute<LdapIdentityAttribute>() != null;
        }
        #endregion
    }
}
