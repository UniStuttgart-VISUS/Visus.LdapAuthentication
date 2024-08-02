// <copyright file="LdapMappingValidator.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using FluentValidation;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// Validates <see cref="LdapMapping"/>.
    /// </summary>
    internal sealed class LdapMappingValidator : AbstractValidator<LdapMapping> {

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        public LdapMappingValidator() {
            this.RuleFor(m => m.DistinguishedNameAttribute).NotEmpty();
            this.RuleFor(m => m.GroupsAttribute).NotEmpty();
            this.RuleFor(m => m.GroupsFilter).NotEmpty();
            this.RuleFor(m => m.PrimaryGroupAttribute).NotEmpty();
            this.RuleFor(m => m.PrimaryGroupIdentityAttribute).NotEmpty();
            this.RuleFor(m => m.UserFilter).NotEmpty();
            this.RuleFor(m => m.UsersFilter).NotEmpty();
        }
    }
}
