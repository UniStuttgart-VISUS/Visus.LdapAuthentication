// <copyright file="LdapUserBaseTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// Tests behaviour of <see cref="LdapUserBase"/>-derived custom user.
    /// </summary>
    [TestClass]
    public sealed class LdapUserBaseTest {

        #region Nested class CustomUser1
        /// <summary>
        /// Custom user class overriding nothing.
        /// </summary>
        private sealed class CustomUser1 : LdapUserBase { }
        #endregion

        #region Nested class CustomUser2
        /// <summary>
        /// Custom user overriding one fo the AD attributes.
        /// </summary>
        private sealed class CustomUser2 : LdapUserBase {
            [LdapAttribute(Schema.ActiveDirectory, "userPrincipalName")]
            public override string AccountName => base.AccountName;
        }
        #endregion

        #region Nested class CustomUser3
        /// <summary>
        /// Custom user overriding adding an attribute
        /// </summary>
        private sealed class CustomUser3 : LdapUserBase {
            [LdapAttribute(Schema.ActiveDirectory, "userPrincipalName")]
            public string UserPrincipalName { get; set; }
        }
        #endregion

        /// <summary>
        /// <see cref="CustomUser1"/> should behave as the built-in <see cref="LdapUser"/>.
        /// </summary>
        [TestMethod]
        public void TestCustomUser1() {
            var type = typeof(CustomUser1);
            var props = from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        where p.CanRead && p.CanWrite
                        select p;

            foreach (var p in props) {
                Assert.IsTrue(Attribute.IsDefined(p, typeof(LdapAttributeAttribute)), $"{p.Name} as LdapAttribute");
                Assert.IsNotNull(LdapAttributeAttribute.GetLdapAttribute(p, Schema.ActiveDirectory), $"{p.Name} has AD attribute");
            }

            var adProps = LdapAttributeAttribute.GetLdapProperties(type, Schema.ActiveDirectory);

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.AccountName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("sAMAccountName", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.ChristianName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("givenName", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.DisplayName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("displayName", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.EmailAddress)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("mail", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.Identity)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("objectSid", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.Surname)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("sn", prop.Value.Name);
            }
        }

        /// <summary>
        /// <see cref="CustomUser2"/> should have a different <see cref="CustomUser2.AccountName"/>.
        /// </summary>
        [TestMethod]
        public void TestCustomUser2() {
            var type = typeof(CustomUser2);
            var props = from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        where p.CanRead && p.CanWrite
                        select p;

            foreach (var p in props) {
                Assert.IsTrue(Attribute.IsDefined(p, typeof(LdapAttributeAttribute)), $"{p.Name} as LdapAttribute");
                Assert.IsNotNull(LdapAttributeAttribute.GetLdapAttribute(p, Schema.ActiveDirectory), $"{p.Name} has AD attribute");
            }

            var adProps = LdapAttributeAttribute.GetLdapProperties(type, Schema.ActiveDirectory);

            {
                var prop = type.GetProperty(nameof(LdapUser.AccountName));
                Assert.AreEqual(1, prop.GetCustomAttributes<LdapAttributeAttribute>().Count(), "Attributes are not inherited");
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.AccountName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("userPrincipalName", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.ChristianName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("givenName", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.DisplayName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("displayName", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.EmailAddress)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("mail", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.Identity)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("objectSid", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.Surname)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("sn", prop.Value.Name);
            }
        }

        /// <summary>
        /// <see cref="CustomUser3"/> should have everything like <see cref="LdapUser"/>
        /// and an additional UPN attribute.
        /// </summary>
        [TestMethod]
        public void TestCustomUser3() {
            var type = typeof(CustomUser3);
            var props = from p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        where p.CanRead && p.CanWrite
                        select p;

            foreach (var p in props) {
                Assert.IsTrue(Attribute.IsDefined(p, typeof(LdapAttributeAttribute)), $"{p.Name} as LdapAttribute");
                Assert.IsNotNull(LdapAttributeAttribute.GetLdapAttribute(p, Schema.ActiveDirectory), $"{p.Name} has AD attribute");
            }

            var adProps = LdapAttributeAttribute.GetLdapProperties(type, Schema.ActiveDirectory);

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.AccountName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("sAMAccountName", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.ChristianName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("givenName", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.DisplayName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("displayName", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.EmailAddress)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("mail", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.Identity)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("objectSid", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(LdapUser.Surname)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("sn", prop.Value.Name);
            }

            {
                var prop = adProps.Where(p => p.Key.Name == nameof(CustomUser3.UserPrincipalName)).SingleOrDefault();
                Assert.IsNotNull(prop);
                Assert.IsNotNull(prop.Key);
                Assert.IsNotNull(prop.Value);
                Assert.AreEqual("userPrincipalName", prop.Value.Name);
            }
        }
    }
}
