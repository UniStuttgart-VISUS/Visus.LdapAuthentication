// <copyright file="ILdapIdentityUser.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using Visus.DirectoryAuthentication;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// The interface for an LDAP user object to be used with the identity
    /// store.
    /// </summary>
    public interface ILdapIdentityUser : ILdapUser {

        /// <summary>
        /// Gets the number of failed login attempts.
        /// </summary>
        int AccessFailedCount { get; }

        /// <summary>
        /// Gets or sets whether the user is locked out.
        /// </summary>
        bool IsLockoutEnabled { get; }

        /// <summary>
        /// Gets the time until which the account is locked.
        /// </summary>
        DateTimeOffset? LockoutTime { get; }

        /// <summary>
        /// Gets the time of the last unsuccessful login time.
        /// </summary>
        DateTimeOffset? LastAccessFailed { get; }

        /// <summary>
        /// Gets the phone number of the user.
        /// </summary>
        string PhoneNumber { get; }
    }
}
