// <copyright file="ILdapSearchService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Threading;
using System.Threading.Tasks;
using Visus.Ldap;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Interface for a service allowing applications to search for users and
    /// groups.
    /// </summary>
    /// <remarks>
    /// The search service allows an application to retrieve user information
    /// without binding as an end user. In order to perform the search, the
    /// credentials specified in <see cref="Configuration.LdapOptions"/> are
    /// used.
    /// </remarks>
    /// <typeparam name="TUser">The type of user that is to be retrieved from
    /// the directory.</typeparam>
    /// <typeparam name="TGroup">The type used to represent group memberships
    /// of <typeparamref name="TUser"/>.</typeparam>
    public interface ILdapSearchService<TUser, TGroup>
        : ILdapSearchServiceBase<TUser, TGroup, SearchScope>;
}
