using SoftwareEngineeringDevOps.App.Manufacturers;
using SoftwareEngineeringDevOps.App.Manufacturers.Persistence;
using SoftwareEngineeringDevOps.App.Users;
using SoftwareEngineeringDevOps.App.Users.Persistence;
using SoftwareEngineeringDevOps.Components;


AppContext.SetSwitch("Npgsql.EnableStoredProcedureCompatMode", true);
Migrator.Migrator.Migrate();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IUsersDB, UsersDB>();
builder.Services.AddSingleton<IUsersMediator, UsersMediator>();

builder.Services.AddSingleton<IManufacturersDB, ManufacturersDB>();
builder.Services.AddSingleton<IManufacturersMediator, ManufacturersMediator>();

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
