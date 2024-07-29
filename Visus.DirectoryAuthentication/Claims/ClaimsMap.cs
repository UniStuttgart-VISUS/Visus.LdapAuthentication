// <copyright file="ClaimsMap.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using Visus.DirectoryAuthentication.Configuration;
using Visus.Ldap.Claims;


namespace Visus.DirectoryAuthentication.Claims {

    /// <summary>
    /// Implementation of the entry to claim mapper, which derives the
    /// direct mapping of LDAP attributes to
    /// <see cref="System.Security.Claims.Claim"/>s from annotations on
    /// <typeparamref name="TObject"/>.
    /// </summary>
    /// <typeparam name="TObject">The object used to reflect the claims from,
    /// which must be annotated with <see cref="LdapAttribute"/> and
    /// <see cref="ClaimAttribute"/> in order to automatically derive the claim
    /// mapping.</typeparam>
    public sealed class ClaimsMap<TObject> : ClaimsMapBase<TObject> {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="options">The LDAP options, which determine the schema to
        /// be used for reflecting LDAP attributes.</param>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="options"/> is <c>null</c>.</exception>
        public ClaimsMap(IOptions<LdapOptions> options)
            : base(options?.Value!) { }
        #endregion
    }
}
