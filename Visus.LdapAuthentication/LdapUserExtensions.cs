// <copyright file="LdapUserExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System.Security.Claims;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Extension methods for <see cref="ILdapUser"/> and derived classes.
    /// </summary>
    public static class LdapUserExtensions {

        /// <summary>
        /// Creates a <see cref="ClaimsIdentity"/> for the given LDAP user.
        /// </summary>
        /// <remarks>
        /// All claims of <see cref="ILdapUser.Claims"/> from
        /// <paramref name="that"/> will be added to the identity and the
        /// authentication type will be set to
        /// <c>nameof(<see cref="ILdapAuthenticationService"/>)</c>.
        /// </remarks>
        /// <param name="that">The LDAP user to create the identity for. It is
        /// safe to pass <c>null</c>, in which case the return will be
        /// <c>null</c> as well.</param>
        /// <returns>The claims identity for the given user.</returns>
        public static ClaimsIdentity ToClaimsIdentity(this ILdapUser that) {
            var authType = nameof(ILdapAuthenticationService);

            return (that != null)
                ? new ClaimsIdentity(that.Claims, authType)
                : null;
        }

        /// <summary>
        /// Creates a <see cref="ClaimsPrincipal"/> for the given LDAP user.
        /// </summary>
        /// <param name="that">The LDAP user to create the principal for. It is
        /// safe to pass <c>null</c>, in which case the return will be
        /// <c>null</c> as well.</param>
        /// <returns>The principal for the given user.</returns>
        public static ClaimsPrincipal ToClaimsPrincipal(this ILdapUser that) {
            return (that != null)
                ? new ClaimsPrincipal(that.ToClaimsIdentity())
                : null;
        }
    }
}
