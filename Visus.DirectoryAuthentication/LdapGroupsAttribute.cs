// <copyright file="LdapGroupsAttribute.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Linq;
using System.Reflection;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Annotates a property of a LDAP user object as holding the LDAP groups.
    /// </summary>
    /// <remarks>
    /// Annotating multiple properties of the same class with this attribute
    /// will cause mapping to fail when using the default
    /// <see cref="LdapMapper{TUser}"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class LdapGroupsAttribute : Attribute{

        #region Public class methods
        /// <summary>
        /// Gets the only property in <paramref name="type"/> that is annotated
        /// with <see cref="LdapGroupsAttribute"/>.
        /// </summary>
        /// <remarks>
        /// The method will return nothing if multiple properties are annotated
        /// as identity.
        /// </remarks>
        /// <param name="type">The type to get the group property for.</param>
        /// <returns>The property annotated as group container or <c>null</c> if
        /// no unique identity was found.</returns>
        public static PropertyInfo GetLdapGroups<TType>()
            => typeof(TType).GetProperties()
                .Where(p =>IsLdapGroups(p))
                .SingleOrDefault();

        /// <summary>
        /// Answer whether <paramref name="property"/> is annotated as LDAP
        /// group container.
        /// </summary>
        /// <param name="property">The property to check. It is safe to pass
        /// <c>null</c>, in which case the result will always be <c>false</c>.
        /// </param>
        /// <returns><c>true</c> if <paramref name="property"/> is annotated as
        /// groups container.</returns>
        public static bool IsLdapGroups(PropertyInfo property)
            => (property?.GetCustomAttribute<LdapGroupsAttribute>() != null);
        #endregion

    }
}
