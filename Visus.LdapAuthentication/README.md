# Visus.LdapAuthentication
Visus.LdapAuthentication implements middleware for ASP.NET Core that enables authenticating users against LDAP directories like Active Directory via an LDAP bind. The library is using [Novell's C#-only LDAP library](https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard) rather than the Windows-only DirectoryServices and is therefore running on Windows and Linux.


# Usage
1. [Make sure the prerequisites are installed](#make-sure-the-prerequisites-are-installed)
1. [Add the LDAP services](#add-the-ldap-services)
1. [Configure the LDAP server](#configure-the-ldap-server)
1. [Authenticate a user](#authenticate-a-user)
1. [Customising LDAP mappings](#customising-ldap-mappings)
1. [Searching users](#searching-users)

## Add the LDAP services
The authentication functionality is added in `ConfigureServices` in your `Startup` class via the following statements:

```C#
using Visus.LdapAuthentication;
// ...

public void ConfigureServices(IServiceCollection services) {
    // ...

    // Add LDAP authentication with default LdapUser object.
    services.AddLdapAuthentication(o => {
        this.Configuration.GetSection("LdapOptions").Bind(o);
    });

    // ...
}
```

Or using the new "minimal hosting model": 
```C#
using Visus.LdapAuthentication;
// ...

var builder = WebApplication.CreateBuilder(args);
// ...

// Add LDAP authentication with default LdapUser object.
builder.Services.AddLdapAuthentication(o => {
    this.Configuration.GetSection(LdapOptions.Section).Bind(o);
});
// ...
```

The above code uses the default `LdapUser` and `LdapGroup` objects from the library, which provides the most commonly used user claims. If you need additional claims or differently mapped claims, you can create your own user class either by inheriting from `LdapUserBase` and [customising its behaviour](#customising-the-user-object). The configuration would look like the following in this case:
```C#
using Visus.LdapAuthentication;
// ...

var builder = WebApplication.CreateBuilder(args);
// ...

// Add LDAP authentication with default LdapUser object.
builder.Services.AddLdapAuthentication<MyUser, MyGroup>(o => {
    this.Configuration.GetSection(LdapOptions.Section).Bind(o);
});
// ...
```

## Configure the LDAP server
The configuration section can have any name of your choice as long as it can be bound to [`LdapOptions`](Configuration/LdapOptions.cs). The following example illustrates a fairly minimal configuration for an Active Directory using SSL, but no certificate validation (this is what you would use for development purposes):
```JSON
{
    "LdapOptions": {
        "Servers": [ "dc.your-domain.de" ],
        "Port": 636,
        "IsSsl": true,
        "IsNoCertificateCheck": true
        "SearchBases": { "DC=your-domain,DC=de": "Subtree" },
        "Schema": "Active Directory",
        "IsRecursiveGroupMembership": true,
    }
}
```

While you can fully customise the properties and claims the library loads for users and groups (see below), there are certain things that must be provided for the library being able to retrieve the group hierarchy. This is controlled via the `Schema` property in the JSON above. The schema selects the `LdapMapping` the library uses to select users and determine group membership. We provide several built-in schemas for frequently used LDAP servers in `LdapOptionsBase`, namely "Active Directory" for Active Directory Domain Services, "IDMU" for Active Directory with Identity Management for Unix installed and "RFC 2307" for this RFC, which is the schema typically used be OpenLDAP servers.

The built-in schemas are hard-coded in the library like this:
```C#
new LdapMapping() {
    GroupsAttribute = "memberOf",
    PrimaryGroupAttribute = "primaryGroupID",
    PrimaryGroupIdentityAttribute = "objectSid",
    UserFilter = "(|(sAMAccountName={0})(userPrincipalName={0}))",
    UsersFilter = "(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))"
}
```

You can, however, provide your own mapping in the JSON configuration like this:
```JSON
{
    "Mapping": {
        "GroupsAttribute": "memberOf",
        "PrimaryGroupAttribute": "primaryGroupID",
        "PrimaryGroupIdentityAttribute:" "objectSid",
        "UserFilter": "(|(sAMAccountName={0})(userPrincipalName={0}))",
        "UsersFilter": "(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))"
    }
}
```

The properties of a mapping are:

- **DistinguishedNameAttribute:** The attribute to get the distinguished name of a user or group. This defaults to "distinguishedName" and there should be no need to modify it.
- **GroupsAttribute:** The attribute where (non-primary) groups are stored. The mapper uses this information to assign groups to user objects.
- **PrimaryGroupAttribute:** The attribute where the SID or GID of the primary group is stored in a user's entry.
- **PrimaryGroupIdentityAttribute:** The attribute where the SID or GID is stored within the entry of the primary group.
- **UserFilter:** The LDAP filter that allows the library to select the user by the user name that is input into the login field. This should cover all inputs that allow the user to bind to the LDAP server. For instance, Active Directory does not only allow for binding via the user name (`sAMAccountName`), but also via user@domain (`userPrincipalName`), so both ways need to be specified in the `UserFilter`. Technically, users could also bind via the distinguished name, but this is typcially not relevant for real-world scenarios, so our built-in mapping does not include this. If you fail to specify the correct filter here, users might be able to authenticate (bind to the LDAP server), but the authentication in the library will fail because the user object cannot be retrieved.
- **UsersFilter:** The LDAP filter that allows for selecting all users. Please note that for both, Active Directory and OpenLDAP, users are people and machines, so you want to filter on people only here. This property is required if you want to user [`ILdapSearchService`](ILdapSearchService.cs).

## Authenticate a user
There are two methods for authenticating a user, the one returning the user object you registered, the other directly returns a `ClaimsPrincipal`. These two methods can be used in controllers to implement cookie-based or JWT-based authorisation. An example for a cookie-based login method using user and group objects looks like:
```C#
// Inject ILdapAuthenticationService to _authService field in constructor.
public MyLoginController(ILdapAuthenticationService<LdapUser> authService,
        ILogger<MyLoginController> logger) {
    this._authService = authService;
    this._logger = logger;
}

[HttpPost]
[AllowAnonymous]
public async Task<ActionResult<ILdapUser>> Login([FromForm] string username, [FromForm] string password) {
    try {
        (var retval, var claims) = await this._authService.LoginUserAsync(username, password, null);
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, princial);
        return this.Ok(retval);
    } catch (LdapException ex) {
        // For LDAP errors, it might be helpful to have the error message from
        // the server.
        this._logger.LogError(ex, ex.ServerErrorMessage);
        return this.Unauthorized();
    } catch {
        return this.Unauthorized();
    }
}
```

## Customising LDAP mappings

### Customising the user object
The built-in `LdapUser` object provides a reasonable mapping of attributes in an Active Directory to user claims. There are two ways you can customise this behaviour: the first is by inheriting from `LdapUser` and adding additional properties that are mapped to LDAP attributes (in contrast to version 1.x of the library, it is not possible to change existing mappings of `LdapUser`), or you can provide a completely new class where all attributes are mapped according to your needs.

An example for addition additional information to the existing `LdapUser` might be the retrieval of the profile picture for display on the website:
```C#
public sealed class MyUser : LdapUser {

    /// <summary>
    /// Gets or sets the base64-encoded profile picture of the user.
    /// </summary>
    [LdapAttribute("Active Directory", "thumbnailPhoto", Converter = typeof(BinaryConverter))]
    public string? ProfilePicture { get; set; }
}
```

If you create a fully customised user, make sure to annotate properties with special meanings accordingly to allow the mapper to understand your class (alternatively, you could also provide your own `ILdapMapper`). These attributes are:

- **[AccountName]:** Marks the (Windows) account name and enables the `ILdapSearchService` to search users by their account name.
- **[DistinguishedName]:** Marks the distinguished name and enables the `ILdapSearchService` to search users by their distinguished name.
- **[Identity]:** Marks the identity (SID or UID/GID) and enables the `ILdapSearchService` to search users by their ID.
- **[GroupMemberships]:** Marks an `IEnumerable<TGroup>` as the property that receives the groups a user belongs to.

A very minimal user object for an Active Directory could look like:
```C#
public sealed class MinimalUser {
    [LdapAttribute(Schema.ActiveDirectory, "sAMAccountName")]
    [Claim(ClaimTypes.Name)]
    [AccountName]
    public string AccountName { get; set; } = null!;

    [LdapAttribute(Schema.ActiveDirectory, "distinguishedName")]
    [DistinguishedName]
    public string DistinguishedName { get; set; } = null!;

    [LdapAttribute(Schema.ActiveDirectory, "objectSid", Converter = typeof(SidConverter))]
    [Claim(ClaimTypes.Sid)]
    [Identity]
    public string Identity { get; set; } = null!;
```

### Customising the group object
In a similar way you can provide your own replacement of `LdapUser`, you can also provide a replacement for `LdapGroup`, which contains the information to create group-based claims. Like for the user, you can rely on `LdapMapper` and annotations via attributes or customise the assignment of LDAP attributes to properties in a custom mapper.

Your own LDAP group object should be annotated with the same attributes described above for the user to allow the default mapper to reflect on them. Groups support an additional attribute:

- **[PrimaryGroupFlag]:** Marks a `bool` property which will be set `true` by the mapper if the group was retrieved from the primary group attribute of a user rather than from the list of group memberships.

### Customising claims
If you do not need additional information from the directory than what is provided by `LdapUser` and `LdapGroup`, but you want to customise the `System.Security.Claims.Claim`s generated, you should consider providing a custom `IClaimsBuilder` to make these claims from the information provided by the user object. The `ILdapAuthenticationService` uses this class to obtain the claims for a user in the `LoginUser` method that returns a user object and its claims. Have a look at the default [`ClaimsBuilderBase`](Claims/ClaimsBuilder.cs) in `Visus.Ldap.Core` for inspiration on how to do this. The default builder uses the `Claim` attribute to translate properties to claims.

Note that if you login a `System.Security.Claims.ClaimPrincipal` directly, the `ILdapAuthentication` will use `IClaimsMapper` instead of `IClaimsBuilder` to map LDAP entries directly to claims without the intermediate step of making a user object. As for the `IClaimsBuilder`, you can replace the default [`ClaimsMapperBase`](Claims/ClaimsMapper.cs) from `Visus.Ldap.Core` with your own one.

Please be aware that, depending on your group hierarchy, a user might be member of a group via multiple paths. The library can recursively enumerate the whole group hierarchy, but it will not eliminate duplicates when doing so. In order to remove duplicate claims from the results of `IClaimsBuilder` and `IClaimsMapper`, we provide the `Visus.Ldap.Claims.ClaimsEqualityComparer`, which can be used with the `Distinct` LINQ method to remove duplicate claims.

Finally, if your users are member of many groups, the list of claims might get larger than it needs to be for your specific application. The claims builder and mapper therefore accept a `Visus.Ldap.Claims.ClaimFilter` callback, which allows you to select the claims you are interested in while they are collected.

## Searching users
In some cases, you might want to search user entries without authenticating the user of your application. One of these cases might be restoring the user object from the claims stored in a cookie. A service account specified in `LdapOptions.User` with a password stored in `LdapOptions.Password` can be used in conjuction with a [`ILdapSearchService`](ILdapSearchService.cs) to implement such a behaviour.

> [!NOTE]
> The LDAP search service has the credentials of the service account configured in the options, not of a user of your application. If you need to perform actions on behalf of a user, use `ILdapConnectionService` to obtain a connection as the user.

Assuming that you have the embedded the user SID in the claims of an authentication cookie, you then can restore the user object from the cookie as follows:

```C#
// Inject ILdapSearchService to _ldapSearchService field in constructor.
public MyLoginController(ILdapSearchService<LdapUser, LdapGroup> searchService) {
    this._searchService = searchService;
}

[HttpGet]
[Authorize]
public ActionResult<LdapUser> GetUser() {
    if (this.User != null) {
        // Determine the claim types we know that the authentication service has
        // stored the SID to.
        var claims = ClaimAttribute.GetClaims<LdapUser>(nameof(LdapUser.Identity));

        // Return the first valid SID that allows for reconstructing the user.
        foreach (var c in claims) {
            var sid = this.User.FindFirstValue(c);
            if (!string.IsNullOrEmpty(sid)) {
                var retval = this._searchService.GetUserByIdentity(sid);

                if (retval != null) {
                    return this.Ok(retval);
                }
            }
        }
    }

    // If something went wrong, we assume that the (anonymous) user must not
    // access the user details.
    return this.Unauthorized();
}
```

You might also want to use the search service if your LDAP server requires users to bind using their distinguished name, but you do not want to force them to remember this name. In this case, you can perform a search for another attribute and retrieve the distinguished name of a matching entry. For example:

```C#
// Inject ILdapAuthenticationService and ILdapSearchService in constructor.
public MyLoginController(ILdapAuthenticationService<LdapUser> authService,
        ILdapSearchService<LdapUser, LdapGroup> searchService) {
    this._authService = authService;
    this._searchService = searchService;
}

[HttpPost]
[AllowAnonymous]
public async Task<ActionResult> Login([FromForm] string username,
        [FromForm] string password,
        [FromQuery] string? returnUrl) {
    try {
        // Retrieve the distinguished name for the user name in an RFC 2307
        // schema. If no unique user was found, the caller is unauthorised.
        var user = await this._searchService.GetUserByAccountNameAsync(username);
        if (user == null) {
            return this.Unauthorized();
        }

        // In this example, we directly retrieve a principal, because we do not
        // need the user object.
        var principal = await this._authService.LoginPrincipalAsync(user.DistinguishedName, password);
        await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return this.Redirect(returnUrl ?? "/");
    } catch {
        return this.Unauthorized();
    }
}
```
