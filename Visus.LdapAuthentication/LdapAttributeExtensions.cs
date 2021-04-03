// <copyright file="LdapAttributeExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using System;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Extension methods for <see cref="LdapAttribute"/>.
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
        public static string ToString(this LdapAttribute that,
                LdapAttributeAttribute attribute, object parameter = null) {
            return that.ToString(attribute?.GetConverter(), parameter);
        }

        /// <summary>
        /// Gets the string value of an attribute.
        /// </summary>
        /// <param name="that">An LDAP attribute.</param>
        /// <param name="converter">An optional converter that is used to
        /// transform the attribute to a string. It is safe to pass <c>null</c>,
        /// in which case the string value will be returned directly.</param>
        /// <param name="parameter">An optional parameter that is passed to
        /// <paramref name="converter"/> as necessary.</param>
        /// <returns>The string version of the attribute.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="that"/> is <c>null</c>.</exception>
        public static string ToString(this LdapAttribute that,
                ILdapAttributeConverter converter,
                object parameter = null) {
            return (converter != null)
                ? converter.Convert(that, parameter) as string
                : that.StringValue;
        }
    }
}
