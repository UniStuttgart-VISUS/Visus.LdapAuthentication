// <copyright file="LdapSearchResultsExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using System;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Extension methods
    /// </summary>
    internal static class LdapSearchResultsExtensions {

        /// <summary>
        /// Returns the next <see cref="LdapEntry"/> is the given
        /// <see cref="ILdapSearchResults"/> and hides an exception that would
        /// be thrown if the search result is empty.
        /// </summary>
        /// <remarks>
        /// Unfortunately, <see cref="ILdapSearchResults.HasMore"/> does not
        /// only indicate whether there is another <see cref="LdapEntry"/>, but
        /// also whether there is another error. As we need a lenient behaviour
        /// in some cases, this extension method hides this potential exception.
        /// </remarks>
        /// <param name="that"></param>
        /// <returns></returns>
        internal static LdapEntry NextEntry(this ILdapSearchResults that,
                ILogger logger = null) {
            try {
                return that.Next();
            } catch (Exception ex) {
                logger?.LogError(ex.Message);
                return null;
            }
        }
    }
}
