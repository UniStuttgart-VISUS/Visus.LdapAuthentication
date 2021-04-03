# ASP.NET Core LDAP Authentication Middleware

[![Build Status](https://visualisierungsinstitut.visualstudio.com/Visus.LdapAuthentication/_apis/build/status/UniStuttgart-VISUS.Visus.LdapAuthentication?branchName=main)](https://visualisierungsinstitut.visualstudio.com/Visus.LdapAuthentication/_build/latest?definitionId=6&branchName=main)
[![Visus.LdapAuthenticationVersion](https://buildstats.info/nuget/Visus.LdapAuthentication)](https://www.nuget.org/packages/Visus.LdapAuthentication)


This library implements middleware for ASP.NET Core that enables authenticating users against LDAP directories like Active Directory via an LDAP bind. The library is using [Novell's C#-only LDAP library](https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard) rather than the Windows-only DirectoryServices and is therefore running on Windows and Linux.

Built-in user objects are automatically mapped to Active Directory attributes and include commonly used claims like user name, actual names, e-mail addresses and group memberships. If necessary, you can also provide your own user object that uses a completely different mapping of LDAP attributes to claims.


# Usage
## Add the authentication service
The authenication functionality is added in `ConfigureServices` via the following statements:

```C#
public void ConfigureServices(IServiceCollection services) {
    // ...
    
    // Add LDAP authentication with default LdapUser object.
    {
        var options = new LdapOptions();
        this.Configuration.GetSection("LdapConfiguration").Bind(options);
        services.AddLdapAuthenticationService(options);
    }
    
    // ...
}
```

The above code uses the default `LdapUser` object from the library, which provides the most commonly used user claims. If you need additional claims or differently mapped claims, you can create your own user class either by inheriting from `LdapUserBase` and customising its behaviour or by implementing `ILdapUser` from scratch. The configuration would look like the following in this case:

```C#
public void ConfigureServices(IServiceCollection services) {
    // ...
    
    // Add LDAP authentication with customised user object.
    {
        var options = new LdapOptions();
        this.Configuration.GetSection("LdapConfiguration").Bind(options);
        services.AddLdapAuthenticationService<CustomApplicationUser>(options);
    }
    
    // ...
}
```

## Configure the LDAP server
The configuration section can have any name of your choice as long as it can be bound to `LdapConfiguration`. Alternatively, you can use your own implementation of `ILdapConfiguration`. The following example illustrates a fairly minimal configuration for an Active Directory using SSL, but no certificate validation (this is what you would use for development purposes):

```JSON
{
    "LdapConfiguration": {
        "Server": "dc.your-domain.de",
        "SearchBase": "DC=your-domain,DC=de",
        "Schema": "Active Directory",
        "IsRecursiveGroupMembership": true,
        "Port": 636,
        "IsSsl": true,
        "IsNoCertificateCheck": true
    }
}
```

## Authenticate a user
Once configured, the middleware can be used in controllers to implement cookie-based or JWT-based authorisation. An example for a cookie-based login method looks like:
```C#
[HttpPost]
[AllowAnonymous]
public ActionResult<ILdapUser> Login([FromForm] string username, [FromForm] string password) {
    try {
        var retval = this._authService.Login(username, password);
        this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, retval.ToClaimsPrincipal());
        return this.Ok(retval);
    } catch {
        return this.Unauthorized();
    }
}
```

## Customising the user object
The built-in `LdapUser` object provides a reasonably mapping of attributes in an Active Directory to user claims. There are two ways you can customise this behaviour.

The first one is by inheriting from `LdapUserBase`, which actually implements all of the behaviour of `LdapUser`. This way enables you to inherit most of this behaviour and override the mapping on a per-property base. As the mapping configured via attributes is not inherited, you can simply override a property and attach a new mapping like this:

```C#
public sealed class CustomApplicationUser : LdapUserBase {

    /// <summary>
    /// The user's account name.
    /// </summary>
    /// <remarks>
    /// Here, the &quot;userPrincipalName&quot; is used instead of
    /// &quot;sAMAccountName&quot; used by <see cref="LdapUser" />. Furthermore,
    /// only the <see cref="ClaimTypes.WindowsAccountName" /> claim is set to
    /// this property, whereas <see cref="LdapUser" /> also sets
    /// <see cref="ClaimTypes.Name" />. All other attribute mappings and claim
    /// mappings are inherited from <see cref="LdapUserBase" /> and therefore
    /// behave like the default <see cref="LdapUser" />.
    /// </remarks>
    [LdapAttribute(Schema.ActiveDirectory, "userPrincipalName")]
    [Claim(ClaimTypes.WindowsAccountName)]
    public override string AccountName => base.AccountName;
}
```

If you need an even higher level of customisation, you can provide a completely new implementation of `ILdapUser` and fully control the whole mapping of LDAP attributes to properties and claims. Before doing so, you should also consider whether you can achieve your goals by overriding one or more of `LdapUserBase.AddGroupClaims` and `LdapUserBase.AddPropertyClaims`.
