// <copyright file="LdapAttributeAttribute.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Visus.LdapAuthentication {

    /// <summary>
    /// Annotates properties of a <see cref="LdapUserBase"/> to enable the class
    /// retrieving the property values automatically from a
    /// <see cref="Novell.Directory.Ldap.LdapEntry"/>.
    /// </summary>
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
        public static LdapAttributeAttribute GetLdapAttribute(
                PropertyInfo property, string schema) {
            _ = property ?? throw new ArgumentNullException(nameof(property));

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
        public static LdapAttributeAttribute GetLdapAttribute(Type type,
                string property, string schema) {
            _ = type ?? throw new ArgumentNullException(nameof(type));
            _ = property ?? throw new ArgumentNullException(nameof(property));

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
        public static LdapAttributeAttribute GetLdapAttribute<TType>(
                    string property, string schema) {
            return GetLdapAttribute(typeof(TType), property, schema);
        }

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
        public static Dictionary<PropertyInfo, LdapAttributeAttribute>
        GetLdapProperties(Type type, string schema) {
            _ = type ?? throw new ArgumentNullException(nameof(type));

            var retval = new Dictionary<PropertyInfo, LdapAttributeAttribute>();
            var props = from p in type.GetProperties()
                        let a = p.GetCustomAttributes<LdapAttributeAttribute>()
                            .Where(a => a.Schema == schema)
                            .FirstOrDefault()
                        where (a != null) && (p.PropertyType == typeof(string))
                        select new {
                            Property = p,
                            Attribute = a
                        };

            // Fix for issue #1: If the user is derived from LdapUserBase and
            // the property is overridden, it does not have an accessible setter
            // any more, so we have to use the base class.
            var patchSetter = typeof(LdapUserBase).IsAssignableFrom(type);

            foreach (var p in props) {
                if (patchSetter && (p.Property?.SetMethod?.IsPublic != true)) {
                    var pp = typeof(LdapUserBase).GetProperty(p.Property.Name);
                    retval[pp ?? p.Property] = p.Attribute;
                } else {
                    retval[p.Property] = p.Attribute;
                }
            }

            return retval;
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
        public static Dictionary<PropertyInfo, LdapAttributeAttribute>
        GetLdapProperties<TType>(string schema) {
            return GetLdapProperties(typeof(TType), schema);
        }
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
        /// Getr or sets the type of an optional converter transforming the
        /// attribute into a usable form.
        /// </summary>
        public Type Converter { get; set; }

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
        /// <returns>A <see cref="ILdapAttributeConverter"/> or <c>null</c> if
        /// no converter was annotated.</returns>
        public ILdapAttributeConverter GetConverter() {
            if (this.Converter != null) {
                return Activator.CreateInstance(this.Converter)
                    as ILdapAttributeConverter;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Gets the value described by the attribute from the given entry.
        /// </summary>
        /// <param name="entry">The entry to get the attribute from.</param>
        /// <param name="parameter">An optional converter parameter.</param>
        /// <returns>The value of the attribute as <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="entry"/>
        /// is <c>null</c>.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        /// If the designated attribute does not exist for the entry or if it
        /// has not been loaded.</exception>
        public string GetValue(LdapEntry entry, object parameter = null) {
            _ = entry ?? throw new ArgumentNullException(nameof(entry));

            var attribute = entry.GetAttribute(this.Name);
            var retval = attribute.ToString(this);

            return retval;
        }
        #endregion
    }
}
