// <copyright file="ValidateLdapOptions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Options;
using System;
using System.Linq;


namespace Visus.DirectoryAuthentication {


    /// <summary>
    /// Wraps the <see cref="LdapOptionsValidator"/> in a
    /// <see cref="IValidateOptions{TOptions}"/>.
    /// </summary>
    internal sealed class ValidateLdapOptions : IValidateOptions<LdapOptions> {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="validator"></param>
        public ValidateLdapOptions(LdapOptionsValidator validator = null) {
            this._validator = validator ?? new();
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public ValidateOptionsResult Validate(string name,
                LdapOptions options) {
            _ = options ?? throw new ArgumentNullException(nameof(options));

            var result = this._validator.Validate(options);

            return result.IsValid
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(result.Errors.Select(e => e.ErrorMessage));
        }
        #endregion

        #region Private fields
        private readonly LdapOptionsValidator _validator;
        #endregion
    }
}
