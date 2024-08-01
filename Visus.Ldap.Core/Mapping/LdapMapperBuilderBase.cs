// <copyright file="LdapMapperBuilderBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// Base implementation of a fluent
    /// <see cref="ILdapMapperBuilder{TEntry, TUser, TGroup}"/>, which configures
    /// everything except for the final build of the mapper.
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TGroup"></typeparam>
    public abstract class LdapMapperBuilderBase<TEntry, TUser, TGroup>
            : ILdapMapperBuilder<TEntry, TUser, TGroup> {

        #region Public methods
        /// <inheritdoc />
        public abstract ILdapMapper<TEntry, TUser, TGroup> Build();

        /// <inheritdoc />
        public ILdapPropertyMappingBuilder MapGroupProperty(
                string propertyName)
            => this._groupMap.MapProperty(propertyName, this._schema);

        /// <inheritdoc />
        public ILdapPropertyMappingBuilder MapUserProperty(
                string propertyName)
            => this._userMap.MapProperty(propertyName, this._schema);
        #endregion

        #region Protected constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="schema">The schema the mapping is intended for.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="schema"/>
        /// is <c>null.</c></exception>
        protected LdapMapperBuilderBase(string schema) => this._schema = schema
            ?? throw new ArgumentNullException(nameof(schema));
        #endregion

        #region Protected properties
        /// <summary>
        /// Gets the map for <see cref="TGroup"/>.
        /// </summary>
        protected ILdapAttributeMap<TGroup> GroupMap => this._groupMap;

        /// <summary>
        /// Gets the map for <see cref="TUser"/>.
        /// </summary>
        protected ILdapAttributeMap<TUser> UserMap => this._userMap;
        #endregion

        #region Private fields
        private readonly FluentLdapAttributeMap<TGroup> _groupMap = new();
        private readonly string _schema;
        private readonly FluentLdapAttributeMap<TUser> _userMap = new();
        #endregion
    }
}
