// <copyright file="SearchBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2023 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


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
        public SearchBase() : this(SearchScope.Sub) {
            this.DistinguishedName = string.Empty;
        }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="dn">The distinguished name of the location where the
        /// search should begin.</param>
        /// <param name="scope">The scope of the search. This parameter defaults
        /// to <see cref="SearchScope.Sub"/>.</param>
        public SearchBase(string dn, SearchScope scope = SearchScope.Sub) {
            this.DistinguishedName = dn ?? string.Empty;
            this.Scope = scope;
        }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="scope">The scope of the search</param>
        public SearchBase(SearchScope scope) : this(string.Empty, scope) { }
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
