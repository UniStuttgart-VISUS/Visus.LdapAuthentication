// <copyright file="ClaimFilter.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.Ldap.Claims {

    /// <summary>
    /// The type of a callback that filters the parameters of
    /// <see cref="System.Security.Claims.Claim"/> before the actual
    /// claim is constructed.
    /// </summary>
    /// <param name="name">The name of the claim.</param>
    /// <param name="value">The value of the claim.</param>
    /// <returns></returns>
    public delegate bool ClaimFilter(string name, string value);
}
