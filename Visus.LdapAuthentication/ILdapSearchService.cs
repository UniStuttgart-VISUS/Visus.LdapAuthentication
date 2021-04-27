﻿// <copyright file="ILdapSearchService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Interface for a service allowing applications to search for users.
    /// </summary>
    /// <remarks>
    /// The search service allows an application to retrieve user information
    /// without binding as an end user. In order to perform the search, the
    /// credentials specified in <see cref="ILdapOptions"/> are used.
    /// </remarks>
    public interface ILdapSearchService {

        /// <summary>
        /// Gets a user with the specified value for the identity attribute.
        /// </summary>
        /// <param name="identity">The value of the identity attribute to
        /// be searched.</param>
        /// <returns>The user or <c>null</c> if no user matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="identity"/> is <c>null</c>.</exception>
        ILdapUser GetUserByIdentity(string identity);

        /// <summary>
        /// Gets all users from the directory that are in matching the search
        /// criteria configured in the <see cref="ILdapOptions"/> used by the
        /// application.
        /// </summary>
        /// <remarks>
        /// <para>This method creates <see cref="ILdapUser"/> object for all
        /// users matching the global search configuration, which might not only
        /// be a large results set, but also trigger a lot of additional LDAP
        /// searched in order to fill the group claims configured in the
        /// user object. Therefore, you should carefully design your LDAP user
        /// object in order to restrict the data that must be retrieved to the
        /// absolute minimum for the application case.</para>
        /// </remarks>
        /// <returns>All users in the directory matching the global search
        /// criteria.</returns>
        IEnumerable<ILdapUser> GetUsers();

        /// <summary>
        /// Gets all users from the directory that are matching the search
        /// criteria configured in the <see cref="ILdapOptions"/> used by the
        /// application <i>and</i> the specified LDAP <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        IEnumerable<ILdapUser> GetUsers(string filter);
    }
}
