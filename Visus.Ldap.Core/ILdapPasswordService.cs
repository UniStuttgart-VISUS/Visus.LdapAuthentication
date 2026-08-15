// <copyright file="ILdapPasswordService.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2025 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Threading.Tasks;


namespace Visus.Ldap {

    /// <summary>
    /// Interface for a service that enables changing user and machine passwords
    /// on an LDAP service.
    /// </summary>
    public interface ILdapPasswordService {

        /// <summary>
        /// Changes the password of the account identified by the given
        /// <paramref name="userName"/>.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called in the context of the account
        /// owner for changing his own password.
        /// </remarks>
        /// <param name="userName">The name of the user or machine account to
        /// change the password for.</param>
        /// <param name="oldPassword">The old password used by the account.
        /// </param>
        /// <param name="newPassword">The new password for the account. The
        /// caller is responsible for making sure that this is as expected by
        /// checking a repeated input.</param>
        void ChangePassword(string userName, string oldPassword,
            string newPassword);

        /// <summary>
        /// Changes the password of the account identified by the given
        /// <paramref name="userName"/>.
        /// </summary>
        /// <remarks>
        /// This method is intended to be called in the context of the account
        /// owner for changing his own password.
        /// </remarks>
        /// <param name="userName">The name of the user or machine account to
        /// change the password for.</param>
        /// <param name="oldPassword">The old password used by the account.
        /// </param>
        /// <param name="newPassword">The new password for the account. The
        /// caller is responsible for making sure that this is as expected by
        /// checking a repeated input.</param>
        /// <returns>A task for awaiting the operation.</returns>
        Task ChangePasswordAsync(string userName, string oldPassword,
            string newPassword);

        /// <summary>
        /// Changes the password of the account identified by the given
        /// <paramref name="userName"/>.
        /// </summary>
        /// <remarks>
        /// The caller must have administrative privileges on the directory to
        /// call this method successfully.
        /// </remarks>
        /// <param name="userName">The name of the user or machine account to
        /// change the password for.</param>
        /// <param name="newPassword">The new password for the account. The
        /// caller is responsible for making sure that this is as expected by
        /// checking a repeated input.</param>
        void ChangePassword(string userName, string newPassword);

        /// <summary>
        /// Changes the password of the account identified by the given
        /// <paramref name="userName"/>.
        /// </summary>
        /// <remarks>
        /// The caller must have administrative privileges on the directory to
        /// call this method successfully.
        /// </remarks>
        /// <param name="userName">The name of the user or machine account to
        /// change the password for.</param>
        /// <param name="newPassword">The new password for the account. The
        /// caller is responsible for making sure that this is as expected by
        /// checking a repeated input.</param>
        /// <returns>A task for awaiting the operation.</returns>
        Task ChangePasswordAsync(string userName, string newPassword);
    }

}
