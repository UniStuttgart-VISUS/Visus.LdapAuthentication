// <copyright file="LdapOptionsValidator.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using FluentValidation;
using FluentValidation.Results;
using Visus.DirectoryAuthentication.Properties;


namespace Visus.DirectoryAuthentication.Configuration {

    /// <summary>
    /// Validates the settings in <see cref="LdapOptions"/>.
    /// </summary>
    internal sealed class LdapOptionsValidator
            : AbstractValidator<LdapOptions> {

        /// <inheritdoc />
        public override ValidationResult Validate(
                ValidationContext<LdapOptions> context) {
            RuleFor(context => context.Mapping)
                .NotNull()
                .SetValidator(new LdapMappingValidator());
            RuleFor(context => context.Mappings).NotNull();
            // Note: The content of Mapping*s* is optional, only the active
            // *Mapping* is relevant for the library to function correctly.
            RuleFor(context => context.Schema).NotEmpty();
            RuleFor(context => context.SearchBases).NotEmpty();
            RuleForEach(context => context.SearchBases)
                .Must(b => !string.IsNullOrWhiteSpace(b.Key))
                .When(context => context.SearchBases != null)
                .WithMessage(Resources.ErrorEmptySearchBase);
            RuleFor(context => context.Servers).NotEmpty();
            RuleForEach(context => context.Servers)
                .NotEmpty();
            RuleFor(context => context.Password)
                .NotNull()
                .When(o => !string.IsNullOrWhiteSpace(o.User))
                .WithMessage(Resources.ErrorEmptyPassword);

            return base.Validate(context);
        }
    }
}
