// <copyright file="LdapConnectionExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication.Configuration;


namespace Visus.DirectoryAuthentication.Extensions {

    /// <summary>
    /// Extension methods for <see cref="LdapConnection"/>.
    /// </summary>
    public static class LdapConnectionExtensions {

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
        public static async Task<string?> GetDefaultNamingContextAsync(
                this LdapConnection that) {
            var rootDse = await that.GetRootDseAsync("defaultNamingContext");
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
        public static SearchResultEntry? GetRootDse(this LdapConnection that,
                params string[] attributes) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            // Cf. https://stackoverflow.com/questions/19696753/how-does-one-connect-to-the-rootdse-and-or-retrieve-highestcommittedusn-with-sys
            var request = new SearchRequest(null,
                "(objectClass=*)",
                SearchScope.Base,
                attributes);
            var response = that.SendRequest(request);

            if (response is SearchResponse r) {
                return r.Entries.Cast<SearchResultEntry>().FirstOrDefault();
            } else {
                return null;
            }
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
        public static async Task<SearchResultEntry?> GetRootDseAsync(
                this LdapConnection that,
                params string[] attributes) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            // Cf. https://stackoverflow.com/questions/19696753/how-does-one-connect-to-the-rootdse-and-or-retrieve-highestcommittedusn-with-sys
            var request = new SearchRequest(null,
                "(objectClass=*)",
                SearchScope.Base,
                attributes);
            var response = await that.SendRequestAsync(request);

            if (response is SearchResponse r) {
                return r.Entries.Cast<SearchResultEntry>().FirstOrDefault();
            } else {
                return null;
            }
        }

        /// <summary>
        /// Performs a paged LDAP search using <paramref name="that"/>.
        /// </summary>
        /// <param name="that">The <see cref="LdapConnection"/> to be used
        /// for the search.</param>
        /// <param name="base">The base DN to start the search at.</param>
        /// <param name="scope">The search scope.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="attributes">The list of LDAP attributes to be loaded
        /// for the search results.</param>
        /// <param name="pageSize">The size of the result pages in number of
        /// entries.</param>
        /// <param name="sortingAttribute">The attribute used to sort the
        /// results. See
        /// <see cref="AddPaging(LdapSearchConstraints, int, int, string)"/> for
        /// more details on the sort key.</param>
        /// <param name="timeLimit">A time limit for the search. If this
        /// parameter is <see cref="TimeSpan.Zero"/> or less, no timeout will
        /// be used.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation between two pages.</param>
        /// <returns>The entries matching the specified search parameters.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="pageSize"/>
        /// is less than 1.</exception>
        public static IEnumerable<SearchResultEntry> PagedSearch(
                this LdapConnection that,
                string @base,
                SearchScope scope,
                string filter,
                string[] attributes,
                int pageSize,
                string sortingAttribute,
                TimeSpan timeLimit,
                CancellationToken cancellationToken) {
            _ = that ?? throw new ArgumentNullException(nameof(that));

            var request = new SearchRequest(@base, filter, scope, attributes);
            var reqControl = request.AddPaging(pageSize, sortingAttribute);

            do {
                cancellationToken.ThrowIfCancellationRequested();

                var results = timeLimit > TimeSpan.Zero
                    ? that.SendRequest(request, timeLimit)
                    : that.SendRequest(request);

                if (results is SearchResponse s) {
                    foreach (SearchResultEntry e in s.Entries) {
                        yield return e;
                    }
                }

                if (reqControl != null) {
                    var control = (from c in results.Controls
                                   let p = c as PageResultResponseControl
                                   where p != null
                                   select p).Single();
                    reqControl.Cookie = control.Cookie;
                }
            } while (reqControl?.Cookie?.Length > 0);
        }

        /// <summary>
        /// Performs an asynchronous paged LDAP search using
        /// <paramref name="that"/>.
        /// </summary>
        /// <param name="that">The <see cref="LdapConnection"/> to be used
        /// for the search.</param>
        /// <param name="base">The base DN to start the search at.</param>
        /// <param name="scope">The search scope.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="attributes">The list of LDAP attributes to be loaded
        /// for the search results.</param>
        /// <param name="pageSize">The size of the result pages in number of
        /// entries.</param>
        /// <param name="sortingAttribute">The attribute used to sort the
        /// results. See
        /// <see cref="AddPaging(LdapSearchConstraints, int, int, string)"/> for
        /// more details on the sort key.</param>
        /// <param name="timeLimit">A time limit for the search. If this
        /// parameter is <see cref="TimeSpan.Zero"/> or less, no timeout will
        /// be used.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns>The entries matching the specified search parameters.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="pageSize"/>
        /// is less than 1.</exception>
        public static async Task<IEnumerable<SearchResultEntry>>
        PagedSearchAsync(this LdapConnection that,
                string @base,
                SearchScope scope,
                string filter,
                string[] attributes,
                int pageSize,
                string sortingAttribute,
                TimeSpan timeLimit,
                CancellationToken cancellationToken = default) {
            _ = that ?? throw new ArgumentNullException(nameof(that));

            var request = new SearchRequest(@base, filter, scope, attributes);
            var reqControl = request.AddPaging(pageSize, sortingAttribute);
            var retval = Enumerable.Empty<SearchResultEntry>();

            do {
                cancellationToken.ThrowIfCancellationRequested();

                var task = timeLimit > TimeSpan.Zero
                    ? that.SendRequestAsync(request, timeLimit)
                    : that.SendRequestAsync(request);
                var results = await task.ConfigureAwait(false);

                if (results is SearchResponse s) {
                    retval = retval.Concat(s.Entries.Cast<SearchResultEntry>());
                }

                if (reqControl != null) {
                    var control = (from c in results.Controls
                                   let p = c as PageResultResponseControl
                                   where p != null
                                   select p).Single();
                    reqControl.Cookie = control.Cookie;
                }
            } while (reqControl?.Cookie?.Length > 0);

            return retval;
        }

        /// <summary>
        /// Performs the specified LDAP search on all search bases configured
        /// in <paramref name="options"/>.
        /// </summary>
        /// <param name="that">The connection used to perform the search.
        /// </param>
        /// <param name="filter">The LDAP filter selecting the entries to return.
        /// </param>
        /// <param name="attributes">The attributes to load for each entry.
        /// </param>
        /// <param name="options">The LDAP options, which determine the search
        /// bases and the search scope.</param>
        /// <returns>The entries matching the specified
        /// <paramref name="filter"/> in the scopes defined in
        /// <paramref name="options"/>.</returns>
        public static IEnumerable<SearchResultEntry> Search(
                this LdapConnection that,
                string filter,
                string[] attributes,
                LdapOptions options) {
            ArgumentNullException.ThrowIfNull(that, nameof(that));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            foreach (var b in options.SearchBases) {
                var req = new SearchRequest(b.Key, filter, b.Value, attributes);
                var res = that.SendRequest(req, options);

                if (res is SearchResponse r) {
                    var entries = r.Entries.OfType<SearchResultEntry>();
                    foreach (var e in entries) {
                        yield return e;
                    }
                }
            }
        }

        /// <summary>
        /// Performs the specified LDAP search on all search bases configured
        /// in <paramref name="options"/>.
        /// </summary>
        /// <param name="that">The connection used to perform the search.
        /// </param>
        /// <param name="filter">The LDAP filter selecting the entries to return.
        /// </param>
        /// <param name="attributes">The attributes to load for each entry.
        /// </param>
        /// <param name="options">The LDAP options, which determine the search
        /// bases and the search scope.</param>
        /// <returns>The entries matching the specified
        /// <paramref name="filter"/> in the scopes defined in
        /// <paramref name="options"/>.</returns>
        public static async Task<IEnumerable<SearchResultEntry>> SearchAsync(
                this LdapConnection that,
                string filter,
                string[] attributes,
                LdapOptions options) {
            ArgumentNullException.ThrowIfNull(that, nameof(that));
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            var retval = new List<SearchResultEntry>();

            foreach (var b in options.SearchBases) {
                var req = new SearchRequest(b.Key, filter, b.Value, attributes);
                var res = await that.SendRequestAsync(req, options);

                if (res is SearchResponse r) {
                    var entries = r.Entries.OfType<SearchResultEntry>();
                    retval.AddRange(entries);
                }
            }

            return retval;
        }

        /// <summary>
        /// Sends a request with a timeout, provided the given
        /// <see cref="LdapOptions.Timeout"/> is greater than zero. Otherwise,
        /// send the request without a timeout.
        /// </summary>
        /// <param name="that">The connection to send the request to.</param>
        /// <param name="request">The request to send.</param>
        /// <param name="options">The options object specifying the potential
        /// timeout.</param>
        /// <returns>The response from the server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DirectoryResponse SendRequest(
                this LdapConnection that,
                DirectoryRequest request,
                LdapOptions options) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            return options?.Timeout > TimeSpan.Zero
                ? that.SendRequest(request, options.Timeout)
                : that.SendRequest(request);
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
                this LdapConnection that,
                DirectoryRequest request,
                TimeSpan timeout) {
            ArgumentNullException.ThrowIfNull(that, nameof(that));

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
        /// provided via <see cref="LdapOptions.Timeout"/>.
        /// </summary>
        /// <param name="that">The connection to send the request to.</param>
        /// <param name="request">The request to send.</param>
        /// <param name="options">The options object specifying the potential
        /// timeout.</param>
        /// <returns>The response from the server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<DirectoryResponse> SendRequestAsync(
                this LdapConnection that,
                DirectoryRequest request,
                LdapOptions options) {
            return that.SendRequestAsync(request,
                options?.Timeout ?? TimeSpan.Zero);
        }

        /// <summary>
        /// Sends an asynchronous request without a timeout.
        /// </summary>
        /// <param name="that">The connection to send the request to.</param>
        /// <param name="request">The request to send.</param>
        /// <param name="options">The options object specifying the potential
        /// timeout.</param>
        /// <returns>The response from the server.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<DirectoryResponse> SendRequestAsync(
                this LdapConnection that,
                DirectoryRequest request) {
            return that.SendRequestAsync(request, TimeSpan.Zero);
        }
    }
}
