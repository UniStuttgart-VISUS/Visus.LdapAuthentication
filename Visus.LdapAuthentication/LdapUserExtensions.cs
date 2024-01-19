// <copyright file="LdapUserExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Linq;
using System.Security.Claims;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Extension methods for <see cref="ILdapUser"/> and derived classes.
    /// </summary>
    public static class LdapUserExtensions {

        /// <summary>
        /// Creates a <see cref="ClaimsIdentity"/> for the given LDAP user
        /// limiting the claims to the ones that pass <paramref name="filter"/>.
        /// </summary>
        /// <param name="that">The LDAP user to create the identity for. It is
        /// safe to pass <c>null</c>, in which case the return will be
        /// <c>null</c> as well.</param>
        /// <param name="authenticationType">The name of the authentication
        /// type used to create the <see cref="ClaimsIdentity"/>.</param>
        /// <param name="filter">A function that checks each claim before it
        /// is being added. It is safe to pass <c>null</c>, in which case
        /// all claims from <see cref="ILdapUser.Claims"/> will be added. This
        /// parameter defaults to <c>null</c>.
        /// </param>
        /// <returns>The claims identity for the given user.</returns>
        public static ClaimsIdentity ToClaimsIdentity(this ILdapUser that,
                string authenticationType,
                Func<Claim, bool> filter = null) {
            if (that == null) {
                // There is no user, so the identity is null, too.
                return null;
            }

            if (filter == null) {
                // There is no filter, so return all claims.
                return new ClaimsIdentity(that.Claims, authenticationType);
            }

            // Return filtered claims.
            var claims = from c in that.Claims
                         where filter(c)
                         select c;
            return new ClaimsIdentity(claims, authenticationType);
        }

        /// <summary>
        /// Creates a <see cref="ClaimsIdentity"/> for the given LDAP user
        /// limiting the claims to the ones that pass <paramref name="filter"/>.
        /// </summary>
        /// <remarks>
        /// The authentication type will be set to
        /// <c>nameof(<see cref="ILdapAuthenticationService"/>)</c>.
        /// </remarks>
        /// <param name="that">The LDAP user to create the identity for. It is
        /// safe to pass <c>null</c>, in which case the return will be
        /// <c>null</c> as well.</param>
        /// <param name="filter">A function that checks each claim before it
        /// is being added. It is safe to pass <c>null</c>, in which case
        /// all claims from <see cref="ILdapUser.Claims"/> will be added. This
        /// parameter defaults to <c>null</c>.
        /// </param>
        /// <returns>The claims identity for the given user.</returns>
        public static ClaimsIdentity ToClaimsIdentity(this ILdapUser that,
                Func<Claim, bool> filter = null) {
            return that.ToClaimsIdentity(nameof(ILdapAuthenticationService),
                filter);
        }

        /// <summary>
        /// Creates a <see cref="ClaimsPrincipal"/> for the given LDAP user.
        /// </summary>
        /// <param name="that">The LDAP user to create the principal for. It is
        /// safe to pass <c>null</c>, in which case the return will be
        /// <c>null</c> as well.</param>
        /// <param name="authenticationType">The name of the authentication
        /// type used to create the <see cref="ClaimsIdentity"/>.</param>
        /// <param name="filter">A function that checks each claim before it
        /// is being added. It is safe to pass <c>null</c>, in which case
        /// all claims from <see cref="ILdapUser.Claims"/> will be added. This
        /// parameter defaults to <c>null</c>.
        /// </param>
        /// <returns>The principal for the given user.</returns>
        public static ClaimsPrincipal ToClaimsPrincipal(this ILdapUser that,
                string authenticationType, Func<Claim, bool> filter = null) {
            if (that != null) {
                return new ClaimsPrincipal(that.ToClaimsIdentity(
                    authenticationType, filter));
            } else {
                return null;
            }
        }

        /// <summary>
        /// Creates a <see cref="ClaimsPrincipal"/> for the given LDAP user.
        /// </summary>
        /// <param name="that">The LDAP user to create the principal for. It is
        /// safe to pass <c>null</c>, in which case the return will be
        /// <c>null</c> as well.</param>
        /// <param name="filter">A function that checks each claim before it
        /// is being added. It is safe to pass <c>null</c>, in which case
        /// all claims from <see cref="ILdapUser.Claims"/> will be added. This
        /// parameter defaults to <c>null</c>.
        /// </param>
        /// <returns>The principal for the given user.</returns>
        public static ClaimsPrincipal ToClaimsPrincipal(this ILdapUser that,
                Func<Claim, bool> filter = null) {
            return (that != null)
                ? new ClaimsPrincipal(that.ToClaimsIdentity(filter))
                : null;
        }
    }
}
