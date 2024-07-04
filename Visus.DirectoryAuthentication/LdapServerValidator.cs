// <copyright file="LdapServerValidator.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using FluentValidation;
using FluentValidation.Results;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Ensures that the properties of a <see cref="LdapServer"/> are
    /// reasonable.
    /// </summary>
    internal sealed class LdapServerValidator : AbstractValidator<LdapServer> {

        /// <inheritdoc />
        public override ValidationResult Validate(
                ValidationContext<LdapServer> context) {
            this.RuleFor(context => context.Address).NotEmpty();
            this.RuleFor(context => context.Port).LessThan(ushort.MaxValue);
            return base.Validate(context);
        }
    }
}
