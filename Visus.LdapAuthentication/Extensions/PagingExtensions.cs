// <copyright file="PagingExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Controls;
using System;
using System.Diagnostics;
using System.Linq;


namespace Visus.LdapAuthentication.Extensions {

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
        internal static int? GetTotalCount(this ILdapSearchResults that) {
            _ = that ?? throw new ArgumentNullException(nameof(that));

            if (that.ResponseControls != null) {
                var r = (from c in that.ResponseControls
                         let d = c as LdapVirtualListResponse
                         where d != null
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
        internal static LdapControl GetVirtualListControl(int currentPage,
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
    }
}
