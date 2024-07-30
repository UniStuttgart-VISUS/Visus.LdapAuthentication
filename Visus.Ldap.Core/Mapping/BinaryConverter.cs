// <copyright file="BinaryConverter.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Globalization;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// Performs conversion from binary attribute values to base64-encoded
    /// strings.
    /// </summary>
    /// <remarks>
    /// The target types for the conversion are <see cref="byte[]"/>, in which
    /// case the input will remain unmodified, or <see cref="string"/>, in which
    /// are it will be converted to a base64 string. If a string is specified as
    /// the converter parameter, it is assumed to be the mime type and the
    /// returned string will become inline data of this type.
    /// </remarks>
    public sealed class BinaryConverter : IValueConverter {

        /// <inheritdoc />
        public object? Convert(object? value, Type target,
                object? parameter, CultureInfo culture) {
            if (value is byte[] bytes) {
                if (target == typeof(byte[])) {
                    return value;

                } else if (target == typeof(string)) {
                    var retval = System.Convert.ToBase64String(bytes);

                    if (parameter is string mimeType) {
                        retval = $"data:{mimeType};base64,{retval}";
                    }

                    return retval;
                }
            }

            return null;
        }
    }
}
