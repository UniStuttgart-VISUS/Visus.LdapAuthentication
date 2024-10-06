// <copyright file="StringExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Visus.Ldap.Properties;

namespace Visus.Ldap.Extensions {

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
        [return: NotNullIfNotNull(nameof(that))]
        public static string? EscapeLdapFilterExpression(this string? that) {
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

        /// <summary>
        /// Create an LDAP filter for matching the given
        /// <paramref name="attribute"/> to the given value
        /// <paramref name="that"/>.
        /// </summary>
        /// <param name="that">The value to be searched. The method will perform
        /// escaping using <see cref="EscapeLdapFilterExpression(string?)"/>
        /// before constructing the filter.</param>
        /// <param name="attribute">The attribute to search the value in.
        /// </param>
        /// <returns>A filter expression for the attribute and value.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="that"/> is <c>null</c>, or if
        /// <paramref name="attribute"/> is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToLdapFilter(this string that, string attribute) {
            ArgumentNullException.ThrowIfNull(that);
            ArgumentNullException.ThrowIfNull(attribute);
            that = that.EscapeLdapFilterExpression();
            return $"({attribute}={that})";
        }

        /// <summary>
        /// Create an LDAP filter that catches any of the
        /// <paramref name="that"/> for the given <paramref name="attribute"/>.
        /// </summary>
        /// <param name="that">The values that should be in the result set.
        /// The method will escape these using
        /// <see cref="EscapeLdapFilterExpression(string?)"/>.</param>
        /// <param name="attribute">The attribute to search the values in.
        /// </param>
        /// <returns>A filter expression matching any of the values for
        /// <paramref name="attribute"/>.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="that"/> is <c>null</c>, or if
        /// <paramref name="attribute"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="that"/>
        /// does not contain at least one non-<c>null</c> string.</exception>
        public static string ToLdapFilter(this IEnumerable<string> that,
                string attribute) {
            ArgumentNullException.ThrowIfNull(that);
            ArgumentNullException.ThrowIfNull(attribute);

            var filters = that.Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.EscapeLdapFilterExpression()!)
                .Select(v => $"({attribute}={v})");

            if (!that.Any()) {
                throw new ArgumentException(Resources.ErrorEmptyFilterList,
                    nameof(that));
            }

            return $"(|{string.Join("", filters)})";
        }
    }
}
