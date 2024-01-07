// <copyright file="SearchBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Specifies the base DN and <see cref="SearchScope"/> of one search
    /// location in <see cref="ILdapOptions"/>.
    /// </summary>
    public sealed class SearchBase {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        public SearchBase() : this(string.Empty, SearchScope.Subtree) { }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="dn">The distinguished name of the location where the
        /// search should begin.</param>
        /// <param name="scope">The scope of the search. This parameter defaults
        /// to <see cref="SearchScope.Sub"/>.</param>
        public SearchBase(string dn, SearchScope scope = SearchScope.Subtree) {
            this.DistinguishedName = dn
                ?? throw new ArgumentNullException(nameof(dn));
            this.Scope = scope;
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets or sets the distinguished name of the location where the search
        /// should begin.
        /// </summary>
        public string DistinguishedName { get; set; }

        /// <summary>
        /// Gets or sets the scope of the search.
        /// </summary>
        public SearchScope Scope { get; set; }
        #endregion
    }
}
