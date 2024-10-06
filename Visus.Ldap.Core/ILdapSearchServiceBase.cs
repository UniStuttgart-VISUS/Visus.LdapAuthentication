// <copyright file="ILdapSearchService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Visus.Ldap {

    /// <summary>
    /// Basic interface for a service to search for users and groups.
    /// </summary>
    /// <remarks>
    /// This base interface includes all methods that are not dependent on
    /// implementation details of the underlying LDAP library..
    /// </remarks>
    /// <typeparam name="TUser">The type of user that is to be retrieved from
    /// the directory.</typeparam>
    /// <typeparam name="TGroup">The type used to represent group memberships
    /// of <typeparamref name="TUser"/>.</typeparam>
    /// <typeparam name="TSearchScope">The type used to represent the search
    /// scope in the underlying library.</typeparam>
    public interface ILdapSearchServiceBase<TUser, TGroup, TSearchScope>
            : IDisposable where TSearchScope : struct, Enum {

        /// <summary>
        /// Gets a group with the specified distinguished name.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the group
        /// to look for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="distinguishedName"/> is <c>null</c>.</exception>
        TGroup? GetGroupByDistinguishedName(string distinguishedName);

        /// <summary>
        /// Asynchronously gets a group with the specified distinguished name.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the group
        /// to look for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="distinguishedName"/> is <c>null</c>.</exception>
        Task<TGroup?> GetGroupByDistinguishedNameAsync(string distinguishedName);

        /// <summary>
        /// Gets a group with the specified value for the identity attribute.
        /// </summary>
        /// <param name="identity">The value of the identity attribute to
        /// be searched.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="identity"/> is <c>null</c>.</exception>
        TGroup? GetGroupByIdentity(string identity);

        /// <summary>
        /// Asynchronously gets a group with the specified value for the
        /// identity attribute.
        /// </summary>
        /// <param name="identity">The value of the identity attribute to
        /// be searched.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="identity"/> is <c>null</c>.</exception>
        Task<TGroup?> GetGroupByIdentityAsync(string identity);

        /// <summary>
        /// Gets a group with the specified name.
        /// </summary>
        /// <param name="name">The name of the group to look for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="name"/> is <c>null</c>.</exception>
        TGroup? GetGroupByName(string name);

        /// <summary>
        /// Asynchronously gets a group with the specified name.
        /// </summary>
        /// <param name="name">The name of the group to look for.</param>
        /// <returns>The group or <c>null</c> if no group matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="name"/> is <c>null</c>.</exception>
        Task<TGroup?> GetGroupByNameAsync(string name);

        /// <summary>
        /// Retrieves the users in the given <paramref name="group"/>.
        /// </summary>
        /// <remarks>
        /// <para>The groups in <paramref name="group"/> will be recursively
        /// expanded if
        /// <see cref="Configuration.LdapOptionsBase.IsRecursiveGroupMembership"/>
        /// is <c>true</c>.</para>
        /// <para>The results provided by the method might not be distinct if the
        /// group hierarchy is expanded and a user is transitive member via
        /// multiple paths.</para>
        /// </remarks>
        /// <param name="group">The group to retrieve the users for.</param>
        /// <returns>The members of the given group.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="group"/>
        /// is <c>null</c>.</exception>
        IEnumerable<TUser> GetGroupMembers(TGroup group);

        /// <summary>
        /// Asynchronously retrieves the users in the given
        /// <paramref name="group"/>.
        /// </summary>
        /// <remarks>
        /// <para>The groups in <paramref name="group"/> will be recursively
        /// expanded if
        /// <see cref="Configuration.LdapOptionsBase.IsRecursiveGroupMembership"/>
        /// is <c>true</c>.</para>
        /// <para>The results provided by the method might not be distinct if the
        /// group hierarchy is expanded and a user is transitive member via
        /// multiple paths.</para>
        /// </remarks>
        /// <param name="group">The group to retrieve the users for.</param>
        /// <returns>The members of the given group.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="group"/>
        /// is <c>null</c>.</exception>
        Task<IEnumerable<TUser>> GetGroupMembersAsync(TGroup group,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all groups from the directory that are in matching the search
        /// criteria configured in the
        /// <see cref="Configuration.LdapOptionsBase"/> used by the application.
        /// </summary>
        /// <remarks>
        /// <para>This method creates <typeparamref name="TGroup" /> object for
        /// all groups matching the global search configuration, which might not
        /// only be a large results set, but also trigger a lot of additional
        /// LDAP searches in order to fill the parent group claims configured in
        /// the group object. Therefore, you should carefully design your LDAP
        /// group object in order to restrict the data that must be retrieved to
        /// the absolute minimum for the application case.</para>
        /// </remarks>
        /// <returns>All groups in the directory matching the global search
        /// criteria.</returns>
        IEnumerable<TGroup> GetGroups();

        /// <summary>
        /// Asynchronously gets all groups from the directory that are in
        /// matching the search criteria configured in the
        /// <see cref="Configuration.LdapOptionsBase"/> used by the application.
        /// </summary>
        /// <remarks>
        /// <para>This method creates <typeparamref name="TGroup" /> object for
        /// all groups matching the global search configuration, which might not
        /// only be a large results set, but also trigger a lot of additional
        /// LDAP searches in order to fill the parent group claims configured in
        /// the group object. Therefore, you should carefully design your LDAP
        /// group object in order to restrict the data that must be retrieved to
        /// the absolute minimum for the application case.</para>
        /// </remarks>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns>All groups in the directory matching the global search
        /// criteria.</returns>
        Task<IEnumerable<TGroup>> GetGroupsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all groups from the directory that are matching the search
        /// criteria configured in the
        /// <see cref="Configuration.LdapOptionsBase"/> used by the application
        /// <i>and</i> the specified LDAP <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for groupsg. It is safe to pass
        /// <c>null</c>, in which case the additional filter criteria will
        /// be ignored.</param>
        /// <returns>All groups in the directory matching the given search
        /// criteria.</returns>
        IEnumerable<TGroup> GetGroups(string filter);

        /// <summary>
        /// Gets all groups from the directory that are matching the search
        /// criteria configured in the
        /// <see cref="Configuration.LdapOptionsBase"/> used by the application
        /// <i>and</i> the specified LDAP <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for groups. It is safe to pass
        /// <c>null</c>, in which case the additional filter criteria will
        /// be ignored.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns>All groups in the directory matching the given search
        /// criteria.</returns>
        Task<IEnumerable<TGroup>> GetGroupsAsync(string filter,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all groups from the directory that are matching the search
        /// critiera configured in the
        /// <see cref="Configuration.LdapOptionsBase"/> used by the application
        /// <i>and</i> the specified LDAP <paramref cref="filter"/> while
        /// overriding the search bases from the
        /// <see cref="Configuration.LdapOptionsBase"/> with the given ones.
        /// </summary>
        /// <param name="searchBases">The search bases to look in. It is safe
        /// to pass <c>null</c>, in which case the search bases from the
        /// <see cref="Configuration.LdapOptionsBase"/> will be used.</param>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for groups. It is safe to pass
        /// <c>null</c>, in which case the additional filter criteria will
        /// be ignored.</param>
        /// <returns>All groups in the directory matching the given search
        /// criteria.</returns>
        IEnumerable<TGroup> GetGroups(
            IDictionary<string, TSearchScope> searchBases,
            string filter);

        /// <summary>
        /// Gets all groups from the directory that are matching the search
        /// critiera configured in the
        /// <see cref="Configuration.LdapOptionsBase"/> used by the application
        /// <i>and</i> the specified LDAP <paramref cref="filter"/> while
        /// overriding the search bases from the
        /// <see cref="Configuration.LdapOptionsBase"/> with the given ones.
        /// </summary>
        /// <param name="searchBases">The search bases to look in. It is safe
        /// to pass <c>null</c>, in which case the search bases from the
        /// <see cref="Configuration.LdapOptionsBase"/> will be used.</param>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for groups. It is safe to pass
        /// <c>null</c>, in which case the additional filter criteria will
        /// be ignored.</param>
        /// <returns>All groups in the directory matching the given search
        /// criteria.</returns>
        Task<IEnumerable<TGroup>> GetGroupsAsync(
            IDictionary<string, TSearchScope> searchBases,
            string filter,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user with the specified account name.
        /// </summary>
        /// <param name="accountName">The account name of the user
        /// to look for.</param>
        /// <returns>The user or <c>null</c> if no user matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="accountName"/> is <c>null</c>.</exception>
        TUser? GetUserByAccountName(string accountName);

        /// <summary>
        /// Asynchronously gets a user with the specified account name.
        /// </summary>
        /// <param name="accountName">The account name of the user
        /// to look for.</param>
        /// <returns>The user or <c>null</c> if no user matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="accountName"/> is <c>null</c>.</exception>
        Task<TUser?> GetUserByAccountNameAsync(string accountName);

        /// <summary>
        /// Gets a user with the specified distinguished name.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the user
        /// to look for.</param>
        /// <returns>The user or <c>null</c> if no user matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="distinguishedName"/> is <c>null</c>.</exception>
        TUser? GetUserByDistinguishedName(string distinguishedName);

        /// <summary>
        /// Asynchronously gets a user with the specified distinguished name.
        /// </summary>
        /// <param name="distinguishedName">The distinguished name of the user
        /// to look for.</param>
        /// <returns>The user or <c>null</c> if no user matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="distinguishedName"/> is <c>null</c>.</exception>
        Task<TUser?> GetUserByDistinguishedNameAsync(string distinguishedName);

        /// <summary>
        /// Gets a user with the specified value for the identity attribute.
        /// </summary>
        /// <param name="identity">The value of the identity attribute to
        /// be searched.</param>
        /// <returns>The user or <c>null</c> if no user matching the query
        /// exists.</returns>
        /// <exception cref="System.ArgumentNullException">If
        /// <paramref name="identity"/> is <c>null</c>.</exception>
        TUser? GetUserByIdentity(string identity);

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
        Task<TUser?> GetUserByIdentityAsync(string identity);

        /// <summary>
        /// Gets all users from the directory that are in matching the search
        /// criteria configured in the
        /// <see cref="Configuration.LdapOptionsBase"/> used by the application.
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
        /// <see cref="Configuration.LdapOptionsBase"/> used by the application.
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
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns>All users in the directory matching the global search
        /// criteria.</returns>
        Task<IEnumerable<TUser>> GetUsersAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all users from the directory that are matching the search
        /// criteria configured in the
        /// <see cref="Configuration.LdapOptionsBase"/> used by the application
        /// <i>and</i> the specified LDAP <paramref name="filter"/>.
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
        /// criteria configured in the
        /// <see cref="Configuration.LdapOptionsBase"/> used by the application
        /// <i>and</i> the specified LDAP <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users. It is safe to pass
        /// <c>null</c>, in which case the additional filter criteria will
        /// be ignored.</param>
        /// <param name="cancellationToken">A cancellation token for aborting
        /// the operation.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        Task<IEnumerable<TUser>> GetUsersAsync(string filter,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all users from the directory that are matching the search
        /// critiera configured in the
        /// <see cref="Configuration.LdapOptionsBase"/> used by the application
        /// <i>and</i> the specified LDAP <paramref cref="filter"/> while
        /// overriding the search bases from the
        /// <see cref="Configuration.LdapOptionsBase"/> with the given ones.
        /// </summary>
        /// <param name="searchBases">The search bases to look in. It is safe
        /// to pass <c>null</c>, in which case the search bases from the
        /// <see cref="Configuration.LdapOptionsBase"/> will be used.</param>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users. It is safe to pass
        /// <c>null</c>, in which case the additional filter criteria will
        /// be ignored.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        IEnumerable<TUser> GetUsers(
            IDictionary<string, TSearchScope> searchBases,
            string filter);

        /// <summary>
        /// Gets all users from the directory that are matching the search
        /// critiera configured in the
        /// <see cref="Configuration.LdapOptionsBase"/> used by the application
        /// <i>and</i> the specified LDAP <paramref cref="filter"/> while
        /// overriding the search bases from the
        /// <see cref="Configuration.LdapOptionsBase"/> with the given ones.
        /// </summary>
        /// <param name="searchBases">The search bases to look in. It is safe
        /// to pass <c>null</c>, in which case the search bases from the
        /// <see cref="Configuration.LdapOptionsBase"/> will be used.</param>
        /// <param name="filter">An LDAP filter expression that is combined
        /// with the global search criteria for users. It is safe to pass
        /// <c>null</c>, in which case the additional filter criteria will
        /// be ignored.</param>
        /// <returns>All users in the directory matching the given search
        /// criteria.</returns>
        Task<IEnumerable<TUser>> GetUsersAsync(
            IDictionary<string, TSearchScope> searchBases,
            string filter,
            CancellationToken cancellationToken = default);
    }
}
