// <copyright file="IClaimsBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;
using System.Security.Claims;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// The interface for a service class that creates <see cref="Claim"/>s for
    /// a given <typeparamref name="TUser"/> and his groups.
    /// </summary>
    /// <typeparam name="TEntry">The type of the LDAP entry, which is dependent
    /// on the underlying library.</typeparam>
    /// <typeparam name="TUser">The type of the user to create the claims for.
    /// </typeparam>
    /// <typeparam name="TGroup">The type of the group to create the claims for.
    /// </typeparam>
    public interface IClaimsBuilder<TEntry, TUser, TGroup> {

        #region Public properties
        /// <summary>
        /// Gets whether the builder supports direct mapping of LDAP entries to
        /// claims or whether it requires and intermediate step via
        /// <typeparamref name="TUser"/> and <see cref="TGroup"/>.
        /// </summary>
        bool SupportsDirectBuild { get; }
        #endregion

        #region Public methods
        /// <summary>
        /// Retrieves all claims for the given <paramref name="user"/> and his
        /// <paramref name="groups"/> directly from the LDAP
        /// <see cref="TEntry"/>.
        /// </summary>
        /// <remarks>
        /// Note to implementors: Implementations that cannot perform direct
        /// mapping from LDAP entries to claims should not throw here, but rather
        /// return an empty collection.
        /// </remarks>
        /// <param name="user">The entry representing the user.</param>
        /// <param name="groups">The entries representing the groups.</param>
        /// <param name="filter">An optional <see cref="ClaimFilter"/> that
        /// decides on whether a specific claim is included in the return
        /// value or not. This parameter can be <c>null</c>, in which case all
        /// claims pass the filter.</param>
        /// <returns>The security <see cref="Claim"/>s of the given
        /// <paramref name="user"/> or an empty enumeration if
        /// <see cref="SupportsDirectBuild"/> is <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="user"/> is <c>null</c>, or if
        /// <paramref name="groups"/> is <c>null</c>.</exception>
        IEnumerable<Claim> GetClaims(TEntry user,
            IEnumerable<TEntry> groups,
            ClaimFilter? filter = null);

        /// <summary>
        /// Retrieves all claims for the given <paramref name="group"/> and
        /// possibly the groups the given one is member of.
        /// </summary>
        /// <param name="group">The group to get the claims for.</param>
        /// <returns>The security <see cref="Claim"/>s of the given
        /// <paramref name="group"/>.</returns>
        /// <param name="filter">An optional <see cref="ClaimFilter"/> that
        /// decides on whether a specific claim is included in the return
        /// value or not. This parameter can be <c>null</c>, in which case all
        /// claims pass the filter.</param>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="group"/> is <c>null</c>.</exception>
        IEnumerable<Claim> GetClaims(TGroup group, ClaimFilter? filter = null);

        /// <summary>
        /// Retrieves all claims for the given <paramref name="user"/> and
        /// possibly the groups assigned to the user.
        /// </summary>
        /// <param name="user">The user to obtain the claims for.</param>
        /// <returns>The security <see cref="Claim"/>s of the given
        /// <paramref name="user"/>.</returns>
        /// <param name="filter">An optional <see cref="ClaimFilter"/> that
        /// decides on whether a specific claim is included in the return
        /// value or not. This parameter can be <c>null</c>, in which case all
        /// claims pass the filter.</param>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="user"/> is <c>null</c>.</exception>
        IEnumerable<Claim> GetClaims(TUser user, ClaimFilter? filter = null);
        #endregion
    }
}
