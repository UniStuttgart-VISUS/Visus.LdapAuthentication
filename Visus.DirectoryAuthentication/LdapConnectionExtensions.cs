// <copyright file="LdapConnectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Threading.Tasks;


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
            } while (reqControl.Cookie.Length > 0);
        }

        /// <summary>
        /// Sends an asynchronous request, possibly with a timeout,
        /// provided the <paramref name="timeout"/> value is positive.
        /// </summary>
        /// <param name="that">The connection to send the request to.</param>
        /// <param name="request">The request to send.</param>
        /// <param name="timeout">The timeout, which has only an effect if it
        /// is positive.</param>
        /// <returns>The response from the server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        public static Task<DirectoryResponse> SendRequestAsync(
                this LdapConnection that, DirectoryRequest request,
                TimeSpan timeout) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            return Task.Factory.FromAsync(
               (cb, ctx) => {
                   if (timeout > TimeSpan.Zero) {
                       return that.BeginSendRequest(request, timeout,
                           PartialResultProcessing.NoPartialResultSupport,
                           cb, ctx);
                   } else {
                       return that.BeginSendRequest(request,
                           PartialResultProcessing.NoPartialResultSupport,
                           cb, ctx);
                   }
               }, that.EndSendRequest, null);
        }

        /// <summary>
        /// Sends an asynchronous request, possibly with a timeout
        /// provided via <see cref="ILdapOptions.Timeout"/>.
        /// </summary>
        /// <param name="that">The connection to send the request to.</param>
        /// <param name="request">The request to send.</param>
        /// <param name="options">The options object specifying the potential
        /// timeout.</param>
        /// <returns>The response from the server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        public static Task<DirectoryResponse> SendRequestAsync(
                this LdapConnection that, DirectoryRequest request,
                ILdapOptions options) {
            return that.SendRequestAsync(request,
                options?.Timeout ?? TimeSpan.Zero);
        }

        /// <summary>
        /// Sends a request with a timeout, provided the given
        /// <see cref="ILdapOptions.Timeout"/> is greater than zero. Otherwise,
        /// send the request without a timeout.
        /// </summary>
        /// <param name="that">The connection to send the request to.</param>
        /// <param name="request">The request to send.</param>
        /// <param name="options">The options object specifying the potential
        /// timeout.</param>
        /// <returns>The response from the server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        public static DirectoryResponse SendRequest(
                this LdapConnection that, DirectoryRequest request,
                ILdapOptions options) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            return (options?.Timeout > TimeSpan.Zero)
                ? that.SendRequest(request, options.Timeout)
                : that.SendRequest(request);
        }
    }
}
