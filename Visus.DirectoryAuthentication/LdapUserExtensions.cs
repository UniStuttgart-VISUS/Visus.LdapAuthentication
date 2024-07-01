// <copyright file="LdapUserExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 -2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Extension methods for LDAP user classes.
    /// </summary>
    public static class LdapUserExtensions {

        /// <summary>
        /// Creates a <see cref="ClaimsIdentity"/> for the given LDAP user
        /// limiting the claims to the ones that pass <paramref name="filter"/>.
        /// </summary>
        /// <remarks>
        /// The authentication type will be set to
        /// <c>nameof(<see cref="ILdapAuthenticationService{TUser}"/>)</c>
        /// unless it is explicitly specified as
        /// <paramref name="authenticationType"/>.
        /// </remarks>
        /// <param name="that">The LDAP user to create the identity for. It is
        /// safe to pass <c>null</c>, in which case the return will be
        /// <c>null</c> as well.</param>
        /// <param name="filter">A function that checks each claim before it
        /// is being added. It is safe to pass <c>null</c>, in which case
        /// all claims from the active
        /// <see cref="IClaimsBuilder{TUser, TGroup}"/> will be added. This
        /// parameter defaults to <c>null</c>.
        /// </param>
        /// <param name="authenticationType">The authentication type to be used
        /// in the <see cref="ClaimsIdentity"/>.</param>
        /// <returns>The claims identity for the given user.</returns>
        public static ClaimsIdentity ToClaimsIdentity<TUser>(this TUser that,
                Func<Claim, bool> filter = null,
                string authenticationType = null)
                where TUser : class {
            if (that == null) {
                // There is no user, so the identity is null, too.
                return null;
            }

            var property = ClaimsAttribute.GetClaims<TUser>();
            if (property == null) {
                // If the user has no claims, we cannot create the identity.
                return null;
            }

            var claims = property.GetValue(that) as IEnumerable<Claim>;
            if (claims == null) {
                // If the claims are invalid, we cannot create the identity.
                return null;
            }

            if (authenticationType == null) {
                authenticationType = nameof(ILdapAuthenticationService<TUser>);
            }

            if (filter == null) {
                // There is no filter, so return all claims.
                return new ClaimsIdentity(claims, authenticationType);
            }

            // Return filtered claims.
            claims = from c in claims
                     where filter(c)
                     select c;
            return new ClaimsIdentity(claims, authenticationType);
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
        public static ClaimsPrincipal ToClaimsPrincipal<TUser>(
                this TUser that,
                Func<Claim, bool> filter = null,
                string authenticationType = null)
                where TUser : class {
            return (that != null)
                ? new ClaimsPrincipal(that.ToClaimsIdentity(filter,
                    authenticationType))
                : null;
        }
    }
}
