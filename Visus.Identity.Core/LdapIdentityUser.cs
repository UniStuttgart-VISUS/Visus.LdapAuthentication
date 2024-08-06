// <copyright file="LdapIdentityUser.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.Security.Claims;
using Visus.Ldap.Claims;
using Visus.Ldap.Mapping;
using Microsoft.AspNetCore.Identity;
using System;


namespace Visus.Identity {

    /// <summary>
    /// A basic representation of an identity user from LDAP.
    /// </summary>
    public class LdapIdentityUser {

        #region Public properties
        /// <summary>
        /// Gets or sets the number of failed login attempts for the user.
        /// </summary>
        [LdapAttribute("Active Directory", "badPwdCount",
            Converter = typeof(NumberConverter))]
        [LdapAttribute("IDMU", "badPwdCount",
            Converter = typeof(NumberConverter))]
        public int AccessFailedCount { get; set; }

        /// <summary>
        /// Gets or sets the Christian name of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "givenName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "givenName")]
        [LdapAttribute(Schema.Rfc2307, "givenName")]
        [Claim(ClaimTypes.GivenName)]
        [ProtectedPersonalData]
        public string? ChristianName { get; set; }

        /// <summary>
        /// Gets or sets the distinguished name of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "distinguishedName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "distinguishedName")]
        [LdapAttribute(Schema.Rfc2307, "distinguishedName")]
        [DistinguishedName]
        [ProtectedPersonalData]
        public string DistinguishedName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the e-mail address of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "mail")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "mail")]
        [LdapAttribute(Schema.Rfc2307, "mail")]
        [Claim(ClaimTypes.Email)]
        [ProtectedPersonalData]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the security identifier of the user, which is
        /// considered the unique primary key.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "objectSid",
            Converter = typeof(SidConverter))]
        [LdapAttribute(Schema.IdentityManagementForUnix, "uidNumber")]
        [LdapAttribute(Schema.Rfc2307, "uidNumber")]
        [Claim(ClaimTypes.PrimarySid)]
        [Claim(ClaimTypes.Sid)]
        [Claim(ClaimTypes.NameIdentifier)]
        [Identity]
        [ProtectedPersonalData]
        public string ID { get; set; } = null!;

        /// <summary>
        /// Gets or sets the point in time of the last failed logon attempt.
        /// </summary>
        [LdapAttribute("Active Directory", "badPasswordTime",
            Converter = typeof(FileTimeConverter))]
        [LdapAttribute("IDMU", "badPasswordTime",
            Converter = typeof(FileTimeConverter))]
        public DateTimeOffset? LastAccessFailed { get; set; }

        /// <summary>
        /// Gets a flag indicating if the user is be locked out.
        /// </summary>
        public bool LockoutEnabled => (this.LockoutEnd != null);

        /// <summary>
        /// Gets or sets the date and time, in UTC, when any user lockout ends.
        /// </summary>
        [LdapAttribute("Active Directory", "lockoutTime",
            Converter = typeof(FileTimeConverter))]
        [LdapAttribute("IDMU", "lockoutTime",
            Converter = typeof(FileTimeConverter))]
        public DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// Gets or sets a telephone number for the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "telephoneNumber")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "telephoneNumber")]
        [ProtectedPersonalData]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the surname of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "sn")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sn")]
        [LdapAttribute(Schema.Rfc2307, "sn")]
        [Claim(ClaimTypes.Surname)]
        [ProtectedPersonalData]
        public string? Surname { get; set; }

        /// <summary>
        /// Gets or sets the unique account name of the user.
        /// </summary>
        [LdapAttribute(Schema.ActiveDirectory, "sAMAccountName")]
        [LdapAttribute(Schema.IdentityManagementForUnix, "sAMAccountName")]
        [LdapAttribute(Schema.Rfc2307, "uid")]
        [Claim(ClaimTypes.Name)]
        [Claim(ClaimTypes.WindowsAccountName)]
        [AccountName]
        [ProtectedPersonalData]
        public string UserName { get; set; } = null!;
        #endregion

        #region Public methods
        /// <inheritdoc />
        public override string ToString() => this.UserName ?? string.Empty;
        #endregion
    }
}
