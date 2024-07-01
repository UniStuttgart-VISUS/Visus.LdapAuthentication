// <copyright file="IClaimsBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;
using System.Security.Claims;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// The interface for a service class that creates <see cref="Claim"/>s for
    /// a given <typeparamref name="TUser"/> and his groups.
    /// </summary>
    /// <typeparam name="TUser">The type of the user to create the claims for.
    /// </typeparam>
    public interface IClaimsBuilder<TUser, TGroup> {

        /// <summary>
        /// Retrieves all claims for the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to obtain the claims for.</param>
        /// <returns>The security <see cref="Claim"/>s of the given
        /// <paramref name="user"/>.</returns>
        IEnumerable<Claim> Build(TUser user);


        IClaimsBuilder<TUser, TGroup> UseMapper(
            ILdapMapper<TUser, TGroup> mapper);

    }
}
