using Application;
using Core.Entities.Identity;
using DataAccess;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// DATABASE
// ==========================
builder.Services.AddDatabaseConfiguration(builder.Configuration);

// ==========================
// APPLICATION
// ==========================
builder.Services.AddApplicationConfiguration();

// ==========================
// IDENTITY
// ==========================
builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

// ==========================
// CONTROLLERS
// ==========================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ==========================
// HTTP PIPELINE
// ==========================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();

app.Run();