// <copyright file="ClaimsAttribute.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Annotates a property of an LDAP user object as holding the
    /// <see cref="System.Security.Claims.Claim"/>s of the user.
    /// </summary>
    /// <remarks>
    /// Annotating multiple properties of the same class with this attribute
    /// will cause mapping to fail when using the default
    /// <see cref="LdapMapper{TUser}"/>. If there is a unique attribute in
    /// the user object, <see cref="LdapMapper{TUser, TGroup}"/> will assign
    /// the claims to this attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class ClaimsAttribute : Attribute {

        #region Public class methods
        /// <summary>
        /// Gets the only property in <typeparamref name="TType" /> that is
        /// annotated with <see cref="ClaimsAttribute"/>.
        /// </summary>
        /// <typeparam name="TType">The type to get the group property for.
        /// </typeparam>
        /// <returns>The property annotated as claims container or <c>null</c> if
        /// no unique identity was found.</returns>
        public static PropertyInfo GetClaims<TType>()
            => typeof(TType).GetProperties()
                .Where(p => IsClaims(p))
                .SingleOrDefault();

        /// <summary>
        /// Answer whether <paramref name="property"/> is annotated as claims
        /// container.
        /// </summary>
        /// <param name="property">The property to check. It is safe to pass
        /// <c>null</c>, in which case the result will always be <c>false</c>.
        /// </param>
        /// <returns><c>true</c> if <paramref name="property"/> is annotated as
        /// claims container.</returns>
        public static bool IsClaims(PropertyInfo property) {
            var att = property?.GetCustomAttribute<ClaimsAttribute>();
            var type = typeof(IEnumerable<Claim>);
            return (att != null) && type.IsAssignableFrom(property.PropertyType);
        }
        #endregion

    }
}
