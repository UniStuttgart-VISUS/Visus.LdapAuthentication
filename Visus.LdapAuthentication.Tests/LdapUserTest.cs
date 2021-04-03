// <copyright file="SidConverterTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;


namespace Visus.LdapAuthentication.Tests {

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
    }
}
