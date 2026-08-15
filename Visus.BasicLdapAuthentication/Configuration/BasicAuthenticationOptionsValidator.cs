// <copyright file="BasicAuthenticationOptionsValidator.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2026 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using FluentValidation;
using System.Linq;
using Visus.BasicLdapAuthentication.Configuration;
using Visus.BasicLdapAuthentication.Properties;


namespace Visus.DirectoryAuthentication.Configuration {

    /// <summary>
    /// Validates the settings in <see cref="BasicAuthenticationOptions"/>.
    /// </summary>
    internal sealed class BasicAuthenticationOptionsValidator
            : AbstractValidator<BasicAuthenticationOptions> {

        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        public BasicAuthenticationOptionsValidator() {
            this.RuleFor(o => o.AdvertisedScheme)
                .NotEmpty();
#if !DEBUG
            this.RuleFor(o => o.AllowUnencrypted)
                .Must(o => !o)
                .WithMessage(Resources.ErrorHttpInRelease);
#endif // !DEBUG
            this.RuleFor(o => o.Encodings)
                .NotEmpty();
            this.RuleFor(o => o.Header)
                .NotEmpty();
            this.RuleFor(o => o.Realm)
                .Must(o => o!.All(c => c <= 127))
                .When(o => o.Realm is not null)
                .WithMessage(Resources.ErrorRealmNotAscii);
        }
    }
}
