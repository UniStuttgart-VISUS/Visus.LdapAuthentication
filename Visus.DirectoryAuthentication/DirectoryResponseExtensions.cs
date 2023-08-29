// <copyright file="DirectoryResponseExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication {

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
        public static bool Any(this SearchResponse that) {
            return ((that != null) && (that.Entries.Count > 0));
        }

        /// <summary>
        /// Answer whether the response is a <see cref="SearchResponse"/>.
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSearchResponse(this DirectoryResponse that) {
            return ((that != null) && (that is SearchResponse));
        }
    }
}
