// <copyright file="LdapAttributeExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Globalization;
using Visus.Ldap.Mapping;


namespace Visus.LdapAuthentication.Extensions {

    /// <summary>
    /// Extension methods for <see cref="LdapAttribute"/> and the
    /// <see cref="LdapAttributeAttribute"/> annotation.
    /// </summary>
    public static class LdapAttributeExtensions {

        /// <summary>
        /// Gets the value of an attribute from the given
        /// <paramref name="entry"/>, possibly converting it using the
        /// <see cref="IValueConverter"/> configured in the
        /// <see cref="LdapAttributeAttribute"/>.
        /// </summary>
        /// <param name="that">An LDAP attribute descriptor.</param>
        /// <param name="entry">The entry to retrieve the attribute
        /// from.</param>
        /// <param name="parameter">An optional parameter that will be passed
        /// to the converter from <paramref name="attribute"/>.</param>
        /// <param name="cultureInfo">The culture possibly used to convert the
        /// attribute value. This parameter can be <c>null</c>, in which case
        /// <see cref="CultureInfo.CurrentCulture" /> will be used.</param>
        /// <returns>The value of the attribute.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="that"/> is <c>null</c>.</exception>
        public static object? GetValue(this LdapAttributeAttribute that,
                LdapEntry entry,
                object? parameter = null,
                CultureInfo? cultureInfo = null) {
            ArgumentNullException.ThrowIfNull(that, nameof(that));
            var attribute = entry?.GetAttribute(that.Name);
            return attribute.GetValue(that.GetConverter(), parameter,
                cultureInfo);
        }

        /// <summary>
        /// Gets the value of an attribute.
        /// </summary>
        /// <remarks>
        /// <para>If a non-<c>null</c> converter is specified, this converter
        /// will be used to generate a string value.</para>
        /// <para>If the attribute is array-valued, only the first element will
        /// be converted to a string value.</para>
        /// </remarks>
        /// <param name="that">An LDAP attribute.</param>
        /// <param name="converter">An optional converter that is used to
        /// transform the attribute to a string. It is safe to pass <c>null</c>,
        /// in which case the string value will be returned directly.</param>
        /// <param name="parameter">An optional parameter that is passed to
        /// <paramref name="converter"/> as necessary.</param>
        /// <param name="cultureInfo">The culture possibly used to convert the
        /// attribute value. This parameter can be <c>null</c>, in which case
        /// <see cref="CultureInfo.CurrentCulture" /> will be used.</param>
        /// <returns>The value of the attribute.</returns>
        public static object? GetValue(this LdapAttribute? that,
                IValueConverter? converter,
                object? parameter = null,
                CultureInfo? cultureInfo = null) {
            if ((that == null) || (that.Size() == 0)) {
                return null;

            } else if (converter != null) {
                return converter.Convert(
                    (object?) that.StringValue ?? that.ByteValue,
                    typeof(string),
                    parameter,
                    cultureInfo ?? CultureInfo.CurrentCulture);

            } else if (that.StringValue != null) {
                return that.StringValue;

            } else if (that.ByteValue != null) {
                return Convert.ToBase64String(that.ByteValue);

            } else {
                return null;
            }
        }

        /// <summary>
        /// Gets all values of the specified <paramref name="type"/> that
        /// <paramref name="that"/> has.
        /// </summary>
        /// <remarks>
        /// This emulates an API that is supported in Microsoft's directory
        /// services API, such we can easier copy code between both libraries.
        /// </remarks>
        /// <param name="that">The attribute to get the values of.</param>
        /// <param name="type">The type to retrieve, which can be either a
        /// <see cref="string"/> or a <see cref="byte"/> array.</param>
        /// <returns>The values of the attribute.</returns>
        public static IEnumerable<object> GetValues(this LdapAttribute that,
                Type type) {
            ArgumentNullException.ThrowIfNull(that, nameof(that));
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            if (type == typeof(string)) {
                if (that.StringValues != null) {
                    while (that.StringValues.MoveNext()) {
                        yield return that.StringValues.Current;
                    }

                } else if (that.StringValue != null) {
                    yield return that.StringValue;
                }

            } else if (type == typeof(byte[])) {
                if (that.ByteValueArray?.Length > 0) {
                    foreach (var v in that.ByteValueArray) {
                        yield return v;
                    }

                } else if (that.ByteValues != null) {
                    yield return that.ByteValues;
                }
            }

        }
    }
}
