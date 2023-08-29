// <copyright file="ILdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2023 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System.Threading.Tasks;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Interface for simple form-based authentication.
    /// </summary>
    public interface ILdapAuthenticationService {

        /// <summary>
        /// Login the specified user.
        /// </summary>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>The logged on use in case of success or <c>null</c> if the
        /// login failed.</returns>
        ILdapUser Login(string username, string password);

        /// <summary>
        /// Asynchronously login the specified user.
        /// </summary>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>The logged on use in case of success or <c>null</c> if the
        /// login failed.</returns>
        Task<ILdapUser> LoginAsync(string username, string password);
    }
}
