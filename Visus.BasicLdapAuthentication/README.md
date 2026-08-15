# Visus.BasicLdapAuthentication
Visus.BasicLdapAuthentication implements the basic (header-based) authentication scheme for ASP.NET Core application using Visus.DirectoryAuthentication or Visus.LdapAuthentication as backends.

# Usage
1. [Add the services](#add-the-services)
1. [Interface with cookie authentication](#interface-with-cookie-authentication)

## Add the services
The authentication functionalityBasic is added in `ConfigureServices` in your `Startup` class or using the `WebApplication` builder in the minimal hosting model:

Or using the new "minimal hosting model": 
```C#
using Visus.BasicLdapAuthentication;
using Visus.LdapAuthentication; // or Visus.DirectoryAuthentication;
// ...

var builder = WebApplication.CreateBuilder(args);
// ...

// Add basic authentication using LDAP.
builder.Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
    .AddBasicLdapAuthentication(o => {
        builder.Configuration.GetSection(BasicAuthenticationOptions.Section).Bind(o);
        // The following statement filters out group-based claims as an example.
        o.ClaimFilter = (c, _) => (c != ClaimTypes.GroupSid) && (c != ClaimTypes.Role);
    });
});

// Add Visus.LdapAuthentication or Visus.DirectoryAuthentication as the
// authentication backend. No further configuration is required as the basic
// authentication handler will pull the configuration from the DI container.
builder.Services.AddLdapAuthentication(o => {
    builder.Configuration.GetSection(LdapOptions.Section).Bind(o);
});

// ...

app.UseAuthentication();
app.UseAuthorization();

/...
```

## Interface with cookie authentication
As pulling the login data from the LDAP server for each request can be expensive, the handler has the ability to interface the cookie authentication middleware:
```C#
builder.Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => {
        // The login cookie is essential in the sense of GDPR.
        o.Cookie.IsEssential = true;
    }).AddBasicLdapAuthentication(o => {
        builder.Configuration.GetSection(BasicAuthenticationOptions.Section).Bind(o);

        // Tell the basic authentication handler to sign in using the default
        // cookie scheme.
        o.CookieScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        // Allow the cookie to be refreshed.
        o.AllowCookieRefresh = true;
        // Set the lifetime of the cookie.
        o.CookieLifetime = TimeSpan.FromMinutes(30);
        // Allow the cookie to outlive the browser session.
        o.PersistentCookie = true;
    });
```
