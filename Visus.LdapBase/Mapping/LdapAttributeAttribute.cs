// <copyright file="LdapAttributeAttribute.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LdapAttributeMap = System.Collections.Generic.Dictionary<
    System.Reflection.PropertyInfo,
    Visus.Ldap.Mapping.LdapAttributeAttribute>;


namespace Visus.Ldap.Mapping {

    /// <summary>
    /// Annotates properties of a user or group class to enable an
    /// <see cref="ILdapMapper{TEntry, TUser, TGroup}"/> retrieving the
    /// property values automatically from an LDAP entry.
    /// </summary>
    [DebuggerDisplay("\\{Name = {Name}, Schema = {Schema}\\}")]
    [AttributeUsage(AttributeTargets.Property,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class LdapAttributeAttribute : Attribute {

        #region Public class methods
        /// <summary>
        /// Gets, if any, the <see cref="LdapAttributeAttribute"/> for the given
        /// property and schema.
        /// </summary>
        /// <param name="property">The <see cref="PropertyInfo"/> of the
        /// property to retrieve the attribute for.</param>
        /// <param name="schema">The LDAP schema to retrieve the attribute for.
        /// </param>
        /// <returns>The attribute for the given schema or <c>null</c> if no such
        /// attribute was found.</returns>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="property"/> is <c>null</c>.</exception>
        public static LdapAttributeAttribute? GetLdapAttribute(
                PropertyInfo property, string schema) {
            ArgumentNullException.ThrowIfNull(property, nameof(property));
            var retval = (from a in property.GetCustomAttributes<LdapAttributeAttribute>()
                          where a.Schema == schema
                          select a).FirstOrDefault();
            return retval;
        }

        /// <summary>
        /// Gets, if any, the <see cref="LdapAttributeAttribute"/> for the given
        /// property of the given type.
        /// </summary>
        /// <param name="type">The type to retrieve the attribute for.</param>
        /// <param name="property">The name of the property to retrieve the
        /// attribute of. It is safe to specify the name of a property that does
        /// not exist or is not a string property. In this case, the method will
        /// return <c>null</c>.</param>
        /// <param name="schema">The LDAP schema to retrieve the attribute for.
        /// </param>
        /// <returns>The attribute for the given schema or <c>null</c> if no such
        /// attribute was found.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="type"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="property"/> is <c>null</c>.</exception>
        public static LdapAttributeAttribute? GetLdapAttribute(Type type,
                string property, string schema) {
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            var prop = type.GetProperty(property);

            if ((prop != null) && (prop.PropertyType == typeof(string))) {
                return GetLdapAttribute(prop, schema);

            } else {
                return null;
            }
        }

        /// <summary>
        /// Gets, if any, the <see cref="LdapAttributeAttribute"/> for the given
        /// property of the given type.
        /// </summary>
        /// <typeparam name="TType">The type to retrieve the attribute for.
        /// </typeparam>
        /// <param name="property">The name of the property to retrieve the
        /// attribute of. It is safe to specify the name of a property that does
        /// not exist or is not a string property. In this case, the method will
        /// return <c>null</c>.</param>
        /// <param name="schema">The LDAP schema to retrieve the attribute for.
        /// </param>
        /// <returns>The attribute for the given schema or <c>null</c> if no such
        /// attribute was found.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="type"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">If
        /// <paramref name="property"/> is <c>null</c>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LdapAttributeAttribute? GetLdapAttribute<TType>(
                string property, string schema) 
            => GetLdapAttribute(typeof(TType), property, schema);

        /// <summary>
        /// Gets all annotated and string-convertible properties of
        /// <paramref name="type"/> that support the given schema.
        /// </summary>
        /// <param name="type">The type to retrieve the attributes for.</param>
        /// <param name="schema">The LDAP schema to retrieve the attribute for.
        /// </param>
        /// <returns>The properties and their attributes.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="type"/>
        /// is <c>null</c>.</exception>
        public static LdapAttributeMap GetMap(Type type, string schema) {
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            return (from p in type.GetProperties()
                    let a = p.GetCustomAttributes<LdapAttributeAttribute>()
                        .Where(a => a.Schema == schema)
                        .FirstOrDefault()
                    where a != null
                    select new {
                        Property = p,
                        Attribute = a
                    }).ToDictionary(v => v.Property, v => v.Attribute);
        }

        /// <summary>
        /// Gets all annotated and string-convertible properties of
        /// <typeparamref name="TType"/> that support the given schema.
        /// </summary>
        /// <typeparam name="TType">The type to retrieve the attributes for.
        /// </typeparam>
        /// <param name="schema">he LDAP schema to retrieve the attribute for.
        /// </param>
        /// <returns>The properties and their attributes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LdapAttributeMap GetMap<TType>(string schema)
            => GetMap(typeof(TType), schema);

        /// <summary>
        /// Gets all annodated properties of <typeparamref name="T1"> and
        /// <typeparamref name="T2"/> that support the given schema.
        /// </summary>
        /// <typeparam name="T1">A type to retrieve the attributes for.
        /// </typeparam>
        /// <typeparam name="T2">A type to retrieve the attributes for.
        /// </typeparam>
        /// <param name="schema">he LDAP schema to retrieve the attribute for.
        /// </param>
        /// <returns>The properties and their attributes for
        /// <typeparamref name="T1"/> and <typeparamref name="T2"/>.
        /// </returns>
        public static (LdapAttributeMap, LdapAttributeMap) GetMap<T1, T2>(
                string schema)
            => (GetMap(typeof(T1), schema), GetMap(typeof(T2), schema));

        /// <summary>
        /// Gets the LDAP attributes that are required to populate all
        /// properties of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the user object.</typeparam>
        /// <returns>A list of attribute names that must be loaded for the LDAP
        /// entries.</returns>
        public static IEnumerable<string> GetRequiredAttributes(Type type,
                string schema) {
            ArgumentNullException.ThrowIfNull(type, nameof(type));
            return (from p in type.GetProperties()
                    let a = GetLdapAttribute(p, schema)
                    where a != null
                    select a.Name)
                    .Distinct();
        }

        /// <summary>
        /// Gets the LDAP attributes that are required to populate all
        /// properties of <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">The type of the user or group object.
        /// </typeparam>
        /// <returns>A list of attribute names that must be loaded for the LDAP
        /// entries.</returns>
        public static IEnumerable<string> GetRequiredAttributes<TObject>(
                string schema)
            => (from p in typeof(TObject).GetProperties()
                let a = GetLdapAttribute(p, schema)
                where a != null
                select a.Name)
                .Distinct();
        #endregion

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
        #endregion

        #region Private fields
        private IValueConverter? _converter;
        #endregion
    }
}
