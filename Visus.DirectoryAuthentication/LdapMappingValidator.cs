// <copyright file="LdapMappingValidator.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using FluentValidation;
using FluentValidation.Results;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Validates the propertes of a <see cref="LdapMapping"/>.
    /// </summary>
    internal sealed class LdapMappingValidator
            : AbstractValidator<LdapMapping> {

        /// <inheritdoc />
        public override ValidationResult Validate(
                ValidationContext<LdapMapping> context) {
            this.RuleFor(context => context.DistinguishedNameAttribute).NotEmpty();
            this.RuleFor(context => context.GroupsAttribute).NotEmpty();
            this.RuleFor(context => context.PrimaryGroupAttribute).NotEmpty();
            this.RuleFor(context => context.UserFilter).NotEmpty();
            this.RuleFor(context => context.UsersFilter).NotEmpty();
            return base.Validate(context);
        }
    }
}
