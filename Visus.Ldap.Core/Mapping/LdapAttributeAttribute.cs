// <copyright file="LdapAttributeAttribute.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// Annotates properties of a user or group class to enable an
    /// <see cref="ILdapMapper{TEntry, TUser, TGroup}"/> retrieving the
    /// property values automatically from an LDAP entry.
    /// </summary>
    /// <remarks>
    /// Callers that need to reflect on the LDAP attributes of a user-defined
    /// type should rely on <see cref="IClaimMap{TObject}"/> to obtain
    /// this information.
    /// </remarks>
    [DebuggerDisplay("\\{Name = {Name}, Schema = {Schema}\\}")]
    [AttributeUsage(AttributeTargets.Property,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class LdapAttributeAttribute : Attribute,
            IEquatable<LdapAttributeAttribute> {

        #region Public constructors
        /// <summary>
        /// Initialises a new instance.
        /// </summary>
        /// <param name="schema">The name of the schema the mapping is valid
        /// for.</param>
        /// <param name="name">The name of the LDAP attribute to lookup for the
        /// annotated property.</param>
        public LdapAttributeAttribute(string schema, string name) {
            this.Name = name
                ?? throw new ArgumentNullException(nameof(name));
            this.Schema = schema
                ?? throw new ArgumentNullException(nameof(schema));
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets or sets the type of an optional converter transforming the
        /// attribute into a usable form.
        /// </summary>
        public Type? Converter { get; set; }

        /// <summary>
        /// Gets the name of the LDAP attribute storing the value of the
        /// property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the name of the LDAP schema the mapping is valid for.
        /// </summary>
        public string Schema { get; }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public bool Equals(LdapAttributeAttribute? other) {
            if (other == null) {
                return false;
            }

            if (this.Name != other.Name) {
                return false;
            }

            if (this.Schema != other.Schema) {
                return false;
            }

            if (this.Converter != other.Converter) {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override bool Equals([NotNullWhen(true)] object? obj)
            => this.Equals(obj as LdapAttributeAttribute);

        /// <summary>
        /// Instantiates the converter if any.
        /// </summary>
        /// <returns>A <see cref="IValueConverter"/> or <c>null</c> if no
        /// converter was annotated.</returns>
        public IValueConverter? GetConverter() {
            if ((this.Converter != null) && (this._converter == null)) {
                this._converter = Activator.CreateInstance(this.Converter)
                    as IValueConverter;
            }

            return this._converter;
        }

        /// <inheritdoc />
        public override int GetHashCode()
            => base.GetHashCode()
            ^ this.Name.GetHashCode()
            ^ this.Schema.GetHashCode()
            ^ this.Converter?.GetHashCode() ?? 0;
        #endregion

        #region Private fields
        private IValueConverter? _converter;
        #endregion
    }
}
