// <copyright file="LdapAttributeMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using System;
using Visus.DirectoryAuthentication.Configuration;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Mapping {

    /// <summary>
    /// The reflection-based default implementation of
    /// <see cref="ILdapAttributeMap{TObject}"/>, which enumerates all
    /// properties of <typeparamref name="TObject"/> that have a
    /// <see cref="LdapAttributeAttribute"/> for the current
    /// <see cref="LdapOptionsBase.Schema"/>.
    /// </summary>
    /// <typeparam name="TObject">The object to be reflected.</typeparam>
    public sealed class LdapAttributeMap<TObject>
            : LdapAttributeMapBase<TObject> {

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP configuration, which determines the
        /// LDAP schema to use.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public LdapAttributeMap(IOptions<LdapOptions> options)
            : base(options?.Value!) { }

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="mapper">A callback that dynamically creates the mapping
        /// using a <see cref="ILdapAttributeMapBuilder{TObject}"/>.</param>
        /// <param name="options">The LDAP configuration, which determines the
        /// LDAP schema to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="mapper"/>
        /// is <c>null</c>, or if <paramref name="options"/> is <c>null</c>.
        /// </exception>
        internal LdapAttributeMap(Action<ILdapAttributeMapBuilder<TObject>,
                LdapOptionsBase> mapper, LdapOptionsBase options)
                : base(mapper, options) { }
    }
}
