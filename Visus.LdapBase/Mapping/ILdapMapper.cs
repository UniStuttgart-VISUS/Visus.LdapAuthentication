// <copyright file="ILdapUserMapper.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


using System.Collections.Generic;

namespace Visus.Ldap.Mapping {

    /// <summary>
    /// Defines the interface of a mapper for mapping LDAP entries of
    /// type <typeparamref name="TEntry"/> to either 
    /// <typeparamref name="TUser" /> or <typeparamref name="TGroup" /> and
    /// provides the LDAP services access to well-known properties.
    /// </summary>
    /// <typeparam name="TEntry">The type of the LDAP entry, which is dependent
    /// on the underlying library.</typeparam>
    /// <typeparam name="TUser">The type used to represent an LDAP user.
    /// </typeparam>
    /// <typeparam name="TGroup">The type used to represent an LDAP group.
    /// </typeparam>
    public interface ILdapMapper<TEntry, TUser, TGroup> {

        #region Public properties
        /// <summary>
        /// Gets whether <typeparamref name="TGroup"/> has a property holding the
        /// group memberships of the group.
        /// </summary>
        /// <remarks>
        /// This information enables the users for the mapper to find out whether
        /// they need to query the directory to obtain the information about
        /// group membership before doing so.
        /// </remarks>
        bool GroupIsGroupMember { get; }

        /// <summary>
        /// Gets the attributes that must be loaded for each LDAP entry in order
        /// to fill all properties of a <typeparamref name="TGroup"/> object.
        /// </summary>
        string[] RequiredGroupAttributes { get; }

        /// <summary>
        /// Gets the attributes that must be loaded for each LDAP entry in order
        /// to fill all properties of a <typeparamref name="TUser"/> object.
        /// </summary>
        string[] RequiredUserAttributes { get; }

        /// <summary>
        /// Gets whether <typeparamref name="TUser"/> has a property holding the
        /// group memberships of the user.
        /// </summary>
        /// <remarks>
        /// This information enables the users for the mapper to find out whether
        /// they need to query the directory to obtain the information about
        /// group membership before doing so.
        /// </remarks>
        bool UserIsGroupMember { get; }
        #endregion

        #region Public methods
        /// <summary>
        /// Gets the account name of the given <paramref name="group"/>.
        /// </summary>
        /// <param name="group">The group to get the account name of.
        /// </param>
        /// <returns>The account name of <paramref name="group"/>.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="group"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">If
        /// <paramref name="group"/> does not have the requested attribute
        /// mapped to a property.</exception>
        string GetAccountName(TGroup group);

        /// <summary>
        /// Gets the account name of the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to get the account name of.
        /// </param>
        /// <returns>The account name of <paramref name="user"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="user"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">If
        /// <paramref name="user"/> does not have the requested attribute
        /// mapped to a property.</exception>
        string GetAccountName(TUser user);

        /// <summary>
        /// Gets the distinguished name of the given <paramref name="group"/>.
        /// </summary>
        /// <param name="group">The group to get the distinguished name of.
        /// </param>
        /// <returns>The distinguished name of <paramref name="group"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="group"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">If
        /// <paramref name="group"/> does not have the requested attribute
        /// mapped to a property.</exception>
        string GetDistinguishedName(TGroup group);

        /// <summary>
        /// Gets the distinguished name of the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to get the distinguished name of.
        /// </param>
        /// <returns>The distinguished name of <paramref name="user"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="user"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">If
        /// <paramref name="user"/> does not have the requested attribute
        /// mapped to a property.</exception>
        string GetDistinguishedName(TUser user);

        /// <summary>
        /// Gets the identity of the given <paramref name="group"/>.
        /// </summary>
        /// <param name="group">The group to get the identity of.</param>
        /// <returns>The identity of <paramref name="group"/>.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="group"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">If
        /// <paramref name="group"/> does not have the requested attribute
        /// mapped to a property.</exception>
        string GetIdentity(TGroup group);

        /// <summary>
        /// Gets the identity of the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to get the identity of.</param>
        /// <returns>The identity of <paramref name="user"/>.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="user"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">If
        /// <paramref name="user"/> does not have the requested attribute
        /// mapped to a property.</exception>
        string GetIdentity(TUser user);

        /// <summary>
        /// Assign the properties of the given LDAP <paramref name="entry"/> to
        /// the given <paramref name="group"/> object.
        /// </summary>
        /// <param name="entry">The LDAP entry to retrieve the attribute values
        /// from.</param>
        /// <param name="group">The group object to assign the LDAP attributes
        /// to.</param>
        /// <returns><paramref name="group"/>.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="group"/> is <c>null</c>,
        /// or if <paramref name="entry"/> is <c>null</c>.</exception>
        TGroup MapGroup(TEntry entry, TGroup group);

        /// <summary>
        /// Assign the properties of the given LDAP <paramref name="entry"/>
        /// and its group claims to the given <paramref name="user"/> object.
        /// </summary>
        /// <param name="entry">The entry to retrieve the values for the
        /// properties from.</param>
        /// <param name="user">The user object to assign the LDAP attributes to.
        /// </param>
        /// <returns><paramref name="user"/>.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="user"/> is <c>null</c>,
        /// or if <paramref name="entry"/> is <c>null</c>.</exception>
        TUser MapUser(TEntry entry, TUser user);

        /// <summary>
        /// Assigns the given <paramref name="groups"/> to the given
        /// <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to the the groups of.</param>
        /// <param name="primaryGroup">If non-<c>null</c>, mark the given
        /// group as the primary one and add it along with
        /// <paramref name="groups"/>. If <c>null</c>, this parameter
        /// will be ignored.</param>
        /// <param name="groups">The groups <paramref name="user"/> belongs
        /// to.</param>
        /// <returns><paramref name="user"/>.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="user"/> is <c>null</c>,
        /// or if <paramref name="groups"/> is <c>null</c>.</exception>
        TUser SetGroups(TUser user, TGroup? primaryGroup,
            IEnumerable<TGroup> groups);
        #endregion
    }
}
