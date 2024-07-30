// <copyright file="FluentValidateOptions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using FluentValidation;
using Microsoft.Extensions.Options;
using System;
using System.Linq;


namespace Visus.Ldap.Configuration {

    /// <summary>
    /// Wraps a <typeparamref name="TValidator"/> in a
    /// <see cref="IValidateOptions{TOptions}"/> object.
    /// </summary>
    public sealed class FluentValidateOptions<TOptions, TValidator>
            : IValidateOptions<TOptions>
            where TOptions : class
            where TValidator : AbstractValidator<TOptions>, new() {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="validator"></param>
        public FluentValidateOptions(TValidator validator) {
            this._validator = validator ?? new();
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public ValidateOptionsResult Validate(string? name, TOptions options) {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            var result = this._validator.Validate(options);

            return result.IsValid
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(result.Errors.Select(e => e.ErrorMessage));
        }
        #endregion

        #region Private fields
        private readonly TValidator _validator;
        #endregion
    }
}
