// <copyright file="Programme.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Identity;
using Visus.DirectoryAuthentication.Configuration;
using Visus.DirectoryIdentity;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddLdapUserManager<IdentityUser>();
builder.Services.AddDefaultIdentity<IdentityUser>().AddIdentityLdapStore(o => {
    builder.Configuration.GetSection(LdapOptions.Section).Bind(o);
});
builder.Services.AddRazorPages();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
