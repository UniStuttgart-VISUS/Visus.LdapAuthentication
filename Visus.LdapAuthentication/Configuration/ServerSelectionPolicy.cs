// <copyright file="ServerSelectionPolicy.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.LdapAuthentication.Configuration {

    /// <summary>
    /// Specifies how the library will select the server to use when multiple
    /// servers are given.
    /// </summary>
    public enum ServerSelectionPolicy {

        /// <summary>
        /// Servers are used round-robin whenever establishing a new connection.
        /// </summary>
        RoundRobin,

        /// <summary>
        /// Servers are used in decreasing priority and only if the previous one
        /// failed.
        /// </summary>
        Failover
    }
}
