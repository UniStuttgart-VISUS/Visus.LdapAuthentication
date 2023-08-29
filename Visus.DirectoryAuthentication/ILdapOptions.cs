// <copyright file="ILdapOptions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2023 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// The configuration options for the directory server.
    /// </summary>
    public interface ILdapOptions {

        /// <summary>
        /// Gets the default domain appended to a user name.
        /// </summary>
        /// <remarks>
        /// Certain LDAP servers (like AD) might require the UPN instead of the
        /// account name to be used for binding. If this property is set, the
        /// <see cref="ILdapAuthenticationService.Login(string, string)"/>
        /// method will check for plain account names and append this domain
        /// to the user name. This will allow users to logon with their short
        /// account name.
        /// </remarks>
        string DefaultDomain { get; }

        /// <summary>
        /// Gets whether the certificate check is disabled for accessing the
        /// LDAP server.
        /// </summary>
        /// <remarks>
        /// <para>You should not do that for production code. This is only
        /// intended for development setups where you are working with
        /// self-signed certificates.</para>
        /// </remarks>
        bool IsNoCertificateCheck { get; }

        /// <summary>
        /// Gets whether group memberships should be looked up recursively.
        /// </summary>
        /// <remarks>
        /// <para>If you can express your login policies using only the primary
        /// group and any direct group memberships of the user accounts, you
        /// can and should disable this flag. Enabling the flag causes the login
        /// process to recursively accumulate all transitive group memberships,
        /// which causes a significant increase in LDAP queries that need to be
        /// performed for each login.</para>
        /// </remarks>
        bool IsRecursiveGroupMembership { get; }

        /// <summary>
        /// Get whether the LDAP connection uses SSL or not.
        /// </summary>
        bool IsSsl { get; }

        /// <summary>
        /// Gets whether the <see cref="LdapSearchBase"/> is searched
        /// recursively or not.
        /// </summary>
        /// <remarks>
        /// <para>Setting this flag will cause the LDAP queries use
        /// <see cref="Novell.Directory.Ldap.LdapConnection.ScopeSub"/>.
        /// Otherwise,
        /// <see cref="Novell.Directory.Ldap.LdapConnection.ScopeBase"/> will
        /// be used, ie all user accounts and groups must reside directly in
        /// <see cref="SearchBase"/>.</para>
        /// </remarks>
        bool IsSubtree { get; }

        /// <summary>
        /// Gets the global LDAP mapping for the selected schema.
        /// </summary>
        /// <remarks>
        /// <para>This property mostly specifies the behaviour of the group
        /// whereas the attribute mapping of the user is specified via
        /// <see cref="ILdapUser.RequiredAttributes"/></para>.
        /// </remarks>
        LdapMapping Mapping { get; }

        /// <summary>
        /// Gets the per-schema global LDAP mappings, which are used to retrieve
        /// group memberships etc.
        /// </summary>
        /// <remarks>
        /// <para>This property is mostly used to specify built-in mappings. If
        /// you want to provide a custom mapping, it suffices overriding the
        /// <see cref="Mapping"/> property</para>.
        /// </remarks>
        Dictionary<string, LdapMapping> Mappings { get; }

        /// <summary>
        /// Gets the maximum number of results the LDAP client should request.
        /// </summary>
        /// <remarks>
        /// <para>This is currently not used.</para>
        /// </remarks>
        int PageSize { get; }

        /// <summary>
        /// Gets the password used to connect to the LDAP server.
        /// </summary>
        /// <remarks>
        /// <para>This password is only used when performing additional
        /// queries using <see cref="ILdapConnectionService"/>. All login
        /// requests in <see cref="ILdapAuthenticationService"/> are performed
        /// by binding the user that is trying to log in. The user therefore
        /// must be able to read his/her own LDAP entry and the groups in
        /// order to fill <see cref="ILdapUser"/>.</para>
        /// </remarks>
        string Password { get; }

        /// <summary>
        /// Gets the port of the LDAP server.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the thumbprint of the trusted root CA for the LDAP server.
        /// </summary>
        /// <remarks>
        /// <para>If this property is <c>null</c>, the certificate chain will
        /// not be checked, ie any root CA will be acceptable</para>
        /// <para>This property is only relevant if <see cref="UseSsl"/> is
        /// enabled.</para>
        /// </remarks>
        string RootCaThumbprint { get; }

        /// <summary>
        /// Gets the name of the LDAP schema which is used by
        /// <see cref="LdapUserBase"/> to automatically retrieve the required
        /// properties.
        /// </summary>
        string Schema { get; }

        /// <summary>
        /// Gets the starting point of any directory search.
        /// </summary>
        string SearchBase { get; }

        /// <summary>
        /// Gets the host name or IP of the LDAP server.
        /// </summary>
        string Server { get; }

        /// <summary>
        /// Gets the certificate thumbprints for the LDAP servers that are
        /// accepted during certificate validation.
        /// </summary>
        /// <remarks>
        /// <para>If this array is empty, any server certificate will be
        /// accepted. Note that if <see cref="RootCaThumbprint"/> is set as
        /// well, the server certificate must have been issued by the specified
        /// root CA, ie both thumbprints must match.</para>
        /// <para>This property is only relevant if <see cref="UseSsl"/> is
        /// enabled.</para>
        /// </remarks>
        string[] ServerThumbprint { get; }

        /// <summary>
        /// Gets the timeout for LDAP queries.
        /// </summary>
        /// <remarks>
        /// <para>This is currently not used.</para>
        /// <para>A value of <see cref="TimeSpan.Zero"/> indicates an infinite
        /// timeout.</para></remarks>
        TimeSpan Timeout { get; }

        /// <summary>
        /// Gets the LDAP user used to connect to the directory.
        /// </summary>
        /// <remarks>
        /// This is only used by <see cref="ILdapConnectionService"/>. See
        /// <see cref="Password"/> for more details.
        /// </remarks>
        string User { get; }
    }
}
