// <copyright file="Programme.cs" company="Visualisierungsinstitut der Universität Stuttgart">
// Copyright © 2024 Visualisierungsinstitut der Universität Stuttgart.
// Licensed under the MIT licence. See LICENCE file for details.
// </copyright>
// <author>Christoph Müller</author>

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using Visus.BasicLdapAuthentication;
using Visus.BasicLdapAuthentication.Configuration;
using Visus.LdapAuthentication;
using Visus.LdapAuthentication.Configuration;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => {
        o.Cookie.IsEssential = true;
    }).AddBasicLdapAuthentication(o => {
        builder.Configuration.GetSection(BasicAuthenticationOptions.Section).Bind(o);
        o.CookieScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        o.ClaimFilter = (c, _) => (c != ClaimTypes.GroupSid) && (c != ClaimTypes.Role);
    });
builder.Services.AddLdapAuthentication(o => {
        builder.Configuration.GetSection(LdapOptions.Section).Bind(o);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
