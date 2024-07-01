// <copyright file="ILdapUserMapper.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Defines the interface of a mapper for <typeparamref name="TUser"/> and
    /// <typeparamref name="TGroup" />, which provides the
    /// <see cref="ILdapAuthenticationService"/> and the
    /// <see cref="ILdapSearchService"/> access to well-known properties.
    /// </summary>
    /// <typeparam name="TUser">The type used to represent an LDAP user.
    /// </typeparam>
    /// <typeparam name="TGroup">The type used to represent an LDAP group.
    /// </typeparam>
    public interface ILdapMapper<TUser, TGroup> {

        #region Public properties
        /// <summary>
        /// Gets the attributes that must be loaded for each
        /// <see cref="SearchResultEntry"/> in order to fill all properties of a
        /// <typeparamref name="TGroup"/> object.
        /// </summary>
        IEnumerable<string> RequiredGroupAttributes { get; }

        /// <summary>
        /// Gets the attributes that must be loaded for each
        /// <see cref="SearchResultEntry"/> in order to fill all properties of a
        /// <typeparamref name="TUser"/> object.
        /// </summary>
        IEnumerable<string> RequiredUserAttributes { get; }
        #endregion

        /// <summary>
        /// Assign the properties of the given LDAP <paramref name="entry"/>
        /// and its group claims to the given <paramref name="user"/> object.
        /// </summary>
        /// <param name="user">The user object to assign the LDAP attributes to.
        /// </param>
        /// <param name="entry">The entry to retrieve the values for the
        /// properties from.</param>
        /// <param name="connection">An <see cref="LdapConnection"/> to retrieve
        /// information about the group claims.</param>
        /// <param name="logger">An optional logger to record errors.</param>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="user"/> is <c>null</c>,
        /// or if <paramref name="entry"/> is <c>null</c>,
        /// or if <paramref name="connection"/> is <c>null</c>.</exception>
        void Assign(TUser user, SearchResultEntry entry,
            LdapConnection connection, ILogger logger);

        /// <summary>
        /// Gets, if any, the groups that <paramref name="user"/> belongs to.
        /// </summary>
        /// <param name="user">The user to obtain the groups for.</param>
        /// <returns>The list of groups the user belongs to.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="user"/> is <c>null</c>.</exception>
        IEnumerable<TGroup> GetGroups(TUser user);

        /// <summary>
        /// Gets the identity string of from the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to retrieve the identity property from.
        /// </param>
        /// <returns>The identity string.</returns>
        string GetIdentity(TUser user);

        /// <summary>
        /// Gets the identity string from the given LDAP
        /// <paramref name="entry"/>.
        /// </summary>
        /// <param name="entry">The LDAP entry to retrieve the identity
        /// attribute from.</param>
        /// <returns>The identity string.</returns>
        string GetIdentity(SearchResultEntry entry);
    }
}
