﻿// <copyright file="ILdapSearchService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2023 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Interface for a service allowing applications to search for users.
    /// </summary>
    /// <remarks>
    /// The search service allows an application to retrieve user information
    /// without binding as an end user. In order to perform the search, the
    /// credentials specified in <see cref="IOptions"/> are used.
    /// </remarks>
    public interface ILdapSearchService : IDisposable {

        /// <summary>
        /// Gets the distinguished names of the entries matching the specified
        /// LDAP <paramref name="filter"/>.
        /// </summary>
        /// <remarks>
        /// This method can be used if your directory requires users to bind
        /// using a distinguished name, but you do not want them to input this
        /// name, but a value of another LDAP attribute that is easier to
        /// remember.
        /// </remarks>
        /// <param name="filter">An LDAP filter expression.</param>
        /// <returns>The distinguished names of all entries in the directory
        /// matching the given search criteria.</returns>
        IEnumerable<string> GetDistinguishedNames(string filter);

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
        /// criteria configured in the <see cref="IOptions"/> used by the
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
        /// criteria configured in the <see cref="IOptions"/> used by the
        /// application <i>and</i> the specified LDAP <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        IEnumerable<ILdapUser> GetUsers(string filter);

        /// <summary>
        /// Gets all users from the directory that match the search criteria
        /// configured in <see cref="IOptions"/> used by the application
        /// <i>and</i> the specified LDAP <paramref name="filter"/> while
        /// overriding the search bases from the <see cref="IOptions"/>
        /// with the given ones.
        /// </summary>
        /// <param name="searchBases">The search bases to look in. It is safe
        /// to pass <c>null</c>, in which case the search bases from the
        /// <see cref="IOptions"/> will be used.</param>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        IEnumerable<ILdapUser> GetUsers(
            IDictionary<string, SearchScope> searchBases,
            string filter);

        /// <summary>
        /// Gets all users from the directory that are matching the search
        /// critiera configured in the <see cref="IOptions"/> used by the
        /// application <i>and</i> the specified LDAP <paramref cref="filter"/>
        /// while overriding the search base from the <see cref="IOptions"/>
        /// with the given one.
        /// </summary>
        /// <param name="searchBase">The search base to look in. It is safe to
        /// pass <c>null</c>, in which case the search base from the configured
        /// <see cref="IOptions"/> will be used.</param>
        /// <param name="searchScope">Overrides the search scope from the
        /// configured <see cref="IOptions"/>.</param>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        public IEnumerable<ILdapUser> GetUsers(string searchBase,
            SearchScope searchScope, string filter);

        /// <summary>
        /// Gets all users from the directory that are matching the search
        /// critiera configured in the <see cref="IOptions"/> used by the
        /// application <i>and</i> the specified LDAP <paramref cref="filter"/>
        /// while overriding the search base from the <see cref="IOptions"/>
        /// with the given one.
        /// </summary>
        /// <param name="searchBase">The search base to look in. It is safe to
        /// pass <c>null</c>, in which case the search base from the configured
        /// <see cref="IOptions"/> will be used.</param>
        /// <param name="searchScope">Overrides the search scope from the
        /// configured <see cref="IOptions"/>. This can be one of
        /// <see cref="Novell.Directory.Ldap.LdapConnection.ScopeBase"/>,
        /// <see cref="Novell.Directory.Ldap.LdapConnection.ScopeOne"/> and
        /// <see cref="Novell.Directory.Ldap.LdapConnection.ScopeSub"/>.</param>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        [Obsolete("Use the strongly typed variant of this method.")]
        public IEnumerable<ILdapUser> GetUsers(string searchBase,
            int searchScope, string filter);
    }


    /// <summary>
    /// A strongly typed variant of <see cref="ILdapSearchService"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of user that is to be retrieved from
    /// the directory.</typeparam>
    public interface ILdapSearchService<TUser> : ILdapSearchService
            where TUser : class, ILdapUser {

        /// <summary>
        /// Gets a user with the specified value for the identity attribute.
        /// </summary>
        /// <param name="identity">The value of the identity attribute to
        /// be searched.</param>
        /// <returns>The user or <c>null</c> if no user matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="identity"/> is <c>null</c>.</exception>
        new TUser GetUserByIdentity(string identity);

        /// <summary>
        /// Gets all users from the directory that are in matching the search
        /// criteria configured in the <see cref="IOptions"/> used by the
        /// application.
        /// </summary>
        /// <remarks>
        /// <para>This method creates <see cref="TUser"/> object for all
        /// users matching the global search configuration, which might not only
        /// be a large results set, but also trigger a lot of additional LDAP
        /// searched in order to fill the group claims configured in the
        /// user object. Therefore, you should carefully design your LDAP user
        /// object in order to restrict the data that must be retrieved to the
        /// absolute minimum for the application case.</para>
        /// </remarks>
        /// <returns>All users in the directory matching the global search
        /// criteria.</returns>
        new IEnumerable<TUser> GetUsers();

        /// <summary>
        /// Gets all users from the directory that are matching the search
        /// criteria configured in the <see cref="IOptions"/> used by the
        /// application <i>and</i> the specified LDAP <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        new IEnumerable<TUser> GetUsers(string filter);

        /// <summary>
        /// Gets all users from the directory that are matching the search
        /// critiera configured in the <see cref="IOptions"/> used by the
        /// application <i>and</i> the specified LDAP <paramref cref="filter"/>
        /// while overriding the search base from the <see cref="IOptions"/>
        /// with the given one.
        /// </summary>
        /// <param name="searchBases">The search bases to look in. It is safe
        /// to pass <c>null</c>, in which case the search bases from the
        /// <see cref="IOptions"/> will be used.</param>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        new IEnumerable<TUser> GetUsers(
            IDictionary<string, SearchScope> searchBases,
            string filter);
    }
}
