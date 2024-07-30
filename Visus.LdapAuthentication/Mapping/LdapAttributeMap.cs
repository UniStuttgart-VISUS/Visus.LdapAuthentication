// <copyright file="LdapAttributeMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using System;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;
using Visus.LdapAuthentication.Configuration;


namespace Visus.LdapAuthentication.Mapping {

    /// <summary>
    /// The reflection-based default implementation of
    /// <see cref="ILdapAttributeMap{TObject}"/>, which enumerates all
    /// properties of <typeparamref name="TObject"/> that have a
    /// <see cref="LdapAttributeAttribute"/> for the current
    /// <see cref="LdapOptionsBase.Schema"/>.
    /// </summary>
    /// <typeparam name="TObject">The object to be reflected.</typeparam>
    /// <param name="options">The LDAP configuration, which determines the
    /// LDAP schema to use.</param>
    /// <exception cref="ArgumentNullException">If
    /// <paramref name="options"/> is <c>null</c>.</exception>
    public sealed class LdapAttributeMap<TObject>(IOptions<LdapOptions> options)
        : LdapAttributeMapBase<TObject>(options?.Value!) { }
}
