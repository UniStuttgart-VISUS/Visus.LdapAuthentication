// <copyright file="LdapGroup.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


using System.Diagnostics;

namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// The default class used to represent a group membership.
    /// </summary>
    /// <remarks>
    /// This implementation uses the attribute-based mapping of LDAP attributes
    /// to user properties and claims that is defined in
    /// <see cref="LdapGroupBase"/>. If you need a customised user object,
    /// inherit from <see cref="LdapGroupBase"/> instead of using this user
    /// object whenever possible. Otherwise, you need to either apply the
    /// correct attributes to your custom object or provide a custom
    /// <see cref="ILdapMapper{TUser, TGroup}"/>.
    /// </remarks>
    [DebuggerDisplay("{DistinguishedName}")]
    public sealed class LdapGroup : LdapGroupBase { }
}
