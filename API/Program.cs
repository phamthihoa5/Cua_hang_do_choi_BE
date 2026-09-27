using API.Hubs;
using API.IoC;
using API.Services;
using Application.IService;
using Application;
using Application.Common.Models;
using Application.MappingProfiles;
using Application.Model.API;
using Core.Entities.Identity;
using DataAccess;
using Microsoft.AspNetCore.Authentication.Cookies;  // Thêm import này để dùng ConfigureApplicationCookie
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
IdentityModelEventSource.ShowPII = true;

// ===== Add Database =====
builder.Services.AddDatabaseConfiguration(builder.Configuration);

// ===== Add Application Layer =====
builder.Services.AddApplicationConfiguration(builder.Configuration);

// ===== Add AutoMapper =====
builder.Services.AddAutoMapper(typeof(IMappingProfilesMarker));

// ===== Add Identity =====
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

// ===== Identity Options =====
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;
});

// ===== Configure Application Cookie (Fix redirect cho API) =====
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        // Nếu request là API path, trả 401 JSON thay vì redirect
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 401;  // Unauthorized
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{\"message\": \"Unauthorized. Please login first.\"}");
        }

        // Giữ redirect cho non-API paths (như /Account/Login)
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    // Optional: Đảm bảo cookie secure cho HTTPS (Render dùng HTTPS)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Load AllowedOrigins từ config
var allowedOrigins = builder.Configuration
    .GetSection("ServerSettings:AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins!)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ===== Add JWT Authentication =====
builder.Services.AddJwt(builder.Configuration);

// ===== Add Shared Configuration =====
builder.Services.AddSharedConfiguration();
// ===== Add Controllers =====
builder.Services.AddControllers();
// ===== Add SignalR & Notifier =====
builder.Services.AddSignalR();
builder.Services.AddScoped<IOrderNotifier, OrderNotifier>();

// ===== Add Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Hệ Thống Cửa hàng đồ chơi", Version = "v1" });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    c.ParameterFilter<SortsQueryParameterFilter>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo dạng: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ===== Middleware Pipeline =====
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    c.RoutePrefix = string.Empty;
});

app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowFrontend");

// ===== Authentication & Authorization =====
app.UseAuthentication();
app.UseAuthorization();

// ===== Map Controllers & Hubs =====
app.MapControllers();
app.MapHub<OrderHub>("/hubs/order");

// ===== Root endpoint =====
app.MapGet("/", () => "🚀 Toy Store API is running! Truy cập /swagger để test API");

// ===== Seed Database (Roles, Admin mặc định) =====
using (var scope = app.Services.CreateScope())
{
    await MigrateAutomation.MigrateAsync(scope.ServiceProvider);
}

app.Run();