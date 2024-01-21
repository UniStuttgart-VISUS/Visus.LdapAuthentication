// <copyright file="TypeExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Reflection;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// Extension methods for <see cref="Type"/>.
    /// </summary>
    internal static class TypeExtensions {

        /// <summary>
        /// Gets the <see cref="TypeInfo"/> of the generic instance of
        /// <paramref name="genericType"/> if <paramref name="that"/> is an
        /// instance of this type or derived from such an instance.
        /// </summary>
        /// <param name="that">The type to check.</param>
        /// <param name="genericType">The generic type to check whether
        /// <paramref name="that"/> is an instance of.</param>
        /// <returns>The type info of the type (or base type) that is the
        /// instance of <paramref name="genericType"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or if <paramref name="genericType"/> is <c>null</c>.
        /// </exception>
        public static TypeInfo GetGenericBaseTypeInfo(this Type that,
                Type genericType) {
            _ = that
                ?? throw new ArgumentNullException(nameof(that));
            _ = genericType
                ?? throw new ArgumentNullException(nameof(genericType));

            while (that != null) {
                var generic = that.IsGenericType
                    ? that.GetGenericTypeDefinition()
                    : null;

                if (generic == genericType) {
                    return that.GetTypeInfo();
                } else {
                    that = that.BaseType;
                }
            }

            return null;
        }
    }

}
