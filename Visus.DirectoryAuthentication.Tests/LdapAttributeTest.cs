// <copyright file="LdapAttributeTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Tests {

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
                var att = pi.GetCustomAttributes<LdapAttributeAttribute>(true)
                    .Where(a => a.Schema == Schema.ActiveDirectory)
                    .SingleOrDefault();
                Assert.IsNotNull(att);
                Assert.AreEqual(att.Name, "attribute1");
                Assert.AreEqual(att.Schema, Schema.ActiveDirectory);
            }

            {
                var pi = type.GetProperty(nameof(TestClass1.Property2));
                Assert.IsNotNull(pi);
                var att = pi.GetCustomAttributes<LdapAttributeAttribute>(true)
                    .Where(a => a.Schema == Schema.ActiveDirectory)
                    .SingleOrDefault();
                Assert.IsNotNull(att);
                Assert.AreEqual(att.Name, "attribute2a");
                Assert.AreEqual(att.Schema, Schema.ActiveDirectory);
            }

            {
                var pi = type.GetProperty(nameof(TestClass1.Property2));
                Assert.IsNotNull(pi);
                var att = pi.GetCustomAttributes<LdapAttributeAttribute>(true)
                    .Where(a => a.Schema == Schema.Rfc2307)
                    .SingleOrDefault();
                Assert.IsNotNull(att);
                Assert.AreEqual(att.Name, "attribute2b");
                Assert.AreEqual(att.Schema, Schema.Rfc2307);
            }

            {
                var pi = type.GetProperty(nameof(TestClass1.Property3));
                Assert.IsNotNull(pi);
                var att = pi.GetCustomAttributes<LdapAttributeAttribute>(true)
                    .Where(a => a.Schema == Schema.IdentityManagementForUnix)
                    .SingleOrDefault();
                Assert.IsNull(att);
            }

            {
                var pi = type.GetProperty(nameof(TestClass1.Property3));
                Assert.IsNotNull(pi);
                var att = pi.GetCustomAttributes<LdapAttributeAttribute>(true)
                    .Where(a => a.Schema == Schema.ActiveDirectory)
                    .SingleOrDefault();
                Assert.IsNull(att);
            }
        }
    }
}
