﻿// <copyright file="LdapUser.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Default LDAP user retrieved on logon.
    /// </summary>
    /// <remarks>
    /// This implementation uses the attribute-based mapping of LDAP attributes
    /// to user properties and claims that is defined in
    /// <see cref="LdapUserBase"/>. If you need a customised user object,
    /// inherit from <see cref="LdapUserBase"/> instead of using this user
    /// object whenever possible. Otherwise, you need to either apply the
    /// correct attributes to your custom object or provide a custom
    /// <see cref="ILdapMapper{TUser, TGroup}"/>.
    /// </remarks>
    public sealed class LdapUser : LdapUserBase<LdapGroup> { }
}
