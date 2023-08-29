// <copyright file="ClaimAttribute.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2023 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Visus.DirectoryAuthentication {

        /// <summary>
    /// Marks a property as a claim that is automatically added by
    /// <see cref="LdapUserBase"/>.
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
            _ = property ?? throw new ArgumentNullException(nameof(property));

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
            _ = type ?? throw new ArgumentNullException(nameof(type));
            _ = property ?? throw new ArgumentNullException(nameof(property));

            var prop = type.GetProperty(property);

            return (prop != null)
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
        #endregion

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="name">The name of the claim.</param>
        public ClaimAttribute(string name) {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
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
