// <copyright file="FileTimeConverter.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Globalization;
using Visus.Ldap.Properties;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// A converter that parses an attribute as <see cref="DateTimeOffset"/>
    /// or <see cref="DateTime"/>.
    /// </summary>
    /// <remarks>
    /// This converter is required, because lockout times in ADDS are stored as
    /// <c>FILETIME</c> structures.
    /// </remarks>
    public sealed class FileTimeConverter : IValueConverter {

        #region Public methods
        /// <inheritdoc />
        public object? Convert(object? value, Type target, object? parameter,
                CultureInfo culture) {
            ArgumentNullException.ThrowIfNull(target, nameof(target));

            var ticks = value switch {
                long l => l,
                null => 0L,
                _ => long.Parse(value.ToString()!)
            };

            switch (target) {
                case Type _ when target == typeof(DateTime):
                    return DateTime.FromFileTime(ticks);

                case Type _ when (target == typeof(DateTime?))
                        && (value == null):
                    return null;

                case Type _ when target == typeof(DateTime?):
                    return DateTime.FromFileTime(ticks);

                case Type _ when target == typeof(DateTimeOffset):
                    return DateTimeOffset.FromFileTime(ticks);

                case Type _ when target == typeof(DateTimeOffset?)
                        && (value == null):
                    return null;

                case Type _ when target == typeof(DateTimeOffset?):
                    return DateTimeOffset.FromFileTime(ticks);

                default:
                    throw new ArgumentException(
                        Resources.ErrorInvalidFileTimeTarget,
                        nameof(target));
            }
        }
        #endregion
    }
}
