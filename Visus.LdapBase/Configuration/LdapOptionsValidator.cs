// <copyright file="LdapOptionsValidator.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using FluentValidation;
using FluentValidation.Results;
using Visus.Ldap.Configuration;
using Visus.Ldap.Mapping;
using Visus.Ldap.Properties;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Validates <see cref="LdapOptionsBase"/>.
    /// </summary>
    internal sealed class LdapOptionsValidator
            : AbstractValidator<LdapOptionsBase> {

        /// <inheritdoc />
        public override ValidationResult Validate(
                ValidationContext<LdapOptionsBase> context) {
            this.RuleFor(context => context.Mapping)
                .NotNull()
                .SetValidator(new LdapMappingValidator()!);
            this.RuleFor(context => context.Mappings).NotNull();
            // Note: The content of Mapping*s* is optional, only the active
            // *Mapping* is relevant for the library to function correctly.
            this.RuleFor(context => context.Schema).NotEmpty();
            //this.RuleFor(context => context.SearchBases).NotEmpty();
            //this.RuleForEach(context => context.SearchBases)
            //    .Must(b => !string.IsNullOrWhiteSpace(b.Key))
            //    .When(context => context.SearchBases != null)
            //    .WithMessage(Resources.ErrorEmptySearchBase);
            this.RuleFor(context => context.Servers).NotEmpty();
            this.RuleForEach(context => context.Servers)
                .NotEmpty();
            this.RuleFor(context => context.Password)
                .NotNull()
                .When(o => !string.IsNullOrWhiteSpace(o.User))
                .WithMessage(Resources.ErrorEmptyPassword);

            return base.Validate(context);
        }
    }
}
