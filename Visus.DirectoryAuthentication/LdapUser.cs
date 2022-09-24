// <copyright file="LdapUser.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2022 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
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
    /// object and customise the attribute mapping or override the whole
    /// process by overloading <see cref="LdapUserBase.Assign"/>.
    /// </remarks>
    public sealed class LdapUser : LdapUserBase { }
}
