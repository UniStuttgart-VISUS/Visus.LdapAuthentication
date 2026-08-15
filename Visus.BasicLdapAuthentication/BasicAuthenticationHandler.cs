// <copyright file="BasicAuthenticationHandler.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2026 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Visus.BasicLdapAuthentication.Configuration;
using Visus.BasicLdapAuthentication.Properties;
using Visus.Ldap;
using static System.StringComparison;


namespace Visus.BasicLdapAuthentication {

    /// <summary>
    /// Implements basic authentication based on LDAP authentication.
    /// </summary>
    /// <typeparam name="TUser">The type of the LDAP user.</typeparam>
    /// <param name="options"></param>
    /// <param name="ldap"></param>
    /// <param name="loggers"></param>
    /// <param name="encoder"></param>
    /// <param name="clock"></param>
    internal sealed class BasicAuthenticationHandler<TUser>(
                IOptionsMonitor<BasicAuthenticationOptions> options,
                ILdapAuthenticationService<TUser> ldap,
                ILoggerFactory loggers,
                UrlEncoder encoder,
                ISystemClock clock)
            : AuthenticationHandler<BasicAuthenticationOptions>(
                options,
                loggers,
                encoder,
                clock)
                where TUser : class {

        #region Protected methods
        /// <inheritdoc />
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            if (!this.Request.Headers.TryGetValue(this.Options.Header,
                    out var values) || string.IsNullOrEmpty(values)) {
                this.Logger.LogWarning("A basic authentication header "
                    + "{Header} is not present, cannot proceed with "
                    + "authentication.", this.Options.Header);
                return AuthenticateResult.NoResult();
            }

            var header = values.FirstOrDefault();
            if (string.IsNullOrEmpty(header)) {
                this.Logger.LogWarning("A basic authentication header "
                    + "{Header} is present, but it contains no value.",
                    this.Options.Header);
                return AuthenticateResult.NoResult();
            }

            if (header == this.Options.AdvertisedScheme) {
                this.Logger.LogError("A basic authentication header "
                    + "{Header} with scheme {Scheme} is present, but "
                    + "contains no credentials.", this.Options.Header,
                    this.Options.AdvertisedScheme);
                return AuthenticateResult.Fail(string.Format(
                    Resources.ErrorNoCredentials, this.Options.Header,
                    this.Options.AdvertisedScheme));
            }

            if (!header.StartsWith(this.Options.AdvertisedScheme + ' ',
                    OrdinalIgnoreCase)) {
                this.Logger.LogWarning("The basic Authentication header "
                    + "{Header} does not start with the expected scheme "
                    + "{Scheme}.", this.Options.Header,
                    this.Options.AdvertisedScheme);
                return AuthenticateResult.NoResult();
            }

            byte[] base64 = null!;
            try {
                var v = header[this.Options.AdvertisedScheme.Length..].Trim();
                base64 = Convert.FromBase64String(v);
            } catch (Exception ex) {
                this.Logger.LogError(ex, "A basic authentication header "
                    + "{Header} with credentials is present, but the "
                    + "credentials could not be decoded.", this.Options.Header);
                return AuthenticateResult.Fail(string.Format(
                    Resources.ErrorNotBase64, this.Options.Header));
            }

            string credentials = null!;
            foreach (var e in this.Options.Encodings) {
                try {
                    if ((credentials = e.GetString(base64)) is not null) {
                        break;
                    }
                } catch (Exception ex) {
                    this.Logger.LogDebug(ex, "Failed to decode basic "
                        + "authentication credentials using {Encoding}.",
                        e.EncodingName);
                }
            }

            if (credentials is null) {
                this.Logger.LogError("The basic authentication credentials "
                    + "could not be decoded using any of {Encodings}.",
                    string.Join(", ", this.Options.Encodings));
                return AuthenticateResult.Fail(string.Format(
                    Resources.ErrorInvalidEncoding, this.Options.Header));
            }

            var parts = credentials.Split(':');
            if (parts.Length != 2) {
                this.Logger.LogError("The basic authentication credentials "
                    + "were successfully decoded, but did not contain the"
                    + " expected pair of user name and password.");
                return AuthenticateResult.Fail(string.Format(
                    Resources.ErrorInvalidFormat, this.Options.Header));
            }

            try {
                this.Logger.LogInformation("Authentication \"{User}\" with "
                    + "LDAP authentication service.", parts[0]);
                var principal = await this._ldap.LoginPrincipalAsync(parts[0],
                    parts[1], this.Options.ClaimFilter, CancellationToken.None);
                Debug.Assert(principal is not null);
                this.Logger.LogInformation("Issuing LDAP-based basic "
                    + "authentication ticket for {User} with scheme {Scheme}.",
                    principal.Identity?.Name, this.Scheme.Name);
                return AuthenticateResult.Success(new AuthenticationTicket(
                    principal, this.Scheme.Name));

            } catch (Exception ex) {
                this.Logger.LogError(ex, "Authentication \"{User}\" with "
                    + "LDAP authentication service failed.", parts[0]);
                return AuthenticateResult.Fail(ex);
            }
        }

        /// <inheritdoc />
        protected override Task HandleChallengeAsync(
                AuthenticationProperties properties) {
            if (!this.Request.IsHttps && !this.AllowUnencrypted) {
                this.Logger.LogError("Basic authentication was requested over "
                    + "an insecure connection. This can only be enabled in "
                    + "debug environments.");
                this.Response.StatusCode = StatusCodes.Status421MisdirectedRequest;
                return Task.CompletedTask;
            }

            this.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var sb = new StringBuilder(this.Options.AdvertisedScheme);
            if (!string.IsNullOrEmpty(this.Options.Realm)) {
                sb.Append(" realm=\"");
                sb.Append(this.Options.Realm);
                sb.Append("\"");

                if (this.Options.AdvertiseEncodings) {
                    foreach (var e in this.Options.Encodings) {
                        sb.Append(", encoding=\"");
                        sb.Append(e.EncodingName);
                        sb.Append("\"");
                    }
                }

                var header = sb.ToString();
                this.Logger.LogDebug("Sending {Header} {HeaderValue}",
                    HeaderNames.WWWAuthenticate, header);
                this.Response.Headers.Append(HeaderNames.WWWAuthenticate,
                    header);
            }

            return Task.CompletedTask;
        }
        #endregion

        #region Private properties
        /// <summary>
        /// Gets whether unencrypted basic authentication is allowed.
        /// </summary>
        private bool AllowUnencrypted {
            get {
#if DEBUG
                if (!this.Options.AllowUnencrypted) {
                    this.Logger.LogDebug("Unencrypted basic authentication is "
                        + "not allowed as per the configuration options.");
                    return false;
                }

                var env = Environment.GetEnvironmentVariable(
                    "ASPNETCORE_ENVIRONMENT");
                this.Logger.LogDebug("Unencrypted basic authentication is "
                    + "allowed via the configuration options, but only in "
                    + "development environments. The current environment is "
                    + "\"{Environment}\".", env);
                return string.Equals(env, "development", OrdinalIgnoreCase);
#else // DEBUG
                this.Logger.LogDebug("Unencrypted basic authentication is "
                    + "never allowed in release builds.");
                return false;
#endif // DEBUG
            }
        }
        #endregion

        #region Private fields
        private readonly ILdapAuthenticationService<TUser> _ldap
            = ldap ?? throw new ArgumentNullException(nameof(ldap));
        #endregion
    }
}
