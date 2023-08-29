// <copyright file="LdapAttributeExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2022 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Extension methods for <see cref="DirectoryAttribute"/>.
    /// </summary>
    public static class LdapAttributeExtensions {

        /// <summary>
        /// Gets the string value of an attribute.
        /// </summary>
        /// <param name="that">An LDAP attribute.</param>
        /// <param name="attribute">An annotation that specifies the
        /// potential conversion of the value. It is safe to pass <c>null</c>,
        /// in which case the string value will be returned directly.</param>
        /// <param name="parameter">An optional parameter that will be passed
        /// to the converter from <paramref name="attribute"/>.</param>
        /// <returns>The string version of the attribute.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="that"/> is <c>null</c>.</exception>
        public static string ToString(this DirectoryAttribute that,
                LdapAttributeAttribute attribute, object parameter = null) {
            return that.ToString(attribute?.GetConverter(), parameter);
        }

        /// <summary>
        /// Gets the string value of an attribute.
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
        /// <returns>The string version of the attribute.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="that"/> is <c>null</c>.</exception>
        public static string ToString(this DirectoryAttribute that,
                ILdapAttributeConverter converter,
                object parameter = null) {
            if (that == null) {
                return null;

            } else if (converter != null) {
                return converter.Convert(that, parameter) as string;

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
