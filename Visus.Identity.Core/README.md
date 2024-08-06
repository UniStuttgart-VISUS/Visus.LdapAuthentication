# Visus.Identity.Core
This library implements shared functionality for [Visus.DirectoryIdentity](https://github.com/UniStuttgart-VISUS/Visus.LdapAuthentication/tree/main/Visus.DirectoryIdentity) and [Visus.LdapIdentity](https://github.com/UniStuttgart-VISUS/Visus.LdapAuthentication/tree/main/Visus.LdapIdentity). There is limited use in adding this library independently.

## Usage
You should typically install this library as a dependency of [Visus.DirectoryIdentity](https://github.com/UniStuttgart-VISUS/Visus.LdapAuthentication/tree/main/Visus.DirectoryIdentity) or [Visus.LdapIdentity](https://github.com/UniStuttgart-VISUS/Visus.LdapAuthentication/tree/main/Visus.LdapIdentity).

There might be a use case for using it directly if you only need the authentication using an LDAP bind, but not the LDAP-based user stores. In such an application case where the users and roles are held in a database and registration only means copying initial data from LDAP to the database, you might want to solely use the `LdapUserManager` from this library in conjunction with EF-based stores. However, this would require manually setting up all LDAP services rather than adding LDAP identity via more convenient extension methods.
