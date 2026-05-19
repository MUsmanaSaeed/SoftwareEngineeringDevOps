using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SoftwareEngineeringDevOps.Components;
using SoftwareEngineeringDevOps.Persistence;
using SoftwareEngineeringDevOps.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Cookie authentication — encrypted, signed, secure by the ASP.NET Core data protection stack
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/auth/logout";
        options.Cookie.Name = "app_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.IsEssential = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Propagates the authentication state cascade to all Blazor components
builder.Services.AddCascadingAuthenticationState();

// Razor components with interactive server rendering
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Persistence — scoped per request / circuit
builder.Services.AddScoped<IUsersPersistenceService, UsersPersistenceService>();

// ViewModels — scoped per request / circuit
builder.Services.AddScoped<LoginVM>();
builder.Services.AddScoped<HomeVM>();
builder.Services.AddScoped<UserProfileVM>();
builder.Services.AddScoped<UserManagementVM>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Logout endpoint — signs the user out and redirects to login
app.MapPost("/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
