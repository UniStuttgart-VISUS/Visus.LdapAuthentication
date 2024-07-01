// <copyright file="PrimaryGroupFlagAttribute.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Reflection;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Identifies the annotated attribute as a flag indicating whether
    /// a group is the primary group of the user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class PrimaryGroupFlagAttribute : Attribute {

        /// <summary>
        /// Gets the only property of <typeparamref name="TType"/> that is
        /// annotated with this attribute.
        /// </summary>
        /// <typeparam name="TType">The type to be checked.</typeparam>
        /// <returns></returns>
        public static PropertyInfo GetProperty<TType>()
            => typeof(TType).GetProperty<PrimaryGroupFlagAttribute>(
                p => (p.PropertyType == typeof(bool))
                && p.CanRead
                && p.CanWrite);
    }
}
