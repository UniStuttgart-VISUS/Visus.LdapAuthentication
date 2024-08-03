// <copyright file="IAttributeClaimsBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Visus.Ldap.Mapping;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// Allows for attaching one or more <see cref="Claim"/>s to an
    /// <see cref="LdapAttributeAttribute"/>.
    /// </summary>
    public interface IAttributeClaimsMapBuilder {

        /// <summary>
        /// Adds the given claim to the attribute.
        /// </summary>
        /// <param name="claim">The type of the claim to be added.</param>
        /// <returns><c>this</c>.</returns>
        /// <exception cref="ArgumentException">If <paramref name="claim"/>
        /// is <c>null</c> or an empty string.</exception>
        IAttributeClaimsMapBuilder ToClaim(string claim);

        /// <summary>
        /// Adds the given claims to the attribute.
        /// </summary>
        /// <param name="claims">A list of claims to be added.</param>
        /// <returns><c>this</c>.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="claims"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If any of the
        /// <paramref name="claims"/> is <c>null</c> or an empty string.
        /// </exception>
        IAttributeClaimsMapBuilder ToClaims(IEnumerable<string> claims);

        /// <summary>
        /// Adds the given claims to the attribute.
        /// </summary>
        /// <param name="claims">A list of claims to be added.</param>
        /// <returns><c>this</c>.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="claims"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If any of the
        /// <paramref name="claims"/> is <c>null</c> or an empty string.
        /// </exception>
        IAttributeClaimsMapBuilder ToClaims(params string[] claims)
            => this.ToClaims(claims.AsEnumerable());
    }
}
