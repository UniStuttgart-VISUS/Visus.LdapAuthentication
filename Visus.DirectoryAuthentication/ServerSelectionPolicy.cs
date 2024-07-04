// <copyright file="ServerSelectionPolicy.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Defines how the library uses more than one <see cref="LdapServer"/>
    /// provided in <see cref="LdapOptions.Servers"/>.
    /// </summary>
    public enum ServerSelectionPolicy {

        /// <summary>
        /// Use the first server and if the connection fails, subsequently try
        /// the other ones in the order they are specified.
        /// </summary>
        Failover,

        /// <summary>
        /// Use one server after the other in a round-robin fashion.
        /// </summary>
        LoadBalancing
    }
}
