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
        /// Retrieves all claims as in <see cref="GetClaims(TUser)"/> and, if
        /// the <typeparamref name="TUser"/> has the ability to store claims,
        /// assigns the claims to the user object.
        /// </summary>
        /// <param name="user">The user to get the claims for and to assign it
        /// to.</param>
        /// <returns><paramref name="user"/>.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="user"/> is <c>null</c>.</exception>
        TUser AddClaims(TUser user);

        /// <summary>
        /// Retrieves all claims for the given <paramref name="user"/> and
        /// possibly the groups assigned to the user.
        /// </summary>
        /// <param name="user">The user to obtain the claims for.</param>
        /// <returns>The security <see cref="Claim"/>s of the given
        /// <paramref name="user"/>.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="user"/> is <c>null</c>.</exception>
        IEnumerable<Claim> GetClaims(TUser user);
    }
}
