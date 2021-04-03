﻿// <copyright file="LdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Interface of a service providing access to a configured LDAP connection.
    /// </summary>
    public interface ILdapConnectionService {

        /// <summary>
        /// Gets the <see cref="ILdapOptions"/> used by the service.
        /// </summary>
        ILdapOptions Options { get; }

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
