# Visus.DirectoryAuthentication
Visus.DirectoryAuthentication implements LDAP authentication using [System.DirectorySerices.Protocols](https://learn.microsoft.com/en-gb/dotnet/api/system.directoryservices.protocols), which is a platform-independent implementation of LDAP services since .NET 5.


# Usage
1. [Make sure the prerequisites are installed](#make-sure-the-prerequisites-are-installed)
1. [Add the authentication service](#add-the-authentication-service)
1. [Configure the LDAP server](#configure-the-ldap-server)
1. [Authenticate a user](#authenticate-a-user)
1. [Customising the user object](#customising-the-user-object)
1. [Customising the group object](#customising-the-group-object)
1. [Customising claims](#customising-claims)
1. [Searching users](#searching-users)
1. [Differences between LdapAuthentication and DirectoryAuthentication](#differences-between-ldapauthentication-and-directoryauthentication)

## Make sure the prerequisites are installed
[System.DirectorySerices.Protocols](https://learn.microsoft.com/en-gb/dotnet/api/system.directoryservices.protocols) requires native LDAP libraries for P/Invoke being installed. This should be the case for all Windows platforms by default, but on Linux, `libldap` must be installed. Please note that P/Invoke requires the [name of the library being hard-coded](https://github.com/dotnet/runtime/issues/69456), which might be a problem. There are basically [two ways for you to resolve this](https://decovar.dev/blog/2022/06/16/dotnet-ldap-authentication/#platform-specific-dependencies), which is installing the expected version or by creating a symlink that pretends the current version is the expected one.

## Add the authentication service
The authentication functionality is added in `ConfigureServices` in your `Startup` class via the following statements:

```C#
using Visus.DirectoryAuthentication;
// ...

public void ConfigureServices(IServiceCollection services) {
    // ...

    // Add LDAP authentication with default LdapUser object.
    services.AddLdapAuthenticationService(o => {
        this.Configuration.GetSection("LdapConfiguration").Bind(o);
    });

    // ...
}
```

Or using the new "minimal hosting model": 
```C#
using Visus.DirectoryAuthentication;
// ...

var builder = WebApplication.CreateBuilder(args);
// ...

// Add LDAP authentication with default LdapUser object.
builder.Services.AddLdapAuthenticationService(o => {
    builder.Configuration.GetSection("LdapConfiguration").Bind(o);
});
// ...
```

The above code uses the default `LdapUser` and `LdapGroup` objects from the library, which provides the most commonly used user claims. If you need additional claims or differently mapped claims, you can create your own user class either by inheriting from `LdapUserBase` and [customising its behaviour](#customising-the-user-object). The configuration would look like the following in this case:

```C#
using Visus.DirectoryAuthentication;
// ...

public void ConfigureServices(IServiceCollection services) {
    // ...

    // Add LDAP authentication with customised user object.
    services.AddLdapAuthenticationService<CustomApplicationUser, CustomApplicationGroup>(o => {
        this.Configuration.GetSection("LdapConfiguration").Bind(o);
    });

    // ...
}
```

## Configure the LDAP server
The configuration section can have any name of your choice as long as it can be bound to [`LdapOptions`](LdapOptions.cs). The following example illustrates a fairly minimal configuration for an Active Directory using SSL, but no certificate validation (this is what you would use for development purposes):

```JSON
{
    "LdapConfiguration": {
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

> [!NOTE]
> Support for multiple serviers is in preparation! Only the first one will be used at the moment.

While you can fully customise the properties and claims the library loads for a user (see below), there are certain things that must be provided. This is controlled via the `Schema` property in the JSON above. The schema selects the [`LdapMapping`](LdapMapping.cs) the library uses the select users and determine group membership. We provide several built-in schemas for frequently used LDAP servers in  [`LdapOptions`](LdapOptions.cs), namely "Active Directory" for Active Directory Domain Services, "IDMU" for Active Directory with Identity Management for Unix installed and "RFC 2307" for this RFC, which is the schema typically used be OpenLDAP servers.

The built-in schemas are hard-coded in the library like this:
```C#
new LdapMapping() {
    DistinguishedNameAttribute = "distinguishedName",
    GroupIdentityAttribute = "objectSid",
    GroupIdentityConverter = typeof(SidConverter).FullName,
    GroupsAttribute = "memberOf",
    PrimaryGroupAttribute = "primaryGroupID",
    UserFilter = "(|(sAMAccountName={0})(userPrincipalName={0}))",
    UsersFilter = "(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))"
}
```

You can, however, provide your own mapping in the JSON configuration like this:
```JSON
{
    "Mapping": {
        "DistinguishedNameAttribute": "distinguishedName",
        "GroupIdentityAttribute": "objectSid",
        "GroupIdentityConverter": "Visus.DirectoryAuthentication.SidConverter",
        "GroupsAttribute": "memberOf",
        "PrimaryGroupAttribute": "primaryGroupID",
        "UserFilter": "(|(sAMAccountName={0})(userPrincipalName={0}))",
        "UsersFilter": "(&(objectClass=user)(objectClass=person)(!(objectClass=computer)))"
    }
}
```
Alternatively, it is also possible to customise the `Mappings` property and select a custom schema by its name (the key in `Mappings`). Finally, if you set your `LdapOptions` in code, you can customise `LdapOptions.Mapping` or `LdapOptions.Mappings` from there.

The following properties must/can be set via JSON:
| Property | Description |
|----------|-------------|
| DistinguishedNameAttribute | The name of the LDAP attribute holding the distinguished name. This property is required and should be "distinguishedName". |
| GroupIdentityAttribute | The name of the LDAP attribute holding the unique identifier of a group. For Active Directory, this is typically the SID, whereas for POSIX, it is the GID number. This property is required. |
| GroupIdentityConverter | If the group identity needs some conversion to be usable, provide the full path to your class implementing [`ILdapAttributeConverter`](ILdapAttributeConverter.cs). |
| GroupsAttribute | The name of the LDAP attribute holding the list of groups a user is member of. This is "memberOf" in most scenarios. This property is required to create group claims. |
| PrimaryGroupAttribute | The name of the LDAP attribute storing the primary group of a user. Both, Active Directory and OpenLDAP, distinguish between the primary group and other groups, so both attributes must be provided for all group claims to be found. This property is required to create group claims. |
| RequiredGroupAttributes | An array of the attributes the library must load for group objects. This should typically not be customised as the library composes it from the other attributes set in the object. |
| UserFilter | The LDAP filter that allows the library to select the user by the user name that is input into the login field. This should cover all inputs that allow the user to bind to the LDAP server. For instance, Active Directory does not only allow for binding via the user name (`sAMAccountName`), but also via user@domain (`userPrincipalName`), so both ways need to be specified in the `UserFilter`. Technically, users could also bind via the distinguished name, but this is typcially not relevant for real-world scenarios, so our built-in mapping does not include this. If you fail to specify the correct filter here, users might be able to authenticate (bind to the LDAP server), but the authentication in the library will fail because the user object cannot be retrieved. This property is required. |
| UsersFilter | The LDAP filter that allows for selecting all users. Please note that for both, Active Directory and OpenLDAP, users are people and machines, so you want to filter on people only here. This property is required if you want to user [`ILdapSearchService`](ILdapSearchService.cs). |

## Authenticate a user
Once configured, the middleware can be used in controllers to implement cookie-based or JWT-based authorisation. An example for a cookie-based login method looks like:
```C#
// Inject ILdapAuthenticationService to _authService field in constructor.
public MyLoginController(ILdapAuthenticationService authService) {
    this._authService = authService;
}

[HttpPost]
[AllowAnonymous]
public async Task<ActionResult<ILdapUser>> Login([FromForm] string username, [FromForm] string password) {
    try {
        var retval = this._authService.Login(username, password);
        await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, retval.ToClaimsPrincipal());
        return this.Ok(retval);
    } catch (DirectoryOperationException ex) {
        // For LDAP errors, it might be helpful to have the error message from
        // the server.
        this._logger.LogError(ex, ex.Response.ErrorMessage);
        return this.Unauthorized();
    } catch {
        return this.Unauthorized();
    }
}
```

## Customising the user object
The built-in [`LdapUser`](LdapUser.cs) object provides a reasonably mapping of attributes in an Active Directory to user claims. There are two ways you can customise this behaviour.

The first one is by inheriting from [`LdapUserBase`](LdapUserBase.cs), which actually implements all of the behaviour of `LdapUser`. This way enables you to inherit most of this behaviour and override the mapping on a per-property base. As the mapping configured via attributes is not inherited, you can simply override a property and attach a new mapping like this:

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

    /// <summary>
    /// Gets the LDAP object class.
    /// </summary>
    /// <remarks>
    /// Here, an additional LDAP attribute is loaded from the directory. The
    /// <see cref="ILdapMapper{TUser, TGRoup}" /> uses the attribute to determine
    /// where to get the information in Active Directory from.
    /// </remarks>
    [LdapAttribute(Schema.ActiveDirectory, "objectClass")]
    public string ObjectClass { get; set; }
}
```

If you need an even higher level of customisation, you can provide a custom mapper by providing your own `ILdapMapper` like this:

```C#
using Visus.DirectoryAuthentication;
// ...

public void ConfigureServices(IServiceCollection services) {
    // ...

    // Add LDAP authentication with default LdapUser object.
    services.AddLdapAuthenticationService<
            CustomApplicationUser,
            CustomApplicationGroup,
            ClaimsBuilder<CustomApplicationUser, CustomApplicationGroup>,
            CustomMapper>(o => {
        this.Configuration.GetSection("LdapConfiguration").Bind(o);
    });

    // ...
}
```

In your custom mapper, you are free to assign properties as you want or use the default behaviour. Have a look at the implementation of [`LdapMapper`](LdapMapper.cs) to see how to implement such a mapper. When providing a pair of user/group objects and mapper, you are not limited to using reflecting for your implementation, but you can directly access the properties.

If you provide a custom implementation and want to rely on [`LdapMapper`](LdapMapper.cs), you need to annotate the properties of your user object in a similar way. The mapper relies on `LdapAttribute` to find the LDAP attributes that should be assigned to a property of the user object. Except for special cases like the `objectSid`, the properties you map should be `string`s.

You can annotate a property of type `IEnumerable<TGroup>` with the `LdapGroups` attribute, which will instruct the default mapper to store the groups in this property. Likewise, you can annotate a property of type `IEnumerable<System.Security.Claims.Claim>` with the `Claims` attribute to instruct the mapper to store the generated claims there. You should annotate the property storing the unique identity of a user with the `LdapIdentity` property.


## Customising the group object
In a similar way you can provide your own replacement of `LdapUser`, you can also provide a replacement for `LdapGroup`, contains the information to create group-based claims. Like for the user, you can rely on [`LdapMapper`](LdapMapper.cs) and annotations via attributes or customise the assignment of LDAP attributes to properties in a custom mapper.


## Customising claims
If you do not need additional information from the directory than what is provided by `LdapUser` and `LdapGroup`, but you want to customise the `System.Security.Claims.Claim`s generated, you could consider providing a custom `IClaimsBuilder` to make these claims from the information provided by the user object. Have a look at the default [`ClaimsBuilder`](ClaimsBuilder.cs) for inspiration on how to do this. The default buider uses the `Claim` attribute to translate properties to claims.

## Searching users
In some cases, you might want to search users objects without authenticating the user of your application. One of these cases might be restoring the user object from the claims stored in a cookie. A service account specified in `LdapOptions.User` with a password stored in `LdapOptions.Password` can be used in conjuction with a [`ILdapSearchService`](ILdapSearchService.cs) to implement such a behaviour. First, configure the service:

```C#
using Visus.DirectoryAuthentication;
// ...

public void ConfigureServices(IServiceCollection services) {
    // ...

    // Add LDAP search service using service account.
    services.AddLdapSearchService(o => {
        this.Configuration.GetSection("LdapConfiguration").Bind(o);
    });

    // ...
}
```

Alternatively, register authentication and search at the same time:

```C#
using Visus.DirectoryAuthentication;
// ...

public void ConfigureServices(IServiceCollection services) {
    // ...

    // Add LDAP authentication and search services.
    services.AddLdapServices(o => {
        this.Configuration.GetSection("LdapConfiguration").Bind(o);
    });

    // ...
}
```

Assuming that you have the embedded the user SID in the claims of an authentication cookie, you then can restore the user object from the cookie as follows:

```C#
// Inject ILdapSearchService to _ldapSearchService field in constructor.
public MyLoginController(ILdapSearchService searchService) {
    this._searchService = searchService;
}

[HttpGet]
[Authorize]
public ActionResult<ILdapUser> GetUser() {
    if (this.User != null) {
        // Determine the claims we know that the authentication service has
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

You may also want to use the search service if your LDAP server requires users to bind using their distinguished name, but you do not want to force them to remember this name. In this case, you can perform a search for another attribute and retrieve the distinguished name of a matching entry. For example:

```C#
// Inject ILdapAuthenticationService and ILdapSearchService in constructor.
public MyLoginController(ILdapAuthenticationService authService, ILdapSearchService searchService) {
    this._authService = authService;
    this._searchService = searchService;
}

[HttpPost]
[AllowAnonymous]
public async Task<ActionResult<ILdapUser>> Login([FromForm] string username, [FromForm] string password) {
    try {
        // Retrieve the distinguished name for the user name in an RFC 2307
        // schema. Please note that you should call .Single() on the result
        // in order to prevent users hijacking other accounts in case your
        // filter was poorly designed like here. For instance, the user could
        // insert the wild card character "*" as his name and then the random
        // first match would be returned, which is not what we want.
        var dn = this._searchService.GetDistinguishedNames($"(&(objectClass=posixAccount)(uid={username}))").Single();
        var retval = this._authService.Login(dn, password);
        await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, retval.ToClaimsPrincipal(CookieAuthenticationDefaults.AuthenticationScheme));
        return this.Ok(retval);
    } catch {
        return this.Unauthorized();
    }
}
```
