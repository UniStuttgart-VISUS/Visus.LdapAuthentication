// <copyright file="SidConverterTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Reflection;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests <see cref="LdapUser"/>.
    /// </summary>
    [TestClass]
    public sealed class LdapUserTest {

        [TestMethod]
        public void TestLdapAttributes() {
            var type = typeof(LdapUser);
            var props = from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        where p.CanRead && p.CanWrite
                        select p;

            foreach (var p in props) {
                Assert.IsTrue(Attribute.IsDefined(p, typeof(LdapAttributeAttribute)), $"{p.Name} as LdapAttribute");
                Assert.IsNotNull(LdapAttributeAttribute.GetLdapAttribute(p, Schema.ActiveDirectory), $"{p.Name} has AD attribute");
            }
        }

        [TestMethod]
        public void TestActiveDirectoryAttributes() {
            var props = LdapAttributeAttribute.GetLdapProperties<LdapUser>(Schema.ActiveDirectory);

            {
                var prop = props.Where(p => p.Key.Name == nameof(LdapUser.AccountName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("sAMAccountName", prop.Value.Name);
            }

            {
                var prop = props.Where(p => p.Key.Name == nameof(LdapUser.ChristianName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("givenName", prop.Value.Name);
            }

            {
                var prop = props.Where(p => p.Key.Name == nameof(LdapUser.DisplayName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("displayName", prop.Value.Name);
            }

            {
                var prop = props.Where(p => p.Key.Name == nameof(LdapUser.EmailAddress)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("mail", prop.Value.Name);
            }

            {
                var prop = props.Where(p => p.Key.Name == nameof(LdapUser.Identity)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("objectSid", prop.Value.Name);
            }

            {
                var prop = props.Where(p => p.Key.Name == nameof(LdapUser.Surname)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("sn", prop.Value.Name);
            }
        }


        [TestMethod]
        public void TestIdmuAttributes() {
            var props = LdapAttributeAttribute.GetLdapProperties<LdapUser>(Schema.IdentityManagementForUnix);

            {
                var prop = props.Where(p => p.Key.Name == nameof(LdapUser.AccountName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("sAMAccountName", prop.Value.Name);
            }

            {
                var prop = props.Where(p => p.Key.Name == nameof(LdapUser.ChristianName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("givenName", prop.Value.Name);
            }

            {
                var prop = props.Where(p => p.Key.Name == nameof(LdapUser.DisplayName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("displayName", prop.Value.Name);
            }

            {
                var prop = props.Where(p => p.Key.Name == nameof(LdapUser.EmailAddress)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("mail", prop.Value.Name);
            }

            {
                var prop = props.Where(p => p.Key.Name == nameof(LdapUser.Identity)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("uidNumber", prop.Value.Name);
            }

            {
                var prop = props.Where(p => p.Key.Name == nameof(LdapUser.Surname)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("sn", prop.Value.Name);
            }
        }
    }
}
