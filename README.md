# ASP.NET Core LDAP Authentication Middleware

[![Build Status](https://visualisierungsinstitut.visualstudio.com/Visus.LdapAuthentication/_apis/build/status/UniStuttgart-VISUS.Visus.LdapAuthentication?branchName=main)](https://visualisierungsinstitut.visualstudio.com/Visus.LdapAuthentication/_build/latest?definitionId=6&branchName=main)
[![Visus.LdapAuthenticationVersion](https://buildstats.info/nuget/Visus.LdapAuthentication)](https://www.nuget.org/packages/Visus.LdapAuthentication)
[![Visus.DirectoryAuthenticationVersion](https://buildstats.info/nuget/Visus.DirectoryAuthentication)](https://www.nuget.org/packages/Visus.DirectoryAuthentication)

This project implements middleware for ASP.NET Core that enables authenticating users against LDAP directories like Active Directory via an LDAP bind.

There are two flavours, the first being [Visus.LdapAuthentication](Visus.LdapAuthentication/README.md), which uses [Novell's C#-only LDAP library](https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard) rather than the Windows-only DirectoryServices and is therefore running on Windows and Linux.

The second, [Visus.DirectoryAuthentication](Visus.DirectoryAuthentication/README.md), is a drop-in replacement using [System.DirectorySerices.Protocols](https://learn.microsoft.com/en-gb/dotnet/api/system.directoryservices.protocols), which is a platform-independent implementation of LDAP services since .NET 5, but requires native LDAP libraries for P/Invoke being installed.

Built-in user objects are automatically mapped to Active Directory attributes based on code annotations and include commonly used claims like user name, actual names, e-mail addresses and group memberships. If necessary, you can also provide your own user object that uses a completely different mapping of LDAP attributes to claims.


# Usage
## Visus.LdapAuthentication
See [README for Visus.LdapAuthentication](Visus.LdapAuthentication/README.md).

## Visus.DirectoryAuthentication
See [README for Visus.DirectoryAuthentication](Visus.DirectoryAuthentication/README.md)

## What's new in version 2?
Version 2.0 is a major rewrite of both libraries, which removes previously deprecated functionality and unifies large parts of the implementation between LdapAuthentication and DirectoryAuthentication in the [`Visus.Ldap.Core`](Visus.Ldap.Core) library. The most important changes to the 1.x branch are:
1. Both libraries now require at least .NET 8.
1. Besides the user object, which can be mapped to LDAP properties, a new group object allows for customising the mapping of group attributes as well. The indirection via the `ILdapUser` interface has been removed.
1. The mapping between LDAP entries and user/group objects is now performed by a `LdapMapper` class, which can be replaced by users of the library. The default implementation of the mapper uses reflection and the attribute annotations from previous versions of the library to support arbitrary user/group classes.
1. Similarly, the mapping between the user/group properties and `Claim`s is now performed by a `ClaimsBuilder` class, which can be replaced by users of the library. The default implementation of the builder uses reflection and the attribute annotations form previous versions of the library to support arbitrary user/group classes.
1. In addition to creating `Claim`s from user/group object, the library now supports direct creation of `Claim`s from LDAP entries via the `ClaimsMapper`, which can be replaced by users of the library. The default implementation of the mapper uses reflection and the attribute annotations from previous versions of the library to support arbitrary user/group classes.
1. The `ILdapOptions` interface has been removed. All configuration is performed via the common options pattern and the `LdapOptions` class.
1. The library now provides a validator for the `LdapOptions`, which is exectued on startup, thus preventing the application from starting if obvious configuration errors have been made.
1. All services including the LDAP configuration are now injected by `AddLdapAuthentication`. Extension methods for adding subsets of the services have been removed.
1. Both libraries now use a `System.TimeSpan` for configuring timeouts. When configuring from JSON, use a string in the format "hh:mm:ss".
1. Both libraries now support and require an array of search bases.
1. Both libraries now support and require an array of servers.
1. Both libraries now support `async`/`await`.
1. Both libraries support creation of `ClaimsPrincipal`s instead of custom user objects to facilitate the implementation of login controllers.

## Differences between LdapAuthentication and DirectoryAuthentication
[Visus.DirectoryAuthentication](Visus.DirectoryAuthentication) and [Visus.LdapAuthentication](Visus.LdapAuthentication) can mostly be used interchangeably with a few exceptions:
1. `System.DirectorySerices.Protocols` requires native LDAP libraries for P/Invoke being installed. This should be the case for all Windows platforms by default, but on Linux, `libldap` must be installed.
1. `LdapOptions.RootCaThumbprint` is not supported. You can, however, check the immediate issuer of the server's certificate using `ILdapOptions.ServerCertificateIssuer`.
1. Visus.DirectoryAuthentication supports different authentication types, which are dependent on the underlying native implementation, though.
