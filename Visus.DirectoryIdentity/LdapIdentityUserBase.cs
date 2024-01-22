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
        [LdapAttribute("Active Directory", "badPwdCount",
            Converter = typeof(IntConverter))]
        [LdapAttribute("IDMU", "badPwdCount",
            Converter = typeof(IntConverter))]
        public virtual int AccessFailedCount { get; internal set; } = 0;

        /// <inheritdoc />
        [LdapAttribute("Active Directory", "lockedOut")]
        [LdapAttribute("IDMU", "lockedOut")]
        public bool IsLockoutEnabled { get; internal set; } = false;

        /// <inheritdoc />
        [LdapAttribute("Active Directory", "accountLockoutTime",
            Converter = typeof(DateConverter))]
        [LdapAttribute("IDMU", "accountLockoutTime",
            Converter = typeof(DateConverter))]
        public virtual DateTimeOffset? LockoutTime { get; internal set; }

        /// <inheritdoc />
        [LdapAttribute("Active Directory", "badPasswordTime",
            Converter = typeof(DateConverter))]
        [LdapAttribute("IDMU", "badPasswordTime",
            Converter = typeof(DateConverter))]
        public virtual DateTimeOffset? LastAccessFailed { get; internal set; }
    }
}
