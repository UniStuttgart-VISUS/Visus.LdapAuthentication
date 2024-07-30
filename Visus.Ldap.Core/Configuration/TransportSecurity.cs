// <copyright file="TransportSecurity.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.Ldap.Configuration {

    /// <summary>
    /// Possible encryption options for LDAP traffic.
    /// </summary>
    public enum TransportSecurity {

        /// <summary>
        /// The LDAP traffic will be unencrypted.
        /// </summary>
        None,

        /// <summary>
        /// Use an LDAPS connection.
        /// </summary>
        Ssl,

        /// <summary>
        /// Connect to an unencrypted end point and start TLS.
        /// </summary>
        StartTls
    }
}
