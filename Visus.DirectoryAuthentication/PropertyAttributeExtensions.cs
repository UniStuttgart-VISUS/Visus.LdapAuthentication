// <copyright file="PropertyAttributeExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Data;
using System.Linq;
using System.Reflection;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Extension methods for finding annotated properties.
    /// </summary>
    internal static class PropertyAttributeExtensions {

        /// <summary>
        /// Gets the only property in <paramref name="that"/> that is annotated
        /// with <typeparamref name="TAttribute"/>.
        /// </summary>
        /// <remarks>
        /// Note that the method will return nothing if multiple properties are
        /// annotated with <typeparamref name="TAttribute"/>.
        /// </remarks>
        /// <typeparam name="TAttribute">The attribute to check.</typeparam>
        /// <param name="that">The type to be checked.</param>
        /// <param name="predicate">An optional predicate that the property
        /// must also fulfill to be eligible. If <c>null</c>, the predicate
        /// will match any property.</param>
        /// <returns>The property annotated with
        /// <typeparamref name="TAttribute"/>, or <c>null</c> if no unique
        /// property was found.</returns>
        internal static PropertyInfo GetProperty<TAttribute>(this Type that,
                Func<PropertyInfo, bool> predicate)
                where TAttribute : Attribute
            => that.GetProperties()
                .Where(p => IsDefined<TAttribute>(p) && predicate.NotFalse(p))
                .SingleOrDefault();

        /// <summary>
        /// Answer whether <paramref name="that"/> is annotated with
        /// <typeparamref name="TAttribute"/>.
        /// </summary>
        /// <typeparam name="TAttribute">The attribute to check.</typeparam>
        /// <param name="that">The property to check. It is safe to pass
        /// <c>null</c>, in which case the result will always be <c>false</c>.
        /// </param>
        /// <returns><c>true</c> if <paramref name="that"/> is annotated 
        /// with <typeparamref name="TAttribute"/>, <c>false</c> otherwise.
        /// </returns>
        internal static bool IsDefined<TAttribute>(this PropertyInfo that)
                where TAttribute : Attribute
            => (that?.GetCustomAttribute<TAttribute>() != null);

        /// <summary>
        /// Answer whether <paramref name="that"/> is either <c>null</c> or
        /// yields <c>true</c> for <paramref name="property"/>.
        /// </summary>
        private static bool NotFalse(this Func<PropertyInfo, bool> that,
                PropertyInfo property)
            => (that?.Invoke(property) != false);
    }
}
