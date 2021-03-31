# LDAP Authentication Middleware
This library implements middleware for ASP.NET Core that enables authenticating users against LDAP directories like Active Directory via an LDAP bind. The user objects generated also include commonly used claims like user name, actual names, e-mail addresses and group memberships.

# Usage
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
    
    // Add LDAP authentication with default LdapUser object.
    {
        var options = new LdapOptions();
        this.Configuration.GetSection("LdapConfiguration").Bind(options);
        services.AddLdapAuthenticationService<CustomApplicationUser>(options);
    }
    
    // ...
}
```

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
