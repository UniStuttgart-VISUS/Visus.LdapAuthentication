// <copyright file="LdapAttributeExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.DirectoryServices.Protocols;
using System.Globalization;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Extensions {

    /// <summary>
    /// Extension methods for <see cref="DirectoryAttribute"/>.
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
                SearchResultEntry entry,
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
        /// will be used to generate the string value.</para>
        /// <para>If the attribute is array-valued, only the first element will
        /// be converted to a string value.</para>
        /// <para>According to Microsoft's documentation at
        /// https://learn.microsoft.com/de-de/dotnet/api/system.directoryservices.protocols.directoryattribute.item
        /// the value is a string whenever possible and a byte array
        /// otherwise. In the former case, the string will be returned as
        /// is. In the latter case, the byte array will be converted to
        /// a base64-encoded string.</para>
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
        public static object? GetValue(this DirectoryAttribute? that,
                IValueConverter? converter,
                object? parameter = null,
                CultureInfo? cultureInfo = null) {
            if (that == null) {
                return null;

            } else if (converter != null) {
                return converter.Convert(that[0], typeof(string), parameter,
                    cultureInfo ?? CultureInfo.CurrentCulture);

            } else if (that[0] is string s) {
                return s;

            } else if (that[0] is byte[] b) {
                return Convert.ToBase64String(b);

            } else {
                return null;
            }
        }
    }
}
