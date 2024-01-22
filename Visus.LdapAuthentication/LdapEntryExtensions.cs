// <copyright file="LdapEntryExtensions.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Visus.LdapAuthentication {

    /// <summary>
    /// Extension methods for <see cref="LdapEntry"/>.
    /// </summary>
    public static class LdapEntryExtensions {

        /// <summary>
        /// Assigns LDAP attributes to the given target object.
        /// </summary>
        /// <param name="that">The entry holding the properties to assign.
        /// </param>
        /// <param name="target">The target object to assign the attributes to.
        /// </param>
        /// <param name="schema">The LDAP schema determining the names of the
        /// attributes we search..</param>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or <paramref name="target"/> is <c>null</c>.
        /// </exception>
        public static void AssignTo(this LdapEntry that, object target,
                string schema) {
            _ = that ?? throw new ArgumentNullException(nameof(that));
            _ = target ?? throw new ArgumentNullException(nameof(target));
            var props = LdapAttributeAttribute.GetLdapProperties(
                target.GetType(), schema);

            foreach (var p in props.Keys) {
                try {
                    var a = props[p];
                    var v = a.GetValue(that);
                    p.SetValue(target, v);
                } catch (KeyNotFoundException) {
                    Debug.WriteLine($"LDAP attribute \"{p.Name}\" not found, "
                        + "ignoring it while assigning properties.");
                    continue;
                }
            }
        }

        /// <summary>
        /// Assigns LDAP attributes to the given target object.
        /// </summary>
        /// <param name="that">The entry holding the properties to assign.
        /// </param>
        /// <param name="target">The target object to assign the attributes to.
        /// </param>
        /// <param name="options">The LDAP options determining the schema that
        /// is used while searching for the LDAP attributes.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="that"/>
        /// is <c>null</c>, or <paramref name="target"/> is <c>null</c>.
        /// </exception>
        public static void AssignTo(this LdapEntry that, object target,
                ILdapOptions options)
            => that.AssignTo(target, options?.Schema);
    }
}
