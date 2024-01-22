// <copyright file="DateConverter.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// A converter that parses an attribute as <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <remarks>
    /// This converter is required, because ADDS stores some numbers as strings.
    /// </remarks>
    public sealed class DateConverter : ILdapAttributeConverter {

        #region Public methods
        /// <inheritdoc />
        public object Convert(DirectoryAttribute attribute, object parameter) {
            _ = attribute ?? throw new ArgumentNullException(nameof(attribute));
            var value = attribute[0] as string;

            try {
                var ticks = long.Parse(value);
                return DateTimeOffset.FromFileTime(ticks);
            } catch {
                return null;
            }
        }
        #endregion
    }
}
