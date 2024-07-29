// <copyright file="ILdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Threading.Tasks;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Interface for simple form-based authentication of users mapped to the
    /// specified type.
    /// </summary>
    /// <typeparam name="TUser">The type of user that is to be retrieved on
    /// successful login.</typeparam>
    /// <typeparam name="TGroup">The type of the groups associated with a
    /// user.</typeparam>
    public interface ILdapAuthenticationService<TUser> where TUser : class {

        /// <summary>
        /// Performs an asynchronous LDAP bind using the specified credentials
        /// and retrieves the LDAP entry with the account name
        /// <paramref name="username"/> in case the bind succeeds.
        /// </summary>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The user object in case of a successful login.</returns>
        TUser? LoginUser(string username, string password);

        /// <summary>
        /// Performs an LDAP bind using the specified credentials and retrieves
        /// the LDAP entry with the account name <paramref name="username"/> in
        /// case the bind succeeds.
        /// </summary>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The user object in case of a successful login.</returns>
        Task<TUser?> LoginUserAsync(string username, string password);
    }

}
