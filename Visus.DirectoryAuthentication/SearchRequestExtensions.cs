// <copyright file="SearchRequestExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2022 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Extension methods for <see cref="SearchRequest"/>.
    /// </summary>
    public static class SearchRequestExtensions {

        /// <summary>
        /// Adds paging controls to the given <see cref="SearchRequest"/>.
        /// </summary>
        /// <param name="that">The request to add the paging controls to.
        /// </param>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortKey"></param>
        /// <returns></returns>
        public static PageResultRequestControl AddPaging(
                this SearchRequest that,
                int pageSize,
                string sortKey = "distinguishedName") {
            _ = that ?? throw new ArgumentNullException(nameof(that));

            that.Controls.Add(new SortRequestControl(sortKey, false));
            var retval = new PageResultRequestControl(pageSize);
            retval.IsCritical = false;
            that.Controls.Add(retval);

            return retval;
        }

        /// <summary>
        /// Replaces all attributes to be loaded by <paramref name="that"/> with
        /// the given attributes.
        /// </summary>
        /// <param name="that">The search request to set the attributes to load
        /// for.</param>
        /// <param name="attributes">The attributes to be loaded. All other
        /// attributes will be erased from
        /// <see cref="SearchRequest.Attributes"/>.</param>
        /// <returns><paramref name="that"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/> is
        /// <c>null</c>, or if <paramref name="attributes"/> is <c>null</c>.
        /// </exception>
        public static SearchRequest SetAttributes(this SearchRequest that,
                string[] attributes) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            that.Attributes.Clear();
            that.Attributes.AddRange(attributes);
            return that;
        }
    }
}
