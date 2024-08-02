// <copyright file="LdapAttributeMapBuilderTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Visus.Ldap;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Tests {

    [TestClass]
    public sealed class LdapAttributeMapBuilderTest {

        [TestMethod]
        public void TestGroupMap() {
            var builder = new LdapAttributeMapBuilder<LdapGroup>(Schema.ActiveDirectory);

            builder.MapProperty(nameof(LdapGroup.AccountName))
                .StoringAccountName()
                .ToAttribute("sAMAccountName");

            builder.MapProperty(nameof(LdapGroup.DisplayName))
                .ToAttribute("displayName");

            builder.MapProperty(nameof(LdapGroup.DistinguishedName))
                .StoringDistinguishedName()
                .ToAttribute("distinguishedName");

            builder.MapProperty(nameof(LdapGroup.Identity))
                .StoringIdentity()
                .ToAttribute("objectSid")
                .WithConverter<SidConverter>();

            builder.MapProperty(nameof(LdapGroup.IsPrimary))
                .StoringPrimaryGroupFlag();

            var map = builder.Build();
            Assert.IsNotNull(map);
            Assert.IsNotNull(map.AccountNameProperty);
            Assert.IsNotNull(map.DistinguishedNameAttribute);
            Assert.IsNotNull(map.IdentityProperty);
            Assert.IsNull(map.GroupMembershipsProperty);
            Assert.IsNotNull(map.IsPrimaryGroupProperty);

            Assert.IsNotNull(map.AccountNameAttribute);
            Assert.AreEqual("sAMAccountName", map.AccountNameAttribute.Name);
            Assert.IsNotNull(map.DistinguishedNameAttribute);
            Assert.AreEqual("distinguishedName", map.DistinguishedNameAttribute.Name);
            Assert.IsNotNull(map.IdentityAttribute);
            Assert.AreEqual("objectSid", map.IdentityAttribute.Name);
            Assert.IsNotNull(map.AccountNameAttribute);

            var group = new LdapGroup() {
                AccountName = "group",
                DisplayName = "display",
                DistinguishedName = "CN=group",
                Identity = "123",
                IsPrimary = true
            };
        }

        [TestMethod]
        public void TestUserMap() {
            var builder = new LdapAttributeMapBuilder<LdapUser>(Schema.ActiveDirectory);

            builder.MapProperty(nameof(LdapUser.AccountName))
                .StoringAccountName()
                .ToAttribute("sAMAccountName");

            builder.MapProperty(nameof(LdapUser.ChristianName))
                .ToAttribute("givenName");

            builder.MapProperty(nameof(LdapUser.DisplayName))
                .ToAttribute("displayName");

            builder.MapProperty(nameof(LdapUser.DistinguishedName))
                .StoringDistinguishedName()
                .ToAttribute(new LdapAttributeAttribute(Schema.ActiveDirectory, "distinguishedName"));

            builder.MapProperty(nameof(LdapUser.EmailAddress))
                .ToAttribute("mail");

            builder.MapProperty(nameof(LdapUser.Groups))
                .StoringGroupMemberships();

            builder.MapProperty(nameof(LdapGroup.Identity))
                .StoringIdentity()
                .ToAttribute("objectSid")
                .WithConverter(typeof(SidConverter));

            builder.MapProperty(nameof(LdapUser.Surname))
                .ToAttribute("sn");

            var map = builder.Build();
            Assert.IsNotNull(map);
            Assert.IsNotNull(map.AccountNameProperty);
            Assert.IsNotNull(map.DistinguishedNameAttribute);
            Assert.IsNotNull(map.IdentityProperty);
            Assert.IsNotNull(map.GroupMembershipsProperty);
            Assert.IsNull(map.IsPrimaryGroupProperty);

            Assert.IsNotNull(map.AccountNameAttribute);
            Assert.AreEqual("sAMAccountName", map.AccountNameAttribute.Name);
            Assert.IsNotNull(map.DistinguishedNameAttribute);
            Assert.AreEqual("distinguishedName", map.DistinguishedNameAttribute.Name);
            Assert.IsNotNull(map.IdentityAttribute);
            Assert.AreEqual("objectSid", map.IdentityAttribute.Name);
            Assert.IsNotNull(map.AccountNameAttribute);

            var user = new LdapUser() {
                AccountName = "user",
                ChristianName = "Walter",
                DisplayName = "Walter Ulbricht",
                DistinguishedName = "CN=ulbricht",
                EmailAddress = "ulbricht@sed.de",
                Identity = "0815",
                Surname = "Ulbricht"
            };
        }

        [TestMethod]
        public void TestRequireSchema() {
            Assert.ThrowsException<ArgumentNullException>(() => new LdapAttributeMapBuilder<LdapGroup>(null!));
            Assert.ThrowsException<ArgumentNullException>(() => new LdapAttributeMapBuilder<LdapUser>(null!));
        }

        [TestMethod]
        public void TestPreventMappingChange() {
            var builder = new LdapAttributeMapBuilder<LdapGroup>(Schema.ActiveDirectory);

            {
                var prop = builder.MapProperty(nameof(LdapGroup.AccountName));
                Assert.IsNotNull(prop.ToAttribute("sAMAccountName"));
                Assert.ThrowsException<InvalidOperationException>(() => prop.ToAttribute("sAMAccountName"));
            }

            {
                var prop = builder.MapProperty(nameof(LdapUser.AccountName));
                Assert.IsNotNull(prop.ToAttribute("sAMAccountName"));
                Assert.ThrowsException<InvalidOperationException>(() => prop.ToAttribute("sAMAccountName"));
            }
        }

        [TestMethod]
        public void TestPreventInvalidProperty() {
            var builder = new LdapAttributeMapBuilder<LdapUser>(Schema.ActiveDirectory);
            Assert.ThrowsException<ArgumentNullException>(() => builder.MapProperty(null!));
            Assert.ThrowsException<ArgumentNullException>(() => builder.MapProperty(null!));
            Assert.ThrowsException<ArgumentException>(() => builder.MapProperty("hurz"));
            Assert.ThrowsException<ArgumentException>(() => builder.MapProperty("hurz"));
        }

        [TestMethod]
        public void TestPreventInvalidAttribute() {
            var builder = new LdapAttributeMapBuilder<LdapUser>(Schema.ActiveDirectory);

            {
                var prop = builder.MapProperty(nameof(LdapGroup.AccountName));
                Assert.ThrowsException<ArgumentNullException>(() => prop.ToAttribute((string) null!));
                Assert.ThrowsException<ArgumentNullException>(() => prop.ToAttribute((LdapAttributeAttribute) null!));
                Assert.ThrowsException<ArgumentException>(() => prop.ToAttribute(string.Empty));
            }

            {
                var prop = builder.MapProperty(nameof(LdapUser.AccountName));
                Assert.ThrowsException<ArgumentNullException>(() => prop.ToAttribute((string) null!));
                Assert.ThrowsException<ArgumentNullException>(() => prop.ToAttribute((LdapAttributeAttribute) null!));
                Assert.ThrowsException<ArgumentException>(() => prop.ToAttribute(string.Empty));
            }
        }

        [TestMethod]
        public void TestPreventSchemaMismatch() {
            var builder = new LdapAttributeMapBuilder<LdapUser>(Schema.ActiveDirectory);

            {
                var prop = builder.MapProperty(nameof(LdapGroup.AccountName));
                Assert.ThrowsException<ArgumentException>(() => prop.ToAttribute(new LdapAttributeAttribute("hurz", "sAMAccountName")));
            }

            {
                var prop = builder.MapProperty(nameof(LdapUser.AccountName));
                Assert.ThrowsException<ArgumentException>(() => prop.ToAttribute(new LdapAttributeAttribute("hurz", "sAMAccountName")));
            }
        }
    }
}
