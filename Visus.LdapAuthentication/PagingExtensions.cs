// <copyright file="PagingExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Extension methods for <see cref="LdapSearchConstraints"/> and
    /// <see cref="LdapSearchResults"/> that make paging LDAP search results
    /// more convenient..
    /// </summary>
    public static class PagingExtensions {

        /// <summary>
        /// Adds paging constraints to the given
        /// <see cref="LdapSearchConstraints"/>.
        /// </summary>
        /// <remarks>
        /// <para>This is a convenience method configuring the search
        /// constraints such that subsequent pages of
        /// <paramref name="pageSize"/> are returned. The first page starts
        /// at index 0.</para>
        /// <para>The method also adds a sort control using the specified
        /// <paramref name="sortKey"/>, which is required for LDAP paging to
        /// succeed. The sort key should be unique. If not specified, the
        /// method will use &quot;distinguishedName&quot;. See
        /// https://stackoverflow.com/questions/55208799/page-ldap-query-against-ad-in-net-core-using-novell-ldap
        /// for more details.</para>
        /// </remarks>
        /// <param name="that">The <see cref="LdapSearchConstraints"/> to
        /// add the constraints to.</param>
        /// <param name="currentPage">The index of the current page. It is
        /// assumed that all pages share the same size
        /// <paramref name="pageSize"/>.</param>
        /// <param name="pageSize">The size of a single page in number of
        /// elements to be returned. For an Active Directory, 1000 is a
        /// reasonable number.</param>
        /// <param name="sortKey">The sort key determining the order. This
        /// value defaults to &quot;distinguishedName&quot;.</param>
        /// <returns><paramref name="that"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If
        /// <paramref name="currentPage"/> is negative.</exception>
        /// <exception cref="ArgumentException">If <paramref name="pageSize"/>
        /// is less than 1.</exception>
        public static LdapSearchConstraints AddPaging(
                this LdapSearchConstraints that,
                int currentPage,
                int pageSize,
                string sortKey = "distinguishedName") {
            _ = that ?? throw new ArgumentNullException(nameof(that));

            that.SetControls(new[] {
                new LdapSortControl(new LdapSortKey(sortKey), true),
                GetVirtualListControl(currentPage, pageSize)
            });

            return that;
        }

        /// <summary>
        /// Gets the total number of elements in a paged search if a
        /// <see cref="LdapVirtualListResponse"/> is present in the results.
        /// </summary>
        /// <param name="that">The search results to determine the total number
        /// of results available for.</param>
        /// <returns>The total number of results in case the results are paged,
        /// <c>null</c> if no <see cref="LdapVirtualListResponse"/> was in the
        /// results and therefore all results are contained in
        /// <paramref name="that"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        private static int? GetTotalCount(this ILdapSearchResults that) {
            _ = that ?? throw new ArgumentNullException(nameof(that));

            if (that.ResponseControls != null) {
                var r = (from c in that.ResponseControls
                         let d = c as LdapVirtualListResponse
                         where (d != null)
                         select (LdapVirtualListResponse) c).SingleOrDefault();
                if (r != null) {
                    Debug.WriteLine($"Paging result: {r.ResultCode}");
                    return r.ContentCount;
                }
            }

            return null;
        }

        /// <summary>
        /// Get an <see cref="LdapVirtualListControl"/> for paging starting
        /// at <paramref name="currentPage"/> with a page size of
        /// <paramref name="pageSize"/>.
        /// </summary>
        /// <param name="currentPage">The starting page.</param>
        /// <param name="pageSize">The number of elements per page. For an
        /// Active Directory, 1000 is a reasonable number.</param>
        /// <returns>The list control that can be passed to
        /// <see cref="LdapSearchConstraints"/> to perform the paging.</returns>
        /// <exception cref="ArgumentException">If
        /// <paramref name="currentPage"/> is negative.</exception>
        /// <exception cref="ArgumentException">If <paramref name="pageSize"/>
        /// is less than 1.</exception>
        public static LdapControl GetVirtualListControl(int currentPage,
                int pageSize) {
            if (currentPage < 0) {
                throw new ArgumentException(Properties.Resources.ErrorLdapPage,
                    nameof(currentPage));
            }
            if (pageSize <= 0) {
                throw new ArgumentException(
                    Properties.Resources.ErrorLdapPageSize, nameof(pageSize));
            }

            var index = currentPage * pageSize + 1;
            var before = 0;
            var after = pageSize - 1;
            var count = 0;
            Debug.WriteLine($"LdapVirtualListControl({index}, {before}, "
                + $"{after}, {count}) = {before}:{after}:{index}:{count}");
            return new LdapVirtualListControl(index, before, after, count);
        }

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
        public static IEnumerable<LdapEntry> PagedSearch(
                this LdapConnection that, string @base, SearchScope scope,
                string filter, string[] attrs, int pageSize,
                string sortingAttribute, int timeLimit = 0) {
            _ = that ?? throw new ArgumentNullException(nameof(that));

            var cntRead = 0;        // Number of entries already read.
            int? cntTotal = null;   // Total number of entries to be read.
            var curPage = 0;        // Current page.
            LdapEntry entry;        // Current entry to be emitted.

            do {
                var constraints = new LdapSearchConstraints() {
                    TimeLimit = timeLimit
                };
                constraints.AddPaging(curPage, pageSize, sortingAttribute);

                var results = that.Search(@base, scope, filter, attrs, false,
                    constraints);

                while (results.HasMore()
                        && ((cntTotal == null) || (cntRead < cntTotal))) {
                    ++cntRead;

                    try {
                        entry = results.Next();
                    } catch (LdapReferralException) {
                        continue;
                    }

                    yield return entry;
                }

                ++curPage;
                cntTotal = results.GetTotalCount();
            } while ((cntTotal != null) && (cntRead < cntTotal));
        }

    }
}
