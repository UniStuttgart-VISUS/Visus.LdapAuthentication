// <copyright file="LdapConnectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Extension methods for <see cref="LdapConnection"/>.
    /// </summary>
    public static class LdapConnectionExtensions {

        /// <summary>
        /// Performs a paged LDAP search using <paramref name="that"/>.
        /// </summary>
        /// <param name="that">The <see cref="LdapConnection"/> to be used
        /// for the search.</param>
        /// <param name="base">The base DN to start the search at.</param>
        /// <param name="scope">The search scope.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="attrs">The list of LDAP attributes to be loaded for
        /// the search results.</param>
        /// <param name="pageSize">The size of the result pages in number of
        /// entries.</param>
        /// <param name="sortingAttribute">The attribute used to sort the
        /// results. See
        /// <see cref="AddPaging(LdapSearchConstraints, int, int, string)"/> for
        /// more details on the sort key.</param>
        /// <param name="timeLimit">A time limit for the search. If this
        /// parameter is <see cref="TimeSpan.Zero"/> or less, no timeout will
        /// be used.</param>
        /// <returns>The entries matching the specified search parameters.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="pageSize"/>
        /// is less than 1.</exception>
        public static IEnumerable<SearchResultEntry> PagedSearch(
                this LdapConnection that, string @base, SearchScope scope,
                string filter, string[] attrs, int pageSize,
                string sortingAttribute, TimeSpan timeLimit) {
            _ = that ?? throw new ArgumentNullException(nameof(that));

            var request = new SearchRequest(@base, filter, scope, attrs);
            var reqControl = request.AddPaging(pageSize, sortingAttribute);

            do {
                var results = (timeLimit > TimeSpan.Zero)
                    ? that.SendRequest(request, timeLimit)
                    : that.SendRequest(request);

                if (results is SearchResponse s) {
                    foreach (SearchResultEntry e in s.Entries) {
                        yield return e;
                    }
                }

                var control = (from c in results.Controls
                               let p = c as PageResultResponseControl
                               where (p != null)
                               select p).Single();
                reqControl.Cookie = control.Cookie;
            } while (reqControl.Cookie.Length > 0) ;
        }
    }
}
