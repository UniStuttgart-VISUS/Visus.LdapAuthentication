// <copyright file="SearchResultEntryExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2022 -2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.DirectoryServices.Protocols;
using System.Runtime.CompilerServices;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Extension methods for <see cref="SearchResultEntry"/>.
    /// </summary>
    internal static class SearchResultEntryExtensions {

        /// <summary>
        /// Gets the attribute with the specified name from
        /// <paramref name="that"/>.
        /// </summary>
        /// <remarks>
        /// This is a convenience accessor for the
        /// <see cref="SearchResultEntry.Attributes"/> property, which reduces
        /// the changes required to port from Novell LDAP.
        /// </remarks>
        /// <param name="that">The entry to retrieve the attribute for.</param>
        /// <param name="attribute">The name of the attribute to be retrived.
        /// </param>
        /// <returns>The <see cref="DirectoryAttribute"/> with the specified
        /// name.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DirectoryAttribute GetAttribute(
                this SearchResultEntry that, string attribute) {
            return that?.Attributes[attribute];
        }
    }
}
