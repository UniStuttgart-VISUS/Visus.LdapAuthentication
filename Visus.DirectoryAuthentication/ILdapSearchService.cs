// <copyright file="ILdapSearchService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Threading.Tasks;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Interface for a service allowing applications to search for users.
    /// </summary>
    /// <remarks>
    /// The search service allows an application to retrieve user information
    /// without binding as an end user. In order to perform the search, the
    /// credentials specified in <see cref="Configuration.LdapOptions"/> are
    /// used.
    /// </remarks>
    /// <typeparam name="TUser">The type of user that is to be retrieved from
    /// the directory.</typeparam>
    /// <typeparam name="TGroup">The type used to represent group memberships
    /// of <typeparamref name="TUser"/>.</typeparam>
    public interface ILdapSearchService<TUser, TGroup> : IDisposable
            where TUser : class {

        /// <summary>
        /// Gets a user with the specified distinguished name.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the user
        /// to look for.</param>
        /// <returns>The user or <c>null</c> if no user matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="distinguishedName"/> is <c>null</c>.</exception>
        TUser GetUserByDistinguishedName(string distinguishedName);

        /// <summary>
        /// Asynchronously gets a user with the specified distinguished name.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the user
        /// to look for.</param>
        /// <returns>The user or <c>null</c> if no user matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="distinguishedName"/> is <c>null</c>.</exception>
        Task<TUser> GetUserByDistinguishedNameAsync(string distinguishedName);

        /// <summary>
        /// Gets a user with the specified value for the identity attribute.
        /// </summary>
        /// <param name="identity">The value of the identity attribute to
        /// be searched.</param>
        /// <returns>The user or <c>null</c> if no user matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="identity"/> is <c>null</c>.</exception>
        TUser GetUserByIdentity(string identity);

        /// <summary>
        /// Asynchronously gets a user with the specified value for the identity
        /// attribute.
        /// </summary>
        /// <param name="identity">The value of the identity attribute to
        /// be searched.</param>
        /// <returns>The user or <c>null</c> if no user matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="identity"/> is <c>null</c>.</exception>
        Task<TUser> GetUserByIdentityAsync(string identity);

        /// <summary>
        /// Gets all users from the directory that are in matching the search
        /// criteria configured in the <see cref="Configuration.LdapOptions"/>
        /// used by the application.
        /// </summary>
        /// <remarks>
        /// <para>This method creates <typeparamref name="TUser" /> object for
        /// all users matching the global search configuration, which might not
        /// only be a large results set, but also trigger a lot of additional
        /// LDAP searched in order to fill the group claims configured in the
        /// user object. Therefore, you should carefully design your LDAP user
        /// object in order to restrict the data that must be retrieved to the
        /// absolute minimum for the application case.</para>
        /// </remarks>
        /// <returns>All users in the directory matching the global search
        /// criteria.</returns>
        IEnumerable<TUser> GetUsers();

        /// <summary>
        /// Asynchronously gets all users from the directory that are in
        /// matching the search criteria configured in the
        /// <see cref="Configuration.LdapOptions"/> used by the application.
        /// </summary>
        /// <remarks>
        /// <para>This method creates <typeparamref name="TUser" /> object for
        /// all users matching the global search configuration, which might not
        /// only be a large results set, but also trigger a lot of additional
        /// LDAP searched in order to fill the group claims configured in the
        /// user object. Therefore, you should carefully design your LDAP user
        /// object in order to restrict the data that must be retrieved to the
        /// absolute minimum for the application case.</para>
        /// </remarks>
        /// <returns>All users in the directory matching the global search
        /// criteria.</returns>
        Task<IEnumerable<TUser>> GetUsersAsync();

        /// <summary>
        /// Gets all users from the directory that are matching the search
        /// criteria configured in the <see cref="Configuration.LdapOptions"/>
        /// used by the application <i>and</i> the specified LDAP
        /// <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users. It is safe to pass
        /// <c>null</c>, in which case the additional filter criteria will
        /// be ignored.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        IEnumerable<TUser> GetUsers(string filter);

        /// <summary>
        /// Gets all users from the directory that are matching the search
        /// criteria configured in the <see cref="Configuration.LdapOptions"/>
        /// used by the application <i>and</i> the specified LDAP
        /// <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users. It is safe to pass
        /// <c>null</c>, in which case the additional filter criteria will
        /// be ignored.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        Task<IEnumerable<TUser>> GetUsersAsync(string filter);

        /// <summary>
        /// Gets all users from the directory that are matching the search
        /// critiera configured in the <see cref="Configuration.LdapOptions"/>
        /// used by the application <i>and</i> the specified LDAP
        /// <paramref cref="filter"/> while overriding the search bases from
        /// the <see cref="Configuration.LdapOptions"/> with the given one.
        /// </summary>
        /// <param name="searchBases">The search bases to look in. It is safe
        /// to pass <c>null</c>, in which case the search bases from the
        /// <see cref="Configuration.LdapOptions"/> will be used.</param>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users. It is safe to pass
        /// <c>null</c>, in which case the additional filter criteria will
        /// be ignored.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        IEnumerable<TUser> GetUsers(
            IDictionary<string, SearchScope> searchBases,
            string filter);

        /// <summary>
        /// Gets all users from the directory that are matching the search
        /// critiera configured in the <see cref="Configuration.LdapOptions"/>
        /// used by the application <i>and</i> the specified LDAP
        /// <paramref cref="filter"/> while overriding the search bases from
        /// the <see cref="Configuration.LdapOptions"/> with the given one.
        /// </summary>
        /// <param name="searchBases">The search bases to look in. It is safe
        /// to pass <c>null</c>, in which case the search bases from the
        /// <see cref="Configuration.LdapOptions"/> will be used.</param>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users. It is safe to pass
        /// <c>null</c>, in which case the additional filter criteria will
        /// be ignored.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        Task<IEnumerable<TUser>> GetUsersAsync(
            IDictionary<string, SearchScope> searchBases,
            string filter);
    }
}
