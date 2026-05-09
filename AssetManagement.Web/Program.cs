using System.Text;
using AssetManagement.Core.Interfaces;
using AssetManagement.Core.Services;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Infrastructure.Identity;
using AssetManagement.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── DATABASE ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── IDENTITY ───────────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── JWT AUTHENTICATION ─────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    // MVC uses cookie challenge (redirects to /Account/Login on 401)
    // API uses JWT bearer (returns 401 JSON on failure)
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = "MvcCookie";
    options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie("MvcCookie", options =>
{
    options.LoginPath  = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    // Cookie scheme itself does no token validation —
    // it only provides the redirect-to-login challenge behaviour.
    // The actual token is validated by JwtBearer below.
    options.Events.OnRedirectToLogin = ctx =>
    {
        // For API requests (Accept: application/json) keep 401, don't redirect
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = 401;
        }
        else
        {
            ctx.Response.Redirect(ctx.RedirectUri);
        }
        return Task.CompletedTask;
    };
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings["Issuer"],
        ValidAudience            = jwtSettings["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(key),
        ClockSkew                = TimeSpan.Zero
    };

    // Read JWT from cookie (MVC) or Authorization header (API / Flutter)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Token))
                context.Token = context.Request.Cookies["AuthToken"];
            return Task.CompletedTask;
        }
    };
});

// Inject WebRootPath so LocalFileStorageService (in Core) can access wwwroot
builder.Configuration["WebRootPath"] = builder.Environment.WebRootPath;

// ── SERVICES (DI) ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IFloorService, FloorService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// ── MVC + API CONTROLLERS ──────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── SWAGGER ────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Only include /api/* paths — hides all MVC controller routes from Swagger
    c.DocInclusionPredicate((_, apiDesc) =>
        apiDesc.RelativePath != null &&
        apiDesc.RelativePath.StartsWith("api/", StringComparison.OrdinalIgnoreCase));

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Asset Management API",
        Version = "v1",
        Description = """
            REST API for the Facility Asset Management Platform.

            ## Authentication
            All endpoints require a **Bearer JWT token** in the Authorization header.
            1. Call `POST /api/auth/login` with username + password.
            2. Copy the `token` from the response.
            3. Click **Authorize** above and enter: `Bearer <your_token>`

            ## Roles
            | Role | Permissions |
            |------|-------------|
            | `manager` | Full access |
            | `coordinator` | Create/close tickets, manage assignments |
            | `facilities` | Update ticket status, upload attachments |

            ## Demo Credentials
            | Username | Password | Role |
            |----------|----------|------|
            | manager1 | Pass@123 | manager |
            | coordinator1 | Pass@123 | coordinator |
            | facilities1 | Pass@123 | facilities |
            """
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Enter: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS (for Flutter on different port) ──────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ── SEED DATABASE ──────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DbInitializer.Initialize(context, userManager);
}

// ── MIDDLEWARE PIPELINE ────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Asset Management API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// MVC default route — goes to Dashboard (redirects to Login if not authenticated)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
