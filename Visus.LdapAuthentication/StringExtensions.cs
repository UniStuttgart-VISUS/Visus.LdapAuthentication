// <copyright file="StringExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.LdapAuthentication {

    /// <summary>
    /// LDAP-related extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions {

        /// <summary>
        /// Performs escape of filter epxression according to
        /// https://docs.microsoft.com/en-us/windows/desktop/adsi/search-filter-syntax
        /// </summary>
        /// <param name="that">The filter string to be escaped. It is safe
        /// to pass <c>null</c>, in which case the result will be <c>null</c>.
        /// </param>
        /// <returns>The escapted filter string.</returns>
        public static string EscapeLdapFilterExpression(this string that) {
            if (that == null) {
                return null;
            }

            that = that.Replace("*", "\\2a");
            that = that.Replace("(", "\\28");
            that = that.Replace(")", "\\29");
            that = that.Replace("\\", "\\5c");
            that = that.Replace("\0", "\\00");
            that = that.Replace("/", "\\2f");

            return that;
        }
    }
}
