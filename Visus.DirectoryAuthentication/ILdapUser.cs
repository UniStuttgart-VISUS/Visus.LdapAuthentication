// <copyright file="ILdapUser.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Security.Claims;
using System.Text.Json.Serialization;


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

        /// <summary>
        /// Gets the attributes that must be loaded for each
        /// <see cref="System.DirectoryServices.Protocols.SearchResultEntry"/>
        /// in order to fill all properties and claims of the user object.
        /// </summary>
        [JsonIgnore]
        IEnumerable<string> RequiredAttributes { get; }

        /// <summary>
        /// Assigns the specified LDAP entry as the user.
        /// </summary>
        /// <param name="entry">The entry representing the user.</param>
        /// <param name="connection">A <see cref="LdapConnection"/> that can be
        /// used to obtain additional information about the user.</param>
        /// <param name="options">The <see cref="LdapOptions"/> that can be used
        /// to obtain additional information about the user-defined mapping of
        /// LDAP attributes.</param>
        void Assign(SearchResultEntry entry, LdapConnection connection,
            LdapOptions options);
    }
}
