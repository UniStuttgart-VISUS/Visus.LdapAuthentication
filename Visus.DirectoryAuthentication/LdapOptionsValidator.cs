// <copyright file="LdapOptionsValidator.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using FluentValidation;
using FluentValidation.Results;
using Visus.DirectoryAuthentication.Properties;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Validates the settings in <see cref="LdapOptions"/>.
    /// </summary>
    internal sealed class LdapOptionsValidator
            : AbstractValidator<LdapOptions> {

        /// <inheritdoc />
        public override ValidationResult Validate(
                ValidationContext<LdapOptions> context) {
            this.RuleFor(context => context.Mapping)
                .NotNull()
                .SetValidator(new LdapMappingValidator());
            this.RuleFor(context => context.Mappings).NotNull();
            // Note: The content of Mapping*s* is optional, only the active
            // *Mapping* is relevant for the library to function correctly.
            this.RuleFor(context => context.Schema).NotEmpty();
            this.RuleFor(context => context.SearchBases).NotEmpty();
            this.RuleForEach(context => context.SearchBases)
                .Must(b => !string.IsNullOrWhiteSpace(b.Key))
                .When(context => context.SearchBases != null)
                .WithMessage(Resources.ErrorEmptySearchBase);
            this.RuleFor(context => context.Servers).NotEmpty();
            this.RuleForEach(context => context.Servers)
                .SetValidator(new LdapServerValidator())
                .When(context => context.Servers != null);

            return base.Validate(context);
        }
    }
}
