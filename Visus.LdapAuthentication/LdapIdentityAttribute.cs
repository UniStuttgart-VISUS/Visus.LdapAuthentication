// <copyright file="LdapIdentityAttribute.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Reflection;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Annotates a property of a fully custom LDAP user object as the identity
    /// property which can be used to retrieve the LDAP entry for the object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class LdapIdentityAttribute : Attribute {

        /// <summary>
        /// Gets the only property in <paramref name="type"/> that is annotated
        /// with <see cref="LdapIdentityAttribute"/>.
        /// </summary>
        /// <remarks>
        /// The method will return nothing if multiple properties are annotated
        /// as identity or if the annotated property is not writable or not a
        /// <see cref="string"/>.
        /// </remarks>
        /// <typeparam name="TType">The type to get the identity for.
        /// </typeparam>
        /// <returns>The property annotated as identity or <c>null</c> if no
        /// unique identity was found.</returns>
        public static PropertyInfo GetProperty<TType>()
            => typeof(TType).GetProperty<LdapIdentityAttribute>(
                p => (p.PropertyType == typeof(string))
                && p.CanRead
                && p.CanWrite);
    }
}
