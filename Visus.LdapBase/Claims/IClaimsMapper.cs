// <copyright file="IClaimsMapper.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;
using System.Security.Claims;


namespace Visus.Ldap.Claims {

    /// <summary>
    /// The interface for a service class that creates <see cref="Claim"/>s
    /// directly from an LDAP <typeparamref name="TEntry"/>.
    /// </summary>
    /// <remarks>
    /// Using an instance of this class may be more performant for
    /// authenticating users than creating user and group objects using
    /// <see cref="Mapping.ILdapMapper{TEntry, TUser, TGroup}"/> provided the
    /// authentication mechanism to be used is based on claims.
    /// </remarks>
    /// <typeparam name="TEntry">The type of the LDAP entry, which is dependent
    /// on the underlying library.</typeparam>
    public interface IClaimsMapper<TEntry> {

        #region Public properties
        /// <summary>
        /// Gets the attributes that must be loaded for each LDAP entry in order
        /// to fill all properties of a group entry.
        /// </summary>
        IEnumerable<string> RequiredGroupAttributes { get; }

        /// <summary>
        /// Gets the attributes that must be loaded for each LDAP entry in order
        /// to fill all properties of a user entry.
        /// </summary>
        IEnumerable<string> RequiredUserAttributes { get; }
        #endregion

        #region Public methods
        /// <summary>
        /// Retrieves all claims for the given <paramref name="user"/> and his
        /// <paramref name="groups"/> directly from the LDAP
        /// <see cref="TEntry"/>.
        /// </summary>
        /// <param name="user">The entry representing the user.</param>
        /// <param name="groups">The entries representing the groups.</param>
        /// <param name="filter">An optional <see cref="ClaimFilter"/> that
        /// decides on whether a specific claim is included in the return
        /// value or not. This parameter can be <c>null</c>, in which case all
        /// claims pass the filter.</param>
        /// <returns>The security <see cref="Claim"/>s of the given
        /// <paramref name="user"/>.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="user"/> is <c>null</c>, or if
        /// <paramref name="groups"/> is <c>null</c>.</exception>
        IEnumerable<Claim> GetClaims(TEntry user,
            IEnumerable<TEntry> groups,
            ClaimFilter? filter = null);
        #endregion
    }
}
