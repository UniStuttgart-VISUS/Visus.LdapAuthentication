// <copyright file="LdapConnectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Visus.LdapAuthentication.Configuration;


namespace Visus.LdapAuthentication.Extensions {

    /// <summary>
    /// Extension methods for <see cref="LdapConnection"/>.
    /// </summary>
    internal static class LdapConnectionExtensions {

        /// <summary>
        /// Gets the default naming context of the server which
        /// <paramref name="that"/> is connected to.
        /// </summary>
        /// <remarks>
        /// The default naming context is obtained from the root DSE of the
        /// given connection. It can be used as the fallback for search bases
        /// as it usually designates the root location of the directory.
        /// </remarks>
        /// <param name="that">The connection to get the default naming context
        /// of.</param>
        /// <returns>The default naming context or <c>null</c>.</returns>
        public static string? GetDefaultNamingContext(this LdapConnection that) {
            var rootDse = that.GetRootDse("defaultNamingContext");
            return rootDse?.GetAttribute("defaultNamingContext")?.ToString();
        }

        /// <summary>
        /// Gets the root DSE for the given connection.
        /// </summary>
        /// <param name="that">The connection from which to get the root DSE.
        /// </param>
        /// <param name="attributes">An optional list of attributes to load
        /// for the root DSE.</param>
        /// <returns>The entry of the root DSE.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        public static LdapEntry? GetRootDse(this LdapConnection that,
                params string[] attributes) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            // Cf. https://stackoverflow.com/questions/19696753/how-does-one-connect-to-the-rootdse-and-or-retrieve-highestcommittedusn-with-sys
            var result = that.Search(string.Empty,
                SearchScope.Base,
                "(objectClass=*)",
                attributes,
                false);

            if (result.HasMore()) {
                var retval = result.NextEntry();
                if (retval != null) {
                    return retval;
                }
            }

            return null;
        }

        /// <summary>
        /// Performs a paged LDAP search using <paramref name="that"/>.
        /// </summary>
        /// <remarks>
        /// This method performs a &quot;normal&quot;, non-paged search if
        /// <paramref name="pageSize"/> is zero or less. This allows users to
        /// disable paging on servers that do not support this control.
        /// </remarks>
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
        /// <param name="logger">An optional logger for recording exception that
        /// are encountered during the paged search. It is safe to pass
        /// <c>null</c>, in which case errors will be silently ignored.</param>
        /// <returns>The entries matching the specified search parameters.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="pageSize"/>
        /// is less than 1.</exception>
        public static IEnumerable<LdapEntry> PagedSearch(
                this LdapConnection that, string @base, SearchScope scope,
                string filter, string[] attrs, int pageSize,
                string sortingAttribute, int timeLimit = 0,
                ILogger logger = null) {
            _ = that ?? throw new ArgumentNullException(nameof(that));

            var cntRead = 0;        // Number of entries already read.
            int? cntTotal = null;   // Total number of entries to be read.
            var curPage = 0;        // Current page.
            LdapEntry entry;        // Current entry to be emitted.

            do {
                var constraints = new LdapSearchConstraints() {
                    TimeLimit = timeLimit
                };

                if (pageSize > 0) {
                    constraints.AddPaging(curPage, pageSize, sortingAttribute);
                }

                var results = that.Search(@base, scope, filter, attrs, false,
                    constraints);

                while (results.HasMore()
                        && (cntTotal == null || cntRead < cntTotal)) {
                    ++cntRead;

                    try {
                        entry = results.Next();
                    } catch (LdapReferralException) {
                        continue;
                    } catch (LdapException ex) {
                        logger.LogWarning(ex.Message, ex);
                        continue;
                    }

                    yield return entry;
                }

                ++curPage;
                // 'cntTotal' should remain null if the response does not
                // contain a paging control, in which case we will leave the
                // loop immediately.
                cntTotal = results.GetTotalCount();
            } while (cntTotal != null && cntRead < cntTotal);
        }

        /// <summary>
        /// Performs an LDAP search while converting our strongly typed
        /// <see cref="SearchScope"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="that"></param>
        /// <param name="base"></param>
        /// <param name="scope"></param>
        /// <param name="filter"></param>
        /// <param name="attributes"></param>
        /// <param name="typesOnly"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILdapSearchResults Search(this LdapConnection that,
                string @base,
                SearchScope scope,
                string filter,
                string[] attributes,
                bool typesOnly = false)
            => that.Search(@base,
                (int) scope,
                filter,
                attributes,
                typesOnly);

        /// <summary>
        /// Performs an LDAP search while converting our strongly typed
        /// <see cref="SearchScope"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="that"></param>
        /// <param name="base"></param>
        /// <param name="filter"></param>
        /// <param name="attributes"></param>
        /// <param name="typesOnly"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILdapSearchResults Search(this LdapConnection that,
                KeyValuePair<string, SearchScope> @base,
                string filter,
                string[] attributes,
                bool typesOnly = false)
            => that.Search(@base.Key,
                (int) @base.Value,
                filter,
                attributes,
                typesOnly);

        /// <summary>
        /// Performs an LDAP search while converting our strongly typed
        /// <see cref="SearchScope"/> to <see cref="int"/>.
        /// </summary>
        /// <param name="that"></param>
        /// <param name="base"></param>
        /// <param name="scope"></param>
        /// <param name="filter"></param>
        /// <param name="attributes"></param>
        /// <param name="typesOnly"></param>
        /// <param name="constraints"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ILdapSearchResults Search(this LdapConnection that,
                string @base,
                SearchScope scope,
                string filter,
                string[] attributes,
                bool typesOnly,
                LdapSearchConstraints constraints)
            => that.Search(@base,
                (int) scope,
                filter,
                attributes,
                typesOnly,
                constraints);
    }
}
