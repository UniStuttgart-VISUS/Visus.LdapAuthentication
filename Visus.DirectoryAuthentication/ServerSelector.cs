// <copyright file="ServerSelector.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Linq;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Implements the state of the <see cref="ServerSelectionPolicy"/>.
    /// </summary>
    internal sealed class ServerSelector {

        /// <summary>
        /// Selects the next server to use from
        /// <see cref="LdapOptions.Servers"/> based on the specified
        /// <see cref="LdapOptions.ServerSelectionPolicy"/>.
        /// </summary>
        /// <param name="options">The <see cref="LdapOptions"/> specifying
        /// the policy and the servers to select from.</param>
        /// <returns>The server to use next.</returns>
        public LdapServer Select(LdapOptions options) {
            return options.Servers.First();
        }

    }
}
