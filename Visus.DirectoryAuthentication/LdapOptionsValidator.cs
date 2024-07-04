// <copyright file="LdapOptionsValidator.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using FluentValidation;
using FluentValidation.Results;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Validates the settings in <see cref="LdapOptions"/>.
    /// </summary>
    internal sealed class LdapOptionsValidator
            : AbstractValidator<LdapOptions> {

        /// <inheritdoc />
        public override ValidationResult Validate(
                ValidationContext<LdapOptions> context) {
            this.RuleFor(context => context.Mapping).NotNull()
                .SetValidator(new LdapMappingValidator());
            this.RuleFor(context => context.Schema).NotEmpty();
            this.RuleFor(context => context.SearchBases).NotEmpty();
            this.RuleFor(context => context.Servers).NotEmpty();
            return base.Validate(context);
        }
    }
}
