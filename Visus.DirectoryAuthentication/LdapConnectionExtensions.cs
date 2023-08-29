// <copyright file="LdapConnectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;


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
        /// <param name="timeLimit">A time limit for the search. This parameter
        /// defaults to zero, which indicates an unlimited amount of search
        /// time.</param>
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

            var cntRead = 0;            // Number of entries already read.
            int? cntTotal = null;       // Total number of entries to be read.
            byte[] cookie = null;       // The cookie for the next page.
            SearchResultEntry entry;    // Current entry to be emitted.

            do {
                var request = new SearchRequest(@base, filter, scope, attrs);

                if (cookie == null) {
                    request.AddPaging(pageSize, sortingAttribute);
                } else {
                    request.AddPaging(cookie, sortingAttribute);
                }


                //var constraints = new LdapSearchConstraints() {
                //    TimeLimit = timeLimit
                //};
                //constraints.AddPaging(curPage, pageSize, sortingAttribute);

                //var results = that.Search(@base, scope, filter, attrs, false,
                //    constraints);
                var results = that.SendRequest(request, timeLimit)
                    as SearchResponse;


                //while (results.HasMore()
                //        && ((cntTotal == null) || (cntRead < cntTotal))) {
                //    ++cntRead;

                //    try {
                //        entry = results.Next();
                //    } catch (LdapReferralException) {
                //        continue;
                //    }

                //    yield return entry;
                //}

                //++curPage;
                //cntTotal = results.GetTotalCount();
                yield break;
            } while ((cntTotal != null) && (cntRead < cntTotal));
        }
    }
}
