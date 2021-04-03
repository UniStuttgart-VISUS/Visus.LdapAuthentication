﻿// <copyright file="LdapOptionsTest.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2021 Visualisierungsinstitut der Universität Stuttgart. Alle Rechte vorbehalten.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Visus.LdapAuthentication.Tests {


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
            Assert.AreEqual("objectSid", mapping.GroupIdentityAttribute);
            Assert.AreEqual(typeof(SidConverter).FullName, mapping.GroupIdentityConverter);
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
            Assert.AreEqual("gidNumber", mapping.GroupIdentityAttribute);
            Assert.IsNull(mapping.GroupIdentityConverter);
            Assert.AreEqual("memberOf", mapping.GroupsAttribute);
            Assert.AreEqual("gidNumber", mapping.PrimaryGroupAttribute);
            Assert.AreEqual("(|(sAMAccountName={0})(userPrincipalName={0}))", mapping.UserFilter);
            Assert.AreEqual("(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))", mapping.UsersFilter);
        }
    }
}
