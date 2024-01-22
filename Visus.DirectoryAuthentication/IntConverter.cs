// <copyright file="IntConverter.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// A converter that parses a string attribute into an <see cref="byte"/>,
    /// or performs a bit conversion on a byte array to <see cref="byte"/>,
    /// <see cref="short"/>, <see cref="int"/> or <see cref="long"/>.
    /// </summary>
    /// <remarks>
    /// This converter is required, because ADDS stores some numbers as strings.
    /// </remarks>
    public sealed class IntConverter : ILdapAttributeConverter {

        #region Public methods
        /// <inheritdoc />
        public object Convert(DirectoryAttribute attribute, object parameter) {
            _ = attribute ?? throw new ArgumentNullException(nameof(attribute));

            switch(attribute[0]) {
                case string s:
                    return int.Parse(s);

                case byte[] b:
                    switch (b.Length) {
                        case 1: return b[0];
                        case 2: return BitConverter.ToInt16(b);
                        case 4: return BitConverter.ToInt32(b);
                        case 8: return BitConverter.ToInt64(b);
                        default: return null;
                    }

                default:
                    return null;
            }
        }
        #endregion
    }
}
