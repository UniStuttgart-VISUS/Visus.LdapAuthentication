// <copyright file="ILdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Interface for simple form-based authentication.
    /// </summary>
    public interface ILdapAuthenticationService {

        /// <summary>
        /// Performs an LDAP bind using the specified credentials and retrieves
        /// the LDAP entry with the account name <paramref name="username"/> in
        /// case the bind succeeds.
        /// </summary>
        /// <param name="username">The user name to authenticate. Depending on
        /// the LDAP server used, this might need to be a distinguished name.
        /// </param>
        /// <param name="password">The password of the user, which is used to
        /// bind against the LDAP server for checking whether it is correct.
        /// </param>
        /// <returns>A user object holding the properties of a successfully
        /// authenticated user, <c>null</c> if the login failed.</returns>
        ILdapUser Login(string username, string password);
    }


    /// <summary>
    /// Strongly typed variant of <see cref="ILdapAuthenticationService"/>.
    /// </summary>
    /// <typeparam name="TUser">The type of user that is to be retrieved on
    /// successful login.</typeparam>
    public interface ILdapAuthenticationService<TUser>
            : ILdapAuthenticationService where TUser : class, ILdapUser {

        /// <summary>
        /// Performs an LDAP bind using the specified credentials and retrieves
        /// the LDAP entry with the account name <paramref name="username"/> in
        /// case the bind succeeds.
        /// </summary>
        /// <param name="username">The user name to authenticate. Depending on
        /// the LDAP server used, this might need to be a distinguished name.
        /// </param>
        /// <param name="password">The password of the user, which is used to
        /// bind against the LDAP server for checking whether it is correct.
        /// </param>
        /// <returns>A user object holding the properties of a successfully
        /// authenticated user, <c>null</c> if the login failed.</returns>
        new TUser Login(string username, string password);

    }
}
