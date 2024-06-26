﻿// <copyright file="ILdapAttributeConverter.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System.DirectoryServices.Protocols;


namespace Visus.DirectoryAuthentication {

    /// <summary>
    /// Converts an <see cref="LdapAttribute"/> into a string representation
    /// that can be used as property of an ldap user object or as a
    /// <see cref="System.Security.Claims.Claim"/>.
    /// </summary>
    public interface ILdapAttributeConverter {

        /// <summary>
        /// Converts <paramref name="attribute"/> into a user-defined object,
        /// normally a string.
        /// </summary>
        /// <param name="attribute">The attribute to be converted.</param>
        /// <param name="parameter">An optional converter parameter.</param>
        /// <returns>The converted object.</returns>
        object Convert(DirectoryAttribute attribute, object parameter);
    }
}
