// <copyright file="LdapAttributeExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Visus.Ldap.Mapping;


namespace Visus.LdapAuthentication.Extensions {

    /// <summary>
    /// Extension methods for <see cref="LdapAttribute"/> and the
    /// <see cref="LdapAttributeAttribute"/> annotation.
    /// </summary>
    public static class LdapAttributeExtensions {

        /// <summary>
        /// Gets the value of an attribute from the given
        /// <paramref name="entry"/>, possibly converting it to
        /// <paramref name="targetType"/> using the
        /// <see cref="IValueConverter"/> configured in <paramref name="that"/>.
        /// </summary>
        /// <param name="that">An LDAP attribute descriptor.</param>
        /// <param name="entry">The entry to retrieve the attribute
        /// from.</param>
        /// <param name="targetType">The target type to return. If anything
        /// but a <see cref="string"/> or a <see cref="byte"/> array is
        /// requested, you must provide a <paramref name="converter"/>.</param>
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
                Type targetType,
                object? parameter = null,
                CultureInfo? cultureInfo = null) {
            ArgumentNullException.ThrowIfNull(that, nameof(that));
            try {
                var attribute = entry?.GetAttribute(that.Name);
                return attribute.GetValue(targetType,
                    that.GetConverter(),
                    parameter,
                    cultureInfo);
            } catch (KeyNotFoundException) {
                return null;
            }
        }

        /// <summary>
        /// Gets the value of an attribute from the given
        /// <paramref name="entry"/>, possibly converting it to
        /// <typeparamref name="TValue"/> using the
        /// <see cref="IValueConverter"/> configured in <paramref name="that"/>.
        /// </summary>
        /// <typeparam name="TValue">The target type to return. If anything
        /// but a <see cref="string"/> or a <see cref="byte"/> array is
        /// requested, you must provide a <paramref name="converter"/>.
        /// </typeparam>
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
        public static TValue? GetValue<TValue>(this LdapAttributeAttribute that,
                LdapEntry entry,
                object? parameter = null,
                CultureInfo? cultureInfo = null)
            => (TValue?) that.GetValue(entry,
                typeof(TValue),
                parameter,
                cultureInfo);

        /// <summary>
        /// Gets the value of an attribute.
        /// </summary>
        /// <remarks>
        /// <para>If a non-<c>null</c> converter is specified, this converter
        /// will be used to generate a string value.</para>
        /// <para>If the attribute is array-valued, only the first element will
        /// be returned.</para>
        /// <para>According to Microsoft's documentation at
        /// https://learn.microsoft.com/de-de/dotnet/api/system.directoryservices.protocols.directoryattribute.item
        /// the value is a string whenever possible and a byte array
        /// otherwise. In the former case, the string will be returned as
        /// is. In the latter case, the byte array will be converted to
        /// a base64-encoded string.</para>
        /// </remarks>
        /// <param name="that">An LDAP attribute. It is safe to pass
        /// <c>null</c>, in which case the returned value will be <c>null</c>
        /// as well.</param>
        /// <param name="targetType">The target type to return. If anything
        /// but a <see cref="string"/> or a <see cref="byte"/> array is
        /// requested, you must provide a <paramref name="converter"/>.</param>
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
                Type targetType,
                IValueConverter? converter,
                object? parameter = null,
                CultureInfo? cultureInfo = null) {
            if ((that == null) || (that.Size() < 1) || (targetType == null)) {
                return null;

            } else if (converter != null) {
                // Note: Novell provides a string conversion for byte arrays in
                // their entry, which is not what we want for SIDs or pictures,
                // so we force the byte value here.
                var value = converter.PreferredSource switch {
                    Type t when t == typeof(byte[][]) => that.ByteValueArray as object,
                    Type u when u == typeof(byte[]) => that.ByteValue,
                    Type v when v == typeof(string[]) => that.StringValueArray,
                    _ => that.StringValue
                };

                return converter.Convert(value,
                    targetType,
                    parameter,
                    cultureInfo ?? CultureInfo.CurrentCulture);

            } else if ((that.StringValue != null)
                    && (targetType == typeof(string))) {
                // Have a string and want a string, so that is easy.
                return that.StringValue;

            } else if (that.ByteValue != null) {
                return (targetType == typeof(string))
                    ? Convert.ToBase64String(that.ByteValue)
                    : that.ByteValue;

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
            ArgumentNullException.ThrowIfNull(that);
            ArgumentNullException.ThrowIfNull(type);

            if (type == typeof(string)) {
                if (that.StringValueArray?.Length > 0) {
                    foreach (var v in that.StringValueArray) {
                        yield return v;
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

        /// <summary>
        /// Gets all values of the specified <typeparamref name="TObject"/> that
        /// <paramref name="that"/> has.
        /// </summary>
        /// <typeparam name="TObject">The type to retrieve, which can be either
        /// a <see cref="string"/> or a <see cref="byte"/> array.</typeparam>
        /// <param name="that">The attribute to get the values of.</param>
        /// <returns>The values of the attribute.</returns>
        public static IEnumerable<TObject> GetValues<TObject>(
                this LdapAttribute that)
            => that.GetValues(typeof(TObject)).Cast<TObject>();
    }
}
