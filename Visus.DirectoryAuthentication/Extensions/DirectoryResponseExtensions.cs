// <copyright file="DirectoryResponseExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication.Extensions {

    /// <summary>
    /// Extension methods for <see cref="DirectoryResponse"/> and derived
    /// classes.
    /// </summary>
    internal static class DirectoryResponseExtensions {

        /// <summary>
        /// Answer whether the response contains any entries.
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool Any(this SearchResponse that)
            => (that != null) && (that.Entries.Count > 0);

        /// <summary>
        /// Answer whether the response is a <see cref="SearchResponse"/>.
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSearchResponse(this DirectoryResponse that)
            => (that != null) && (that is SearchResponse);
    }
}
