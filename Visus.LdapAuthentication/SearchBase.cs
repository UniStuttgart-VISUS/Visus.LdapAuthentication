// <copyright file="SearchBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


using System;

namespace Visus.LdapAuthentication {

    /// <summary>
    /// Specifies the base DN and <see cref="SearchScope"/> of one search
    /// location in <see cref="ILdapOptions"/>.
    /// </summary>
    /// <remarks>
    /// <para>This class is part of enhancement #7.</para>
    /// </remarks>
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
        /// to <see cref="SearchScope.Subtree"/>.</param>
        public SearchBase(string dn, SearchScope scope = SearchScope.Subtree) {
            this.DistinguishedName = dn
                ?? throw new ArgumentNullException(nameof(dn));
            this.Scope = scope;
        }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="dn">The distinguished name of the location where the
        /// search should begin.</param>
        /// <param name="sub">If <c>true</c>, set the scope of the search to
        /// <see cref="SearchScope.Subtree"/>, to <see cref="SearchScope.Base"/>
        /// otherwise.</param>
        public SearchBase(string dn, bool sub) {
            this.DistinguishedName = dn
                ?? throw new ArgumentNullException(nameof(dn));
            this.IsSubtree = sub;
        }

        ///// <summary>
        ///// Initialises a new instance.
        ///// </summary>
        ///// <param name="scope">The scope of the search</param>
        //public SearchBase(SearchScope scope) : this(string.Empty, scope) { }
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

        #region Internal properties
        /// <summary>
        /// Gets or sets whether the whole subtree should be searched.
        /// </summary>
        internal bool IsSubtree {
            get => (this.Scope == SearchScope.Subtree);
            set => this.Scope = value ? SearchScope.Subtree : SearchScope.Base;
        }
        #endregion
    }
}
