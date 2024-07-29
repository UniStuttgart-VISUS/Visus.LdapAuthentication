// <copyright file="ClaimsBuilder.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Visus.DirectoryAuthentication.Configuration;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Claims {

    /// <summary>
    /// The default implementation of
    /// <see cref="IClaimsBuilder{TUser, TGroup}"/> which is based on
    /// attribute annotations of the user and group objects.
    /// </summary>
    /// <remarks>
    /// <para>This builder can be used for any user and group type that uses
    /// <see cref="ClaimAttribute"/>s to mark the properties of the user
    /// or group as a claim. Note that for adding group-based claims to a
    /// user automatically, the property holding the groups in the user
    /// must be marked with <see cref="GroupMembershipsAttribute"/> and
    /// return an <see cref="IEnumerable{T}"/> of
    /// <typeparamref name="TGroup"/>.</para>
    /// </remarks>
    /// <typeparam name="TUser">The type of the user to create the claims for.
    /// </typeparam>
    /// <typeparam name="TGroup">The type of the group to create the claims for.
    /// </typeparam>
    /// <param name="userClaims">The map from LDAP attributes to per-user claims.
    /// </param>
    /// <param name="userMap">The amp from LDAP attributes to properties of
    /// <typeparamref name="TUser"/>.</param>
    /// <param name="groupClaims">The map from LDAP attributes to per-group
    /// claims.</param>
    /// <param name="groupMap">The amp from LDAP attributes to properties of
    /// <typeparamref name="TGroup"/>.</param>
    /// <param name="options">The LDAP configuration which determines the schema
    /// to be used.</param>
    public class ClaimsBuilder<TUser, TGroup>(IUserClaimsMap userClaims,
            ILdapAttributeMap<TUser> userMap,
            IGroupClaimsMap groupClaims,
            ILdapAttributeMap<TGroup> groupMap,
            IOptions<LdapOptions> options)
        : ClaimsBuilderBase<TUser, TGroup>(userClaims,
            userMap,
            groupClaims,
            groupMap,
            options?.Value!) { }
}
