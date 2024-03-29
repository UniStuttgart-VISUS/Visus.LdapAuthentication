﻿// <copyright file="ILdapConnectionService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Interface of a service providing access to a configured LDAP connection.
    /// </summary>
    public interface ILdapConnectionService {

        /// <summary>
        /// Gets the <see cref="IOptions"/> used by the service.
        /// </summary>
        IOptions Options { get; }

        /// <summary>
        /// Connect to the preconfigured LDAP service with the preconfigured
        /// credentials.
        /// </summary>
        /// <returns></returns>
        LdapConnection Connect();

        /// <summary>
        /// Connect to the preconfigured LDAP service with the specified
        /// credentials.
        /// </summary>
        /// <param name="username">The user name used to perform the LDAP bind.
        /// </param>
        /// <param name="password">The password used to perfom the LDAP bind.
        /// </param>
        /// <returns></returns>
        LdapConnection Connect(string username, string password);
    }
}
