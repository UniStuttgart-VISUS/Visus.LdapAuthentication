// <copyright file="LdapOptionsTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 - 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Visus.DirectoryAuthentication.Configuration;


namespace Visus.DirectoryAuthentication.Tests {

    /// <summary>
    /// Tests for <see cref="LdapOptions"/>.
    /// </summary>
    [TestClass]
    public sealed class LdapOptionsTest {

        [TestMethod]
        public void TestActiveDirectoryMapping() {
            var options = new LdapOptions();
            var mapping = new LdapMapping();

            Assert.IsTrue(options.Mappings.TryGetValue(Schema.ActiveDirectory, out mapping));
            Assert.AreEqual("distinguishedName", mapping.DistinguishedNameAttribute);
            Assert.AreEqual("memberOf", mapping.GroupsAttribute);
            Assert.AreEqual("primaryGroupID", mapping.PrimaryGroupAttribute);
            Assert.AreEqual("(|(sAMAccountName={0})(userPrincipalName={0}))", mapping.UserFilter);
            Assert.AreEqual("(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))", mapping.UsersFilter);
        }

        [TestMethod]
        public void TestIdmuMapping() {
            var options = new LdapOptions();
            var mapping = new LdapMapping();

            Assert.IsTrue(options.Mappings.TryGetValue(Schema.IdentityManagementForUnix, out mapping));
            Assert.AreEqual("distinguishedName", mapping.DistinguishedNameAttribute);
            Assert.AreEqual("memberOf", mapping.GroupsAttribute);
            Assert.AreEqual("gidNumber", mapping.PrimaryGroupAttribute);
            Assert.AreEqual("(|(sAMAccountName={0})(userPrincipalName={0}))", mapping.UserFilter);
            Assert.AreEqual("(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))", mapping.UsersFilter);
        }
    }
}
