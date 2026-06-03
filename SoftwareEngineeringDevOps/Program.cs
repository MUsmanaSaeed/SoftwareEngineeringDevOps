using Microsoft.AspNetCore.Components.Authorization;
using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.BrickOrders;
using SoftwareEngineeringDevOps.App.BrickOrders.Persistence;
using SoftwareEngineeringDevOps.App.BrickOrders.Repository;
using SoftwareEngineeringDevOps.App.BrickOrdersReceived;
using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence;
using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Repository;
using SoftwareEngineeringDevOps.App.Bricks;
using SoftwareEngineeringDevOps.App.Bricks.Persistence;
using SoftwareEngineeringDevOps.App.Bricks.Repository;
using SoftwareEngineeringDevOps.App.Manufacturers;
using SoftwareEngineeringDevOps.App.Manufacturers.Persistence;
using SoftwareEngineeringDevOps.App.Manufacturers.Repository;
using SoftwareEngineeringDevOps.App.Users;
using SoftwareEngineeringDevOps.App.Users.Persistence;
using SoftwareEngineeringDevOps.App.Users.Repository;
using SoftwareEngineeringDevOps.Components;
using SoftwareEngineeringDevOps.Components.Shared;


AppContext.SetSwitch("Npgsql.EnableStoredProcedureCompatMode", true);
Migrator.Migrator.Migrate();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

builder.Services.AddSingleton<IUsersDB, UsersDB>();
builder.Services.AddSingleton<IUsersRepository, UsersRepository>();
builder.Services.AddSingleton<IUsersMediator, UsersMediator>();

builder.Services.AddSingleton<IManufacturersDB, ManufacturersDB>();
builder.Services.AddSingleton<IManufacturersRepository, ManufacturersRepository>();
builder.Services.AddSingleton<IManufacturersMediator, ManufacturersMediator>();

builder.Services.AddSingleton<IBricksDB, BricksDB>();
builder.Services.AddSingleton<IBricksRepository, BricksRepository>();
builder.Services.AddSingleton<IBricksMediator, BricksMediator>();

builder.Services.AddSingleton<IBrickOrdersDB, BrickOrdersDB>();
builder.Services.AddSingleton<IBrickOrdersRepository, BrickOrdersRepository>();
builder.Services.AddSingleton<IBrickOrdersMediator, BrickOrdersMediator>();

builder.Services.AddSingleton<IBrickOrdersReceivedDB, BrickOrdersReceivedDB>();
builder.Services.AddSingleton<IBrickOrdersReceivedRepository, BrickOrdersReceivedRepository>();
builder.Services.AddSingleton<IBrickOrdersReceivedMediator, BrickOrdersReceivedMediator>();

// Authentication - scoped per circuit and persisted in protected browser storage
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthService>());
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthService>());

builder.Services.AddScoped<IToastService, ToastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
