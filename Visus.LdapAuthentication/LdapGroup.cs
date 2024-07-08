// <copyright file="LdapGroup.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>


using System.Diagnostics;

namespace Visus.LdapAuthentication {

    /// <summary>
    /// The default class used to represent a group membership.
    /// </summary>
    /// <remarks>
    /// <para>This implementation uses the attribute-based mapping of LDAP
    /// attributes to user properties and claims that is defined in
    /// <see cref="LdapGroupBase"/>. It is the group implementation used by
    /// <see cref="LdapUser"/>.</para>
    /// <para>If you need a customised user object, the first choice is inheriting
    /// from <see cref="LdapGroupBase"/> and use the customised class instead of
    /// this one. If you provide a fully custom group class, you either need to
    /// apply the correct attributes to your custom object or provide a custom
    /// <see cref="ILdapMapper{TUser, TGroup}"/> that is compatible with your
    /// class.</para>
    /// </remarks>
    [DebuggerDisplay("{DistinguishedName}")]
    public sealed class LdapGroup : LdapGroupBase { }
}
