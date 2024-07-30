// <copyright file="LdapOptionsTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Visus.Ldap.Mapping;
using Visus.LdapAuthentication.Configuration;


namespace Visus.LdapAuthentication.Tests {

    /// <summary>
    /// Tests for <see cref="LdapOptions"/>.
    /// </summary>
    [TestClass]
    public sealed class LdapOptionsTest {

        [TestMethod]
        public void TestActiveDirectoryMapping() {
            var options = new LdapOptions();

            Assert.IsTrue(options.Mappings.TryGetValue(Schema.ActiveDirectory, out var mapping));
            Assert.AreEqual("distinguishedName", mapping.DistinguishedNameAttribute);
            Assert.AreEqual("memberOf", mapping.GroupsAttribute);
            Assert.AreEqual("primaryGroupID", mapping.PrimaryGroupAttribute);
            Assert.AreEqual("objectSid", mapping.PrimaryGroupIdentityAttribute);
            Assert.AreEqual("(|(sAMAccountName={0})(userPrincipalName={0}))", mapping.UserFilter);
            Assert.AreEqual("(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))", mapping.UsersFilter);
        }

        [TestMethod]
        public void TestIdmuMapping() {
            var options = new LdapOptions();

            Assert.IsTrue(options.Mappings.TryGetValue(Schema.IdentityManagementForUnix, out var mapping));
            Assert.AreEqual("distinguishedName", mapping.DistinguishedNameAttribute);
            Assert.AreEqual("memberOf", mapping.GroupsAttribute);
            Assert.AreEqual("gidNumber", mapping.PrimaryGroupAttribute);
            Assert.AreEqual("gidNumber", mapping.PrimaryGroupIdentityAttribute);
            Assert.AreEqual("(|(sAMAccountName={0})(userPrincipalName={0}))", mapping.UserFilter);
            Assert.AreEqual("(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))", mapping.UsersFilter);
        }

        [TestMethod]
        public void TestRfc2307Mapping() {
            var options = new LdapOptions();

            Assert.IsTrue(options.Mappings.TryGetValue(Schema.Rfc2307, out var mapping));
            Assert.AreEqual("distinguishedName", mapping.DistinguishedNameAttribute);
            Assert.AreEqual("memberOf", mapping.GroupsAttribute);
            Assert.AreEqual("gidNumber", mapping.PrimaryGroupAttribute);
            Assert.AreEqual("gidNumber", mapping.PrimaryGroupIdentityAttribute);
            Assert.AreEqual("(&(objectClass=posixAccount)(entryDN={0}))", mapping.UserFilter);
            Assert.AreEqual("(&(objectClass=posixAccount)(objectClass=person))", mapping.UsersFilter);
        }
    }
}
