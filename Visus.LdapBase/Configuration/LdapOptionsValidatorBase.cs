// <copyright file="LdapOptionsBaseValidator.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using FluentValidation;
using Visus.Ldap.Mapping;
using Visus.Ldap.Properties;


namespace Visus.Ldap.Configuration {

    /// <summary>
    /// Validates <see cref="LdapOptionsBase"/> and servers as the base class
    /// for the LDAP library-specific validators.
    /// </summary>
    public abstract class LdapOptionsValidatorBase<TOptions>
            : AbstractValidator<TOptions>
            where TOptions : LdapOptionsBase {

        /// <summary>
        /// Initialises a new instance
        /// </summary>
        protected LdapOptionsValidatorBase() {
            this.RuleFor(o => o.Mapping)
                .NotNull()
                .SetValidator(new LdapMappingValidator()!);
            this.RuleFor(o => o.Mappings).NotNull();
            // Note: The content of Mapping*s* is optional, only the active
            // *Mapping* is relevant for the library to function correctly.
            this.RuleFor(o => o.Schema).NotEmpty();
            //this.RuleFor(context => context.SearchBases).NotEmpty();
            //this.RuleForEach(context => context.SearchBases)
            //    .Must(b => !string.IsNullOrWhiteSpace(b.Key))
            //    .When(context => context.SearchBases != null)
            //    .WithMessage(Resources.ErrorEmptySearchBase);
            this.RuleFor(o => o.Servers).NotEmpty();
            this.RuleForEach(o => o.Servers)
                .NotEmpty();
            this.RuleFor(o => o.Password)
                .NotNull()
                .When(o => !string.IsNullOrWhiteSpace(o.User))
                .WithMessage(Resources.ErrorEmptyPassword);
        }
    }
}
