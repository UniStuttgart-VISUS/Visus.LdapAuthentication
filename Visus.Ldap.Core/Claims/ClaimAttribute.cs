// <copyright file="ClaimAttribute.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Visus.Ldap.Mapping;
using ClaimMap = System.Collections.Generic.Dictionary<
    System.Reflection.PropertyInfo,
    System.Collections.Generic.IEnumerable<Visus.Ldap.Claims.ClaimAttribute>>;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// Marks a property as a claim that a
    /// <see cref="ILdapMapper{TEntry, TUser, TGroup}"/> can convert into
    /// a claim for a security principal.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class ClaimAttribute : Attribute {

        #region Public class methods
        /// <summary>
        /// Gets the names of all claims attached to <paramref name="property"/>
        /// via <see cref="ClaimAttribute"/>s.
        /// </summary>
        /// <param name="property">The property to retrieve the claims for.
        /// </param>
        /// <returns>The names of the claims attached to the property.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="property"/> is <c>null</c>.</exception>
        public static IEnumerable<string> GetClaims(PropertyInfo property) {
            ArgumentNullException.ThrowIfNull(property, nameof(property));
            var retval = from a in property.GetCustomAttributes<ClaimAttribute>()
                         select a.Name;
            return retval;
        }

        /// <summary>
        /// Gets the names of all claims attached to the property names
        /// <paramref name="property"/> of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to retrieve the property for.</param>
        /// <param name="property">The name of the property to search the
        /// claims for.</param>
        /// <returns>The names of the claims attached to the property.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="type"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="property"/> is <c>null</c>.</exception>
        public static IEnumerable<string> GetClaims(Type type,
                string property) {
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            var prop = type.GetProperty(property);
            return prop != null
                ? GetClaims(prop)
                : Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets the names of all claims attached to the property names
        /// <paramref name="property"/> of <typeparamref name="TType"/>.
        /// </summary>
        /// <typeparam name="TType">The type to retrieve the property for.
        /// </typeparam>
        /// <param name="property">The name of the property to search the
        /// claims for.</param>
        /// <returns>The names of the claims attached to the property.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="property"/> is <c>null</c>.</exception>
        public static IEnumerable<string> GetClaims<TType>(string property) {
            return GetClaims(typeof(TType), property);
        }

        /// <summary>
        /// Gets all annotated <<see cref="string" /> properties of
        /// <paramref name="type"/> that are annotated with
        /// <see cref="ClaimAttribute"/>.
        /// </summary>
        /// <param name="type">The type to retrieve the claims for.</param>
        /// <returns>The properties and their attributes.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="type"/>
        /// is <c>null</c>.</exception>
        public static ClaimMap GetMap(Type type) {
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            return (from p in type.GetProperties()
                    let a = p.GetCustomAttributes<ClaimAttribute>()
                    where a != null
                    select new {
                        Property = p,
                        Attributes = a
                    }).ToDictionary(v => v.Property, v => v.Attributes);
        }

        /// <summary>
        /// Gets all annotated <<see cref="string" /> properties of
        /// <typeparamref name="TType"/> that are annotated with
        /// <see cref="ClaimAttribute"/>.
        /// </summary>
        /// <typeparam name="TType">The type to retrieve the claims for.
        /// </typeparam>
        /// <param name="type"></param>
        /// <returns>The properties and their attributes.</returns>
        public static ClaimMap GetMap<TType>() => GetMap(typeof(TType));
        #endregion

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="name">The name of the claim.</param>
        public ClaimAttribute(string name) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the name of the claim.
        /// </summary>
        public string Name { get; }
        #endregion
    }
}
