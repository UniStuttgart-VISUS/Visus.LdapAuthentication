# ASP.NET Core LDAP Authentication Middleware

[![Build Status](https://visualisierungsinstitut.visualstudio.com/Visus.LdapAuthentication/_apis/build/status/UniStuttgart-VISUS.Visus.LdapAuthentication?branchName=main)](https://visualisierungsinstitut.visualstudio.com/Visus.LdapAuthentication/_build/latest?definitionId=6&branchName=main)
[![Visus.LdapAuthenticationVersion](https://img.shields.io/nuget/v/Visus.LdapAuthentication.svg)](https://www.nuget.org/packages/Visus.LdapAuthentication)
[![Visus.DirectoryAuthenticationVersion](https://img.shields.io/nuget/v/Visus.DirectoryAuthentication.svg)](https://www.nuget.org/packages/Visus.DirectoryAuthentication)

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

### Things to look at when upgrading
1. In your LDAP options section in appsettings.json, make sure to change "Server" to "Servers" and provide an array of at least one server.
1. In your LDAP options section in appsettings.json, make sure to change "SearchBase" to "SearchBases" and provide a map from the DN to the search scope.
1. In your LDAP options section in appsettings.json, make sure to replace "IsSsl" with the equivalent "TransportSecurity". **"IsSsl" is not honoured anymore! Without "TransportSecurity", your connections will not be encrypted!**
1. In your startup code, replace all previous dependency injection extension methods with a version of `AddLdapAuthentication`. The template parameters allow you to change the type of user and group that the LDAP entries are mapped to.
1. In your code, change all services to include the user and/or group type you want to use as generic parameters of the service interfaces.
1. In your code, update the `using` statements. Some shared classes like `LdapUser` have been moved from the `Visus.DirectoryAuthentication` and `Visus.LdapAuthentication` namespaces to the shared `Visus.Ldap` namespace. Furthermore, the namespaces are now structured to isolate LDAP mapping, claims mapping, etc.
1. In your code, replace `ILdapAuthenticationService.Login` with `ILdapAuthenticationService.LoginUser` or `ILdapAuthenticationService.LoginPrincipal` depending on your needs.
1. If you use `LdapAttributeAttribute.GetLdapAttribute` to reflect on LDAP attribute mappings in your code, inject `ILdapAttributeMap<LdapUserOrGroup>` to obtain similar information. `ILdapAttributeMap`s provide direct access to attribute names and `PropertyInfo`s and are more efficient than the previous on-demand reflection.

## Differences between LdapAuthentication and DirectoryAuthentication
[Visus.DirectoryAuthentication](Visus.DirectoryAuthentication) and [Visus.LdapAuthentication](Visus.LdapAuthentication) can mostly be used interchangeably with a few exceptions:
1. `System.DirectorySerices.Protocols` requires native LDAP libraries for P/Invoke being installed. This should be the case for all Windows platforms by default, but on Linux, `libldap` must be installed.
1. We found Visus.DirectoryAuthentication to be significantly faster on Windows systems than [Visus.LdapAuthentication](Visus.LdapAuthentication), particularly when using LDAPS. On Linux systems, however, we found that [Visus.LdapAuthentication](Visus.LdapAuthentication) is usually more reliable than [Visus.DirectoryAuthentication](Visus.DirectoryAuthentication) due to `System.DirectorySerices.Protocols` lacking a bunch of features on top of `libldap`.
1. `LdapOptions.RootCaThumbprint` is not supported. You can, however, check the immediate issuer of the server's certificate using `LdapOptions.ServerCertificateIssuer`.
1. [Visus.DirectoryAuthentication](Visus.DirectoryAuthentication) supports different authentication types, which are dependent on the underlying native implementation, though. This can be configured via `LdapOptions.AuthenticationType`.
1. While [Visus.DirectoryAuthentication](Visus.DirectoryAuthentication) relies on the underlying API to handle multiple servers, you can configure the behaviour of [Visus.LdapAuthentication](Visus.LdapAuthentication) in your application settings via the `LdapOptions.ServerSelectionPolicy`.
