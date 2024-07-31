// <copyright file="ILdapAuthenticationService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Visus.Ldap.Claims;


namespace Visus.Ldap {

    /// <summary>
    /// Interface for simple form-based authentication of users mapped to the
    /// specified type.
    /// </summary>
    /// <typeparam name="TUser">The type of user that is to be retrieved on
    /// successful login.</typeparam>
    public interface ILdapAuthenticationService<TUser> where TUser : class {

        /// <summary>
        /// Performs an asynchronous LDAP bind using the specified credentials
        /// and creates a <see cref="ClaimsPrincipal"/> from the claims annotated
        /// in the <typeparamref name="TUser"/> class.
        /// </summary>
        /// <remarks>
        /// <para>If the application only requires the annotated claims for
        /// authentication, this method is faster than mapping user objects and
        /// creating claims from there, because it can directly create the claims
        /// from the LDAP entry. Implementors should perform this direct mapping
        /// whenever possible.</para>
        /// </remarks>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="authenticationType">The authentication type used to
        /// construct the <see cref="ClaimsIdentity"/>. The authentication type
        /// is required to create a valid <see cref="ClaimsPrincipal"/>. If this
        /// parameter is <c>null</c>, implementations shall use their type name
        /// as the authentication type.</param>
        /// <param name="nameType">The name type used to construct the
        /// <see cref="ClaimsIdentity"/>.</param>
        /// <param name="roleType">The role type used to construct the
        /// <see cref="ClaimsIdentity"/>.</param>
        /// <param name="filter">If not <c>null</c>, this callback will receive
        /// all claims before they are added to the principal and can decide
        /// which ones should be added and which ones should not.</param>
        /// <returns>A principal in case of a successful login.</returns>
        ClaimsPrincipal LoginPrincipal(string username,
            string password,
            string? authenticationType = null,
            string? nameType = null,
            string? roleType = null,
            ClaimFilter? filter = null);

        /// <summary>
        /// Performs an asynchronous LDAP bind using the specified credentials
        /// and creates a <see cref="ClaimsPrincipal"/> from the claims annotated
        /// in the <typeparamref name="TUser"/> class.
        /// </summary>
        /// <remarks>
        /// <para>If the application only requires the annotated claims for
        /// authentication, this method is faster than mapping user objects and
        /// creating claims from there, because it can directly create the claims
        /// from the LDAP entry. Implementors should perform this direct mapping
        /// whenever possible.</para>
        /// </remarks>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="filter">If not <c>null</c>, this callback will receive
        /// all claims before they are added to the principal and can decide
        /// which ones should be added and which ones should not.</param>
        /// <returns>A principal in case of a successful login.</returns>
        ClaimsPrincipal LoginPrincipal(string username, string password,
                ClaimFilter? filter)
            => this.LoginPrincipal(username, password, null, null, null, filter);

        /// <summary>
        /// Performs an asynchronous LDAP bind using the specified credentials
        /// and creates a <see cref="ClaimsPrincipal"/> from the claims annotated
        /// in the <typeparamref name="TUser"/> class.
        /// </summary>
        /// <remarks>
        /// <para>If the application only requires the annotated claims for
        /// authentication, this method is faster than mapping user objects and
        /// creating claims from there, because it can directly create the claims
        /// from the LDAP entry. Implementors should perform this direct mapping
        /// whenever possible.</para>
        /// </remarks>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="authenticationType">The authentication type used to
        /// construct the <see cref="ClaimsIdentity"/>. The authentication type
        /// is required to create a valid <see cref="ClaimsPrincipal"/>. If this
        /// parameter is <c>null</c>, implementations shall use their type name
        /// as the authentication type.</param>
        /// <param name="nameType">The name type used to construct the
        /// <see cref="ClaimsIdentity"/>.</param>
        /// <param name="roleType">The role type used to construct the
        /// <see cref="ClaimsIdentity"/>.</param>
        /// <param name="filter">If not <c>null</c>, this callback will receive
        /// all claims before they are added to the principal and can decide
        /// which ones should be added and which ones should not.</param>
        /// <returns>A principal in case of a successful login.</returns>
        Task<ClaimsPrincipal> LoginPrincipalAsync(string username,
            string password,
            string? authenticationType = null,
            string? nameType = null,
            string? roleType = null,
            ClaimFilter? filter = null);

        /// <summary>
        /// Performs an asynchronous LDAP bind using the specified credentials
        /// and creates a <see cref="ClaimsPrincipal"/> from the claims annotated
        /// in the <typeparamref name="TUser"/> class.
        /// </summary>
        /// <remarks>
        /// <para>If the application only requires the annotated claims for
        /// authentication, this method is faster than mapping user objects and
        /// creating claims from there, because it can directly create the claims
        /// from the LDAP entry. Implementors should perform this direct mapping
        /// whenever possible.</para>
        /// </remarks>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="filter">If not <c>null</c>, this callback will receive
        /// all claims before they are added to the principal and can decide
        /// which ones should be added and which ones should not.</param>
        /// <returns>A principal in case of a successful login.</returns>
        Task<ClaimsPrincipal> LoginPrincipalAsync(string username,
                string password, ClaimFilter? filter)
            => this.LoginPrincipalAsync(username, password, null, null, null,
                filter);

        /// <summary>
        /// Performs an asynchronous LDAP bind using the specified credentials
        /// and retrieves the LDAP entry with the account name
        /// <paramref name="username"/> in case the bind succeeds.
        /// </summary>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The user object in case of a successful login.</returns>
        TUser LoginUser(string username, string password);

        /// <summary>
        /// Performs an asynchronous LDAP bind using the specified credentials,
        /// retrieves the LDAP entry with the account name
        /// <paramref name="username"/> in case the bind succeeds and returns
        /// the claims derived from the user object returned.
        /// </summary>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="filter">An optional filter that allows callers to remove
        /// claims they are not interested in from the retuned ones.</param>
        /// <returns>The user object and the claims derived from it in case of
        /// a successful login.</returns>
        (TUser, IEnumerable<Claim>) LoginUser(string username,
            string password, ClaimFilter? filter = null);

        /// <summary>
        /// Performs an LDAP bind using the specified credentials and retrieves
        /// the LDAP entry with the account name <paramref name="username"/> in
        /// case the bind succeeds.
        /// </summary>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The user object in case of a successful login.</returns>
        Task<TUser> LoginUserAsync(string username, string password);

        /// <summary>
        /// Performs an asynchronous LDAP bind using the specified credentials,
        /// retrieves the LDAP entry with the account name
        /// <paramref name="username"/> in case the bind succeeds and returns
        /// the claims derived from the user object returned.
        /// </summary>
        /// <param name="username">The user name to logon with.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="filter">An optional filter that allows callers to remove
        /// claims they are not interested in from the retuned ones.</param>
        /// <returns>The user object and the claims derived from it in case of
        /// a successful login.</returns>
        Task<(TUser, IEnumerable<Claim>)> LoginUserAsync(string username,
            string password, ClaimFilter? filter = null);
    }

}
