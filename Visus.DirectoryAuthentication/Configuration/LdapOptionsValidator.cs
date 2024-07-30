// <copyright file="LdapOptionsValidator.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using FluentValidation;
using Visus.DirectoryAuthentication.Properties;
using Visus.Ldap.Configuration;


namespace Visus.DirectoryAuthentication.Configuration {

    /// <summary>
    /// Validates the settings in <see cref="LdapOptions"/>.
    /// </summary>
    internal sealed class LdapOptionsValidator
            : LdapOptionsValidatorBase<LdapOptions> {

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        public LdapOptionsValidator() {
            this.RuleFor(o => o.SearchBases).NotEmpty();
            this.RuleForEach(o => o.SearchBases)
                .Must(b => !string.IsNullOrWhiteSpace(b.Key))
                .When(o => o.SearchBases != null)
                .WithMessage(Resources.ErrorEmptySearchBase);
        }
    }
}
