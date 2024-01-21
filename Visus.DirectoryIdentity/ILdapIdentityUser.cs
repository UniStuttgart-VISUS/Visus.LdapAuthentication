// <copyright file="ILdapIdentityUser.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Visus.DirectoryAuthentication;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// The interface for an LDAP user object to be used with the identity
    /// store.
    /// </summary>
    public interface ILdapIdentityUser : ILdapUser { }
}
