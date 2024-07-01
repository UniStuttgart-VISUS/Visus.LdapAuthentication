// <copyright file="DistinguishedNameAttribute.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Linq;
using System.Reflection;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Annotates a property of a LDAP user or group object as holding the
    /// distinguished name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class DistinguishedNameAttribute : Attribute {

        #region Public class methods
        /// <summary>
        /// Gets the only property in <paramref name="type"/> that is annotated
        /// with <see cref="DistinguishedNameAttribute"/>.
        /// </summary>
        /// <remarks>
        /// The method will return nothing if multiple properties are annotated
        /// as identity.
        /// </remarks>
        /// <param name="type">The type to get the group property for.</param>
        /// <returns>The property annotated as group container or <c>null</c> if
        /// no unique identity was found.</returns>
        public static PropertyInfo GetDistinguishedName(Type type) {
            if (type == null) {
                return null;
            }

            return (from p in type.GetProperties()
                    where IsDistinguishedName(p)
                    select p).SingleOrDefault();
        }

        /// <summary>
        /// Answer whether <paramref name="property"/> is annotated as the
        /// distinguished name.
        /// </summary>
        /// <param name="property">The property to check. It is safe to pass
        /// <c>null</c>, in which case the result will always be <c>false</c>.
        /// </param>
        /// <returns><c>true</c> if <paramref name="property"/> is annotated as
        /// identity property.</returns>
        public static bool IsDistinguishedName(PropertyInfo property)
            => (property?.GetCustomAttribute<DistinguishedNameAttribute>()
                != null);
        #endregion
    }
}
