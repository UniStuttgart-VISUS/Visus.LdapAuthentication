// <copyright file="ILdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Interface for simple form-based authentication.
    /// </summary>
    public interface ILdapAuthenticationService {

        /// <summary>
        /// Login the specified user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        ILdapUser Login(string username, string password);
    }
}
