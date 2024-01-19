// <copyright file="ILdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Threading.Tasks;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Interface for simple form-based authentication.
    /// </summary>
    public interface ILdapAuthenticationService {

        /// <summary>
        /// Performs an LDAP bind using the specified credentials and retrieves
        /// the LDAP entry with the account name <paramref name="username"/> in
        /// case the bind succeeds.
        /// </summary>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The user object in case of a successful login.</returns>
        ILdapUser Login(string username, string password);

        /// <summary>
        /// Performs an asynchronous LDAP bind using the specified credentials
        /// and retrieves the LDAP entry with the account name
        /// <paramref name="username"/> in case the bind succeeds.
        /// </summary>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The user object in case of a successful login.</returns>
        Task<ILdapUser> LoginAsync(string username, string password);
    }


    /// <summary>
    /// Strongly typed variant of <see cref="ILdapAuthenticationService"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of user that is to be retrieved on
    /// successful login.</typeparam>
    public interface ILdapAuthenticationService<TUser>
            : ILdapAuthenticationService where TUser : class, ILdapUser {

        /// <summary>
        /// Performs an asynchronous LDAP bind using the specified credentials
        /// and retrieves the LDAP entry with the account name
        /// <paramref name="username"/> in case the bind succeeds.
        /// </summary>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The user object in case of a successful login.</returns>
        new TUser Login(string username, string password);

        /// <summary>
        /// Performs an LDAP bind using the specified credentials and retrieves
        /// the LDAP entry with the account name <paramref name="username"/> in
        /// case the bind succeeds.
        /// </summary>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The user object in case of a successful login.</returns>
        new Task<TUser> LoginAsync(string username, string password);
    }
}
