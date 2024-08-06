// <copyright file="IdentityUserClaimsMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Visus.Identity.Properties;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;
using LdapAttribute = Visus.Ldap.Mapping.LdapAttributeAttribute;


namespace Visus.Ldap.Claims {

    ///// <summary>
    ///// Implements a <see cref="IUserClaimsMap"/> for
    ///// <see cref="IdentityUser{TKey}"/>.
    ///// </summary>
    //public class IdentityUserClaimsMap<TObject, TKey, TOptions>
    //        : IUserClaimsMap
    //        where TObject : IdentityUser<TKey>
    //        where TKey : IEquatable<TKey>
    //        where TOptions : LdapOptionsBase {

    //    public IdentityUserClaimsMap(
    //            IOptions<IdentityOptions> identityOptions,
    //            IOptions<TOptions> ldapOptions) {
    //        ArgumentNullException.ThrowIfNull(identityOptions?.Value);
    //        ArgumentNullException.ThrowIfNull(ldapOptions?.Value);

    //        switch (ldapOptions.Value.Schema) {
    //            case Schema.ActiveDirectory:
    //                this._map[new LdapAttribute(Schema.ActiveDirectory, "objectSid")]

    //                builder.MapAttribute("objectSid")
    //                    .WithConverter<SidConverter>()
    //                    .ToClaim(ClaimTypes.Sid);

    //                builder.MapAttribute("uidNumber")
    //                    .ToClaim(options.UserIdClaimType);

    //                builder.MapAttribute("sAMAccountName")
    //                    .ToClaim(options.UserNameClaimType);
    //                break;

    //            case Schema.IdentityManagementForUnix:
    //                break;

    //            case Schema.Rfc2307:
    //                break;

    //            default:
    //                throw new ArgumentException(string.Format(
    //                    Resources.ErrorSchemaNotWellKnown,
    //                    ldapOptions.Value.Schema));
    //        }
    //    }

    //    public IEnumerable<ClaimAttribute> this[LdapAttribute attribute]
    //        => this._map.TryGetValue(attribute, out var retval)
    //        ? retval : Enumerable.Empty<ClaimAttribute>();

    //    public IEnumerable<string> AttributeNames => throw new NotImplementedException();

    //    public IEnumerable<LdapAttribute> Attributes => this._map.Keys;

    //    public IEnumerator<KeyValuePair<LdapAttribute,
    //            IEnumerable<ClaimAttribute>>> GetEnumerator()
    //        => this._map.GetEnumerator();

    //    IEnumerator IEnumerable.GetEnumerator()
    //        => this._map.GetEnumerator();

    //    #region Private methods

    //    #endregion

    //    #region Private fields
    //    private readonly Dictionary<LdapAttribute,
    //        IEnumerable<ClaimAttribute>> _map = new();
    //    #endregion
    //}
}
