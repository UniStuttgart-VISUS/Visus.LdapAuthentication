// <copyright file="ILdapUser.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;
using System.Security.Claims;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Interface for an application user object.
    /// </summary>
    public interface ILdapUser {

        /// <summary>
        /// Gets the unique account name of the user.
        /// </summary>
        string AccountName { get; }

        /// <summary>
        /// Gets the claims (eg group memberships) for the user.
        /// </summary>
        IEnumerable<Claim> Claims { get; }

        /// <summary>
        /// Gets the display name of the user.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the e-mail address of the user.
        /// </summary>
        string EmailAddress { get; }

        /// <summary>
        /// Gets the security identifier of the user.
        /// </summary>
        string Identity { get; }
    }
}
