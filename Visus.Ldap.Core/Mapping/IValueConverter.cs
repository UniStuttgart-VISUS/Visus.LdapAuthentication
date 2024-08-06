// <copyright file="IValueConverter.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Globalization;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// Converts the value(s) of an LDAP attribute into the form that is used in
    /// the in-memory representation of the user or group type.
    /// </summary>
    public interface IValueConverter {

        /// <summary>
        /// Gets, if any, the preferred source type.
        /// </summary>
        /// <remarks>
        /// The LDAP libraries have in some cases the ability to return values
        /// as <see cref="string"/> or <see cref="byte[]"/> or arrays thereof.
        /// Using this property, a converter can specify the preferred format
        /// of the input.
        /// </remarks>
        Type? PreferredSource { get; }

        /// <summary>
        /// Converts <paramref name="value"/> into a user-defined object,
        /// normally a string.
        /// </summary>
        /// <param name="value">The attribute value to be converted.</param>
        /// <param name="target">The target type to convert to. An implementation
        /// might only support one target type and use this parameter to check
        /// whether the caller expects the correct type.</param>
        /// <param name="parameter">An optional converter parameter.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to assume, for
        /// instance when converting from and to string representations that may
        /// be culture-dependent.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref="ArgumentException">If the requested conversion
        /// cannot be performed for he given types.</exception>
        object? Convert(object? value, Type target, object? parameter,
            CultureInfo culture);

        /// <summary>
        /// Converts <paramref name="value"/> into a user-defined object,
        /// normally a string.
        /// </summary>
        /// <param name="value">The attribute value to be converted.</param>
        /// <param name="target">The target type to convert to. An implementation
        /// might only support one target type and use this parameter to check
        /// whether the caller expects the correct type.</param>
        /// <param name="parameter">An optional converter parameter.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref="ArgumentException">If the requested conversion
        /// cannot be performed for he given types.</exception>
        object? Convert(object? value, Type target, object? parameter = null) {
            return this.Convert(value, target, parameter,
                CultureInfo.CurrentCulture);
        }
    }
}
