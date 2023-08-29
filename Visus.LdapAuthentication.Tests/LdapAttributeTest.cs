// <copyright file="LdapAttributeTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// Tests <see cref="LdapAttributeAttribute"/>.
    /// </summary>
    [TestClass]
    public sealed class LdapAttributeTest {

        #region Nested class TestClass1
        private sealed class TestClass1 {

            [LdapAttribute(Schema.ActiveDirectory, "attribute1")]
            public string Property1 { get; set; }

            [LdapAttribute(Schema.ActiveDirectory, "attribute2a")]
            [LdapAttribute(Schema.Rfc2307, "attribute2b")]
            public string Property2 { get; set; }

            public string Property3 { get; set; }
        }
        #endregion

        [TestMethod]
        public void TestRetrieveFromMemberInfo() {
            var type = typeof(TestClass1);

            {
                var pi = type.GetProperty(nameof(TestClass1.Property1));
                Assert.IsNotNull(pi);
                var att = LdapAttributeAttribute.GetLdapAttribute(pi, Schema.ActiveDirectory);
                Assert.IsNotNull(att);
                Assert.AreEqual(att.Name, "attribute1");
                Assert.AreEqual(att.Schema, Schema.ActiveDirectory);
            }

            {
                var pi = type.GetProperty(nameof(TestClass1.Property2));
                Assert.IsNotNull(pi);
                var att = LdapAttributeAttribute.GetLdapAttribute(pi, Schema.ActiveDirectory);
                Assert.IsNotNull(att);
                Assert.AreEqual(att.Name, "attribute2a");
                Assert.AreEqual(att.Schema, Schema.ActiveDirectory);
            }

            {
                var pi = type.GetProperty(nameof(TestClass1.Property2));
                Assert.IsNotNull(pi);
                var att = LdapAttributeAttribute.GetLdapAttribute(pi, Schema.Rfc2307);
                Assert.IsNotNull(att);
                Assert.AreEqual(att.Name, "attribute2b");
                Assert.AreEqual(att.Schema, Schema.Rfc2307);
            }

            {
                var pi = type.GetProperty(nameof(TestClass1.Property3));
                Assert.IsNotNull(pi);
                var att = LdapAttributeAttribute.GetLdapAttribute(pi, Schema.IdentityManagementForUnix);
                Assert.IsNull(att);
            }

            {
                var pi = type.GetProperty(nameof(TestClass1.Property3));
                Assert.IsNotNull(pi);
                var att = LdapAttributeAttribute.GetLdapAttribute(pi, Schema.ActiveDirectory);
                Assert.IsNull(att);
            }
        }

        [TestMethod]
        public void TestRetrieveFromName() {
            var type = typeof(TestClass1);

            {
                var att = LdapAttributeAttribute.GetLdapAttribute(type, nameof(TestClass1.Property1), Schema.ActiveDirectory);
                Assert.IsNotNull(att);
                Assert.AreEqual(att.Name, "attribute1");
                Assert.AreEqual(att.Schema, Schema.ActiveDirectory);
            }

            {
                var att = LdapAttributeAttribute.GetLdapAttribute<TestClass1>(nameof(TestClass1.Property1), Schema.ActiveDirectory);
                Assert.IsNotNull(att);
                Assert.AreEqual(att.Name, "attribute1");
                Assert.AreEqual(att.Schema, Schema.ActiveDirectory);
            }

            {
                var att = LdapAttributeAttribute.GetLdapAttribute(type, nameof(TestClass1.Property2), Schema.ActiveDirectory);
                Assert.IsNotNull(att);
                Assert.AreEqual(att.Name, "attribute2a");
                Assert.AreEqual(att.Schema, Schema.ActiveDirectory);
            }

            {
                var att = LdapAttributeAttribute.GetLdapAttribute<TestClass1>(nameof(TestClass1.Property2), Schema.ActiveDirectory);
                Assert.IsNotNull(att);
                Assert.AreEqual(att.Name, "attribute2a");
                Assert.AreEqual(att.Schema, Schema.ActiveDirectory);
            }

            {
                var att = LdapAttributeAttribute.GetLdapAttribute(type, nameof(TestClass1.Property2), Schema.Rfc2307);
                Assert.IsNotNull(att);
                Assert.AreEqual(att.Name, "attribute2b");
                Assert.AreEqual(att.Schema, Schema.Rfc2307);
            }

            {
                var att = LdapAttributeAttribute.GetLdapAttribute<TestClass1>(nameof(TestClass1.Property2), Schema.Rfc2307);
                Assert.IsNotNull(att);
                Assert.AreEqual(att.Name, "attribute2b");
                Assert.AreEqual(att.Schema, Schema.Rfc2307);
            }

            {
                var att = LdapAttributeAttribute.GetLdapAttribute(type, nameof(TestClass1.Property2), Schema.IdentityManagementForUnix);
                Assert.IsNull(att);
            }

            {
                var att = LdapAttributeAttribute.GetLdapAttribute<TestClass1>(nameof(TestClass1.Property2), Schema.IdentityManagementForUnix);
                Assert.IsNull(att);
            }

            {
                var att = LdapAttributeAttribute.GetLdapAttribute(type, nameof(TestClass1.Property3), Schema.ActiveDirectory);
                Assert.IsNull(att);
            }

            {
                var att = LdapAttributeAttribute.GetLdapAttribute<TestClass1>(nameof(TestClass1.Property3), Schema.ActiveDirectory);
                Assert.IsNull(att);
            }
        }

        [TestMethod]
        public void TestRetrieveAll() {
            var type = typeof(TestClass1);

            {
                var props = LdapAttributeAttribute.GetLdapProperties(type, Schema.ActiveDirectory);
                Assert.IsTrue(props.Any(p => p.Key.Name == nameof(TestClass1.Property1)));
                Assert.IsTrue(props.Any(p => p.Key.Name == nameof(TestClass1.Property2)));
                Assert.IsTrue(!props.Any(p => p.Key.Name == nameof(TestClass1.Property3)));
                Assert.IsTrue(props.All(p => p.Value.Schema == Schema.ActiveDirectory));
            }

            {
                var props = LdapAttributeAttribute.GetLdapProperties<TestClass1>(Schema.ActiveDirectory);
                Assert.IsTrue(props.Any(p => p.Key.Name == nameof(TestClass1.Property1)));
                Assert.IsTrue(props.Any(p => p.Key.Name == nameof(TestClass1.Property2)));
                Assert.IsTrue(!props.Any(p => p.Key.Name == nameof(TestClass1.Property3)));
                Assert.IsTrue(props.All(p => p.Value.Schema == Schema.ActiveDirectory));
            }

            {
                var props = LdapAttributeAttribute.GetLdapProperties(type, Schema.Rfc2307);
                Assert.IsTrue(!props.Any(p => p.Key.Name == nameof(TestClass1.Property1)));
                Assert.IsTrue(props.Any(p => p.Key.Name == nameof(TestClass1.Property2)));
                Assert.IsTrue(!props.Any(p => p.Key.Name == nameof(TestClass1.Property3)));
                Assert.IsTrue(props.All(p => p.Value.Schema == Schema.Rfc2307));
            }

            {
                var props = LdapAttributeAttribute.GetLdapProperties<TestClass1>(Schema.Rfc2307);
                Assert.IsTrue(!props.Any(p => p.Key.Name == nameof(TestClass1.Property1)));
                Assert.IsTrue(props.Any(p => p.Key.Name == nameof(TestClass1.Property2)));
                Assert.IsTrue(!props.Any(p => p.Key.Name == nameof(TestClass1.Property3)));
                Assert.IsTrue(props.All(p => p.Value.Schema == Schema.Rfc2307));
            }

            {
                var props = LdapAttributeAttribute.GetLdapProperties(type, Schema.IdentityManagementForUnix);
                Assert.IsTrue(!props.Any());
            }

            {
                var props = LdapAttributeAttribute.GetLdapProperties<TestClass1>(Schema.IdentityManagementForUnix);
                Assert.IsTrue(!props.Any());
            }
        }
    }
}
