// <copyright file="ClaimAttribute.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using System;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Marks a property as a claim that is automatically added by
    /// <see cref="LdapUserBase"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class ClaimAttribute : Attribute {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="name">The name of the claim.</param>
        public ClaimAttribute(string name) {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the name of the claim.
        /// </summary>
        public string Name { get; }
        #endregion
    }
}
