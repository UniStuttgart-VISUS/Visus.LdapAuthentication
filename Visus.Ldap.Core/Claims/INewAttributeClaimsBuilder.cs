// <copyright file="INewAttributeClaimsBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Security.Claims;
using Visus.Ldap.Mapping;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// Allows for attaching one or more <see cref="Claim"/>s to an
    /// <see cref="LdapAttributeAttribute"/>.
    /// </summary>
    public interface INewAttributeClaimsBuilder: IAttributeClaimsBuilder {

        /// <summary>
        /// Adds a converter of the specified type to the mapping.
        /// </summary>
        /// <param name="converter">The type of the converter, which must
        /// implement <see cref="IValueConverter"/>.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="converter"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="converter"/>
        /// does not implement <see cref="IValueConverter"/>.</exception>
        void WithConverter(Type converter);

        /// <summary>
        /// Adds a converter to the mapping.
        /// </summary>
        /// <typeparam name="TConverter">The type of the converter.</typeparam>
        void WithConverter<TConverter>() where TConverter : IValueConverter
            => this.WithConverter(typeof(TConverter));
    }
}
