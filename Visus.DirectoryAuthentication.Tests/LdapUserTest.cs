﻿// <copyright file="LdapUserTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
                        where p.Name != nameof(LdapUser.Claims)
                        where p.Name != nameof(LdapUser.Groups)
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

        [TestMethod]
        public void TestClaims() {
            var type = typeof(LdapUser);
            var prop = type.GetProperty(nameof(LdapUser.Claims));
            Assert.IsNotNull(prop);
            Assert.IsTrue(ClaimsAttribute.IsClaims(prop));
            Assert.AreEqual(prop, ClaimsAttribute.GetClaims<LdapUser>());
        }

        [TestMethod]
        public void TestGroups() {
            var type = typeof(LdapUser);
            var prop = type.GetProperty(nameof(LdapUser.Groups));
            Assert.IsNotNull(prop);
            Assert.IsTrue(LdapGroupsAttribute.IsLdapGroups(prop));
            Assert.AreEqual(prop, LdapGroupsAttribute.GetLdapGroups<LdapUser>());
        }
    }
}
