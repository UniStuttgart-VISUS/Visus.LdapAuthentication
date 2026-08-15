// <copyright file="BasicAuthenticationOptions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2026 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Authentication;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using Visus.DirectoryAuthentication.Configuration;
using Visus.Ldap.Claims;


namespace Visus.BasicLdapAuthentication.Configuration {

    /// <summary>
    /// Configuration options for the basic authentication scheme.
    /// </summary>
    public sealed class BasicAuthenticationOptions
            : AuthenticationSchemeOptions {

        /// <summary>
        /// The suggested name of the configuration section.
        /// </summary>
        public const string Section = "BasicAuthentication";

        #region Public properties
        /// <summary>
        /// Gets or sets the authentication scheme advertised to the client in
        /// the WWW-Authenticate header. This defaults to &quot;Basic&quot;.
        /// </summary>
        public string AdvertisedScheme { get; set; } = "Basic";

        /// <summary>
        /// Gets or sets a value indicating whether the encoding preference is
        /// appended to the WWW-Authenticate header sent to the client.
        /// </summary>
        public bool AdvertiseEncodings { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the cookie can be refreshed if
        /// <see cref="CookieScheme"/> is set.
        /// </summary>
        public bool AllowCookieRefresh { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the authentication handler allows unencrypted
        /// HTTP requests. This feature is only allowed in debug builds and
        /// development environments.
        /// </summary>
        public bool AllowUnencrypted { get; set; } = false;

        /// <summary>
        /// Gets or sets a filter that is applied to the LDAP-based claims
        /// before creating a principal from them.
        /// </summary>
        public ClaimFilter? ClaimFilter { get; set; } = null;

        /// <summary>
        /// Gets or sets the lifetime of the cookie used for authentication,
        /// which is only relevant if <see cref="CookieScheme"/> is set.
        /// </summary>
        public TimeSpan CookieLifetime { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Gets or sets a cookie authentication scheme that can be used in
        /// addition to the basic authentication scheme.
        /// </summary>
        /// <remarks>
        /// If this property is a non-empty string, the authentication handler
        /// will sign in the user to the
        /// <see cref="Microsoft.AspNetCore.Http.HttpContext"/> of the
        /// authentication request using this scheme.
        /// </remarks>
        public string? CookieScheme { get; set; } = null;

        /// <summary>
        /// Gets or sets, in order of preference, the encodings for the
        /// credentials.
        /// </summary>
        public IEnumerable<Encoding> Encodings {
            get;
            set;
        } = [Encoding.UTF8, Encoding.Latin1];

        /// <summary>
        /// Gets or sets the name of the authentication header. This defaults to
        /// <see cref="HeaderNames.Authorization"/>.
        /// </summary>
        public string Header { get; set; } = HeaderNames.Authorization;

        /// <summary>
        /// Gets or sets whether the cookie is configured to be persistent, i.e.
        /// outlives the session, if <see cref="CookieScheme"/> is set.
        /// </summary>
        public bool PersistentCookie { get; set; } = false;

        /// <summary>
        /// Gets or sets the realm sent in the authentication header.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null" />, no realm will be sent.
        /// </remarks>
        public string? Realm { get; set; }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public override void Validate() {
            base.Validate();
            new BasicAuthenticationOptionsValidator().Validate(this);
        }
        #endregion
    }
}
