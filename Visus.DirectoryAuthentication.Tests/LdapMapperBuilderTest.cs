// <copyright file="LdapMapperBuilderTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Visus.DirectoryAuthentication.Mapping;
using Visus.Ldap;
using Visus.Ldap.Mapping;


namespace Visus.DirectoryAuthentication.Tests {

    [TestClass]
    public sealed class LdapMapperBuilderTest {

        [TestMethod]
        public void TestActiveDirectoryMapping() {
            var builder = new LdapMapperBuilder<LdapUser, LdapGroup>()
                .ForSchema(Schema.ActiveDirectory);

            builder.MapGroupProperty(nameof(LdapGroup.AccountName))
                .StoringAccountName()
                .ToAttribute("sAMAccountName");

            builder.MapGroupProperty(nameof(LdapGroup.DisplayName))
                .ToAttribute("displayName");

            builder.MapGroupProperty(nameof(LdapGroup.DistinguishedName))
                .StoringDistinguishedName()
                .ToAttribute("distinguishedName");

            builder.MapGroupProperty(nameof(LdapGroup.Identity))
                .StoringIdentity()
                .ToAttribute("objectSid")
                .WithConverter<SidConverter>();

            builder.MapGroupProperty(nameof(LdapGroup.IsPrimary))
                .StoringPrimaryGroupFlag();

            builder.MapUserProperty(nameof(LdapUser.AccountName))
                .StoringAccountName()
                .ToAttribute("sAMAccountName");

            builder.MapUserProperty(nameof(LdapUser.ChristianName))
                .ToAttribute("givenName");

            builder.MapUserProperty(nameof(LdapUser.DisplayName))
                .ToAttribute("displayName");

            builder.MapUserProperty(nameof(LdapUser.DistinguishedName))
                .StoringDistinguishedName()
                .ToAttribute(new LdapAttributeAttribute(Schema.ActiveDirectory, "distinguishedName"));

            builder.MapUserProperty(nameof(LdapUser.EmailAddress))
                .ToAttribute("mail");

            builder.MapUserProperty(nameof(LdapUser.Groups))
                .StoringGroupMemberships();

            builder.MapUserProperty(nameof(LdapGroup.Identity))
                .StoringIdentity()
                .ToAttribute("objectSid")
                .WithConverter(typeof(SidConverter));

            builder.MapUserProperty(nameof(LdapUser.Surname))
                .ToAttribute("sn");

            var mapper = builder.Build();
            Assert.IsNotNull(mapper);
            Assert.IsFalse(mapper.GroupIsGroupMember);
            Assert.IsTrue(mapper.UserIsGroupMember);

            var group = new LdapGroup() {
                AccountName = "group",
                DisplayName = "display",
                DistinguishedName = "CN=group",
                Identity = "123",
                IsPrimary = true
            };

            Assert.AreEqual(group.AccountName, mapper.GetAccountName(group));
            Assert.AreEqual(group.DistinguishedName, mapper.GetDistinguishedName(group));
            Assert.AreEqual(group.Identity, mapper.GetIdentity(group));

            var user = new LdapUser() {
                AccountName = "user",
                ChristianName = "Walter",
                DisplayName = "Walter Ulbricht",
                DistinguishedName = "CN=ulbricht",
                EmailAddress = "ulbricht@sed.de",
                Identity = "0815",
                Surname = "Ulbricht"
            };

            Assert.AreEqual(user.AccountName, mapper.GetAccountName(user));
            Assert.AreEqual(user.DistinguishedName, mapper.GetDistinguishedName(user));
            Assert.AreEqual(user.Identity, mapper.GetIdentity(user));
        }

        [TestMethod]
        public void TestRequireSchema() {
            var builder = new LdapMapperBuilder<LdapUser, LdapGroup>();
            Assert.ThrowsException<InvalidOperationException>(() => builder.MapGroupProperty(nameof(LdapGroup.AccountName)));
            Assert.ThrowsException<InvalidOperationException>(() => builder.MapUserProperty(nameof(LdapUser.AccountName)));
            Assert.ThrowsException<ArgumentNullException>(() => builder.ForSchema(null!));
        }

        [TestMethod]
        public void TestPreventSchemaChange() {
            var builder = new LdapMapperBuilder<LdapUser, LdapGroup>()
                .ForSchema(Schema.ActiveDirectory);
            Assert.ThrowsException<InvalidOperationException>(() => builder.ForSchema(Schema.IdentityManagementForUnix));
            Assert.IsNotNull(builder.ForSchema(Schema.ActiveDirectory));
        }

        [TestMethod]
        public void TestPreventMappingChange() {
            var builder = new LdapMapperBuilder<LdapUser, LdapGroup>()
                .ForSchema(Schema.ActiveDirectory);

            {
                var prop = builder.MapGroupProperty(nameof(LdapGroup.AccountName));
                Assert.IsNotNull(prop.ToAttribute("sAMAccountName"));
                Assert.ThrowsException<InvalidOperationException>(() => prop.ToAttribute("sAMAccountName"));
            }

            {
                var prop = builder.MapUserProperty(nameof(LdapUser.AccountName));
                Assert.IsNotNull(prop.ToAttribute("sAMAccountName"));
                Assert.ThrowsException<InvalidOperationException>(() => prop.ToAttribute("sAMAccountName"));
            }
        }

        [TestMethod]
        public void TestPreventInvalidProperty() {
            var builder = new LdapMapperBuilder<LdapUser, LdapGroup>()
                .ForSchema(Schema.ActiveDirectory);
            Assert.ThrowsException<ArgumentNullException>(() => builder.MapGroupProperty(null!));
            Assert.ThrowsException<ArgumentNullException>(() => builder.MapUserProperty(null!));
            Assert.ThrowsException<ArgumentException>(() => builder.MapGroupProperty("hurz"));
            Assert.ThrowsException<ArgumentException>(() => builder.MapUserProperty("hurz"));
        }

        [TestMethod]
        public void TestPreventInvalidAttribute() {
            var builder = new LdapMapperBuilder<LdapUser, LdapGroup>()
                .ForSchema(Schema.ActiveDirectory);

            {
                var prop = builder.MapGroupProperty(nameof(LdapGroup.AccountName));
                Assert.ThrowsException<ArgumentNullException>(() => prop.ToAttribute((string) null!));
                Assert.ThrowsException<ArgumentNullException>(() => prop.ToAttribute((LdapAttributeAttribute) null!));
                Assert.ThrowsException<ArgumentException>(() => prop.ToAttribute(string.Empty));
            }

            {
                var prop = builder.MapUserProperty(nameof(LdapUser.AccountName));
                Assert.ThrowsException<ArgumentNullException>(() => prop.ToAttribute((string) null!));
                Assert.ThrowsException<ArgumentNullException>(() => prop.ToAttribute((LdapAttributeAttribute) null!));
                Assert.ThrowsException<ArgumentException>(() => prop.ToAttribute(string.Empty));
            }
        }

        [TestMethod]
        public void TestPreventSchemaMismatch() {
            var builder = new LdapMapperBuilder<LdapUser, LdapGroup>()
                .ForSchema(Schema.ActiveDirectory);

            {
                var prop = builder.MapGroupProperty(nameof(LdapGroup.AccountName));
                Assert.ThrowsException<ArgumentException>(() => prop.ToAttribute(new LdapAttributeAttribute("hurz", "sAMAccountName")));
            }

            {
                var prop = builder.MapUserProperty(nameof(LdapUser.AccountName));
                Assert.ThrowsException<ArgumentException>(() => prop.ToAttribute(new LdapAttributeAttribute("hurz", "sAMAccountName")));
            }
        }
    }
}
