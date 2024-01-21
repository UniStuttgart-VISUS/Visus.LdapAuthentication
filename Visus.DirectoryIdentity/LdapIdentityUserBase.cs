// <copyright file="LdapIdentityUserBase.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using Visus.DirectoryAuthentication;


namespace Visus.DirectoryIdentity {

    /// <summary>
    /// Basic implementation of <see cref="ILdapIdentityUser"/>.
    /// </summary>
    public abstract class LdapIdentityUserBase : LdapUserBase,
            ILdapIdentityUser {

        /// <inheritdoc />
        [LdapAttribute("Active Directory", "BadPwdCount")]
        [LdapAttribute("IDMU", "BadPwdCount")]
        public virtual int AccessFailedCount { get; internal set; } = 0;

        /// <inheritdoc />
        [LdapAttribute("Active Directory", "LockedOut")]
        [LdapAttribute("IDMU", "LockedOut")]
        public bool IsLockoutEnabled { get; internal set; } = false;

        /// <inheritdoc />
        [LdapAttribute("Active Directory", "AccountLockoutTime")]
        [LdapAttribute("IDMU", "AccountLockoutTime")]
        public virtual DateTimeOffset? LockoutTime { get; internal set; }

        /// <inheritdoc />
        [LdapAttribute("Active Directory", "LastBadPasswordAttempt")]
        [LdapAttribute("IDMU", "LastBadPasswordAttempt")]
        public virtual DateTimeOffset? LastAccessFailed { get; internal set; }
    }
}
