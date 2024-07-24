// <copyright file="LdapRoleStore.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Visus.DirectoryAuthentication;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// A role store, which is based on security groups on the LDAP server.
    /// </summary>
    /// <typeparam name="TRole">The object used to represent a role (group).
    /// </typeparam>
    public class LdapRoleStore<TRole> : IQueryableRoleStore<TRole>,
            IRoleClaimStore<TRole>
            where TRole : class {

        /// <summary>
        /// Gets all groups in the directory.
        /// </summary>
        public IQueryable<TRole> Roles => throw new NotImplementedException();

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="claim"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task AddClaimAsync(TRole role,
                Claim claim,
                CancellationToken cancellationToken = default) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<IdentityResult> CreateAsync(TRole role,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<IdentityResult> DeleteAsync(TRole role,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose() {
            if (this._searchService != null) {
                this._searchService.Dispose();
                this._searchService = null;
            }
        }

        public Task<TRole> FindByIdAsync(string roleID,
                CancellationToken cancellationToken) {
            _ = roleID ?? throw new ArgumentNullException(nameof(roleID));
            throw new NotImplementedException();
        }

        public Task<TRole> FindByNameAsync(string normalisedRoleName,
                CancellationToken cancellationToken) {
            _ = normalisedRoleName
                ?? throw new ArgumentNullException(nameof(normalisedRoleName));
            throw new NotImplementedException();
        }

        public Task<IList<Claim>> GetClaimsAsync(TRole role,
                CancellationToken cancellationToken = default) {
            _ = role ?? throw new ArgumentNullException(nameof(role));
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role,
                CancellationToken cancellationToken) {
            _ = role ?? throw new ArgumentNullException(nameof(role));
            throw new NotImplementedException();
        }

        public Task<string> GetRoleIdAsync(TRole role,
                CancellationToken cancellationToken) {
            _ = role ?? throw new ArgumentNullException(nameof(role));
            throw new NotImplementedException();
        }

        public Task<string> GetRoleNameAsync(TRole role,
                CancellationToken cancellationToken) {
            _ = role ?? throw new ArgumentNullException(nameof(role));
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="claim"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task RemoveClaimAsync(TRole role,
                Claim claim,
                CancellationToken cancellationToken = default) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="normalizedName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetNormalizedRoleNameAsync(TRole role,
                string normalizedName,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="roleName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task SetRoleNameAsync(TRole role,
                string roleName,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented, because the current implementation
        /// does not allow for modifying the user database.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="NotImplementedException">Unconditionally.
        /// </exception>
        public Task<IdentityResult> UpdateAsync(TRole role,
                CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        #region Private fields
        private ILdapSearchService<LdapIdentityUser, LdapGroup> _searchService;
        #endregion
    }
}
