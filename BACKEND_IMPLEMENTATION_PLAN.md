# Asset Management Platform - Backend Implementation Plan

## Context

This plan outlines the complete implementation of an Asset Management Platform backend using ASP.NET Core with N-tier architecture. The system will support:

1. **MVC Dashboard** - Web-based interface for facilities management and administrators
2. **API Endpoints** - RESTful API for Flutter mobile app integration

Both components will coexist in a single MVC project, sharing business logic and data access layers.

**Authentication Approach:**
- **Unified JWT tokens** for both MVC and API
- **MVC:** JWT stored in HTTP-only cookies (secure, user-friendly)
- **API:** JWT sent in Authorization header (stateless, no cookies)
- **Identity Framework:** For user management and password hashing
- Both use same `IAuthService` and JWT validation logic

**Why this approach?**
- The spec document requires both web dashboard and mobile API
- N-tier architecture ensures separation of concerns and maintainability
- Single JWT system eliminates duplicate authentication code
- Cookies for MVC provide better security (HTTP-only, CSRF protection)
- Stateless API for mobile (no cookie dependency)
- Starting from scratch in `C:\Users\Kareem Usama\Desktop\Facility Mangement\`

---

## Architecture Overview

### N-Tier Project Structure

```
📦 AssetManagement Solution
│
├── 🎯 AssetManagement.Web (ASP.NET Core MVC - Main Project)
│   ├── Controllers/
│   │   ├── MVC/ (Dashboard - JWT in cookies)
│   │   │   ├── HomeController.cs
│   │   │   ├── AccountController.cs (Login/Logout)
│   │   │   ├── DashboardController.cs
│   │   │   ├── TicketsController.cs
│   │   │   └── AssetsController.cs
│   │   │
│   │   └── API/ (Flutter - JWT in header, no cookies)
│   │       ├── AuthController.cs
│   │       ├── AssetsController.cs
│   │       ├── TicketsController.cs
│   │       └── DashboardController.cs
│   │
│   ├── Views/ (Razor Pages for Dashboard)
│   ├── wwwroot/ (CSS, JS, Images)
│   ├── Areas/Identity/ (Identity Scaffolding)
│   ├── Uploads/ (Local file storage for attachments)
│   └── Program.cs
│
├── 📚 AssetManagement.Core (Business Logic Layer)
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── ITicketService.cs
│   │   ├── IAssetService.cs
│   │   ├── IDashboardService.cs
│   │   └── IFileStorageService.cs
│   │
│   └── Services/
│       ├── AuthService.cs
│       ├── TicketService.cs
│       ├── AssetService.cs
│       ├── DashboardService.cs
│       └── LocalFileStorageService.cs
│
├── 🗄️ AssetManagement.Infrastructure (Data Access Layer)
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── DbInitializer.cs (Seed data)
│   │   └── Migrations/
│   │
│   ├── Repositories/
│   │   ├── IRepository.cs (Generic)
│   │   ├── Repository.cs
│   │   ├── ITicketRepository.cs
│   │   ├── TicketRepository.cs
│   │   └── ... (other repositories)
│   │
│   └── Identity/
│       └── ApplicationUser.cs (extends IdentityUser)
│
└── 📋 AssetManagement.Models (Domain Models & DTOs)
    ├── Entities/ (Database Models)
    │   ├── User.cs (maps to ApplicationUser)
    │   ├── Building.cs
    │   ├── Floor.cs
    │   ├── Asset.cs
    │   ├── Vendor.cs
    │   ├── Ticket.cs
    │   ├── Attachment.cs
    │   └── TicketHistory.cs
    │
    ├── DTOs/ (Data Transfer Objects for API)
    │   ├── Auth/
    │   │   ├── LoginRequestDto.cs
    │   │   ├── LoginResponseDto.cs
    │   │   └── UserDto.cs
    │   │
    │   ├── Tickets/
    │   │   ├── CreateTicketDto.cs
    │   │   ├── UpdateTicketStatusDto.cs
    │   │   ├── CloseTicketDto.cs
    │   │   └── TicketDto.cs
    │   │
    │   ├── Assets/
    │   │   └── AssetDto.cs
    │   │
    │   └── Dashboard/
    │       ├── DashboardStatsDto.cs
    │       └── BuildingPerformanceDto.cs
    │
    └── ViewModels/ (For MVC Views)
        ├── DashboardViewModel.cs
        ├── TicketListViewModel.cs
        └── TicketDetailsViewModel.cs
```

---

## Technology Stack

- **Framework:** ASP.NET Core 9.0
- **Database:** SQL Server with Entity Framework Core 9.0
- **Authentication:**
  - **Both MVC & API:** JWT Bearer Tokens
  - **MVC:** JWT tokens stored in cookies (for better web UX)
  - **API:** JWT tokens in Authorization header (stateless, no cookies)
  - **Identity:** ASP.NET Core Identity 9.0 for user management (with custom table names)
- **File Storage:** Local file system (`wwwroot/uploads/`)
- **ORM:** Entity Framework Core 9.0
- **API Documentation:** Swagger/OpenAPI (for mobile dev reference)

---

## Database Schema

All tables will be created upfront in the initial migration:

### 1. Users (via Identity Framework - Custom Table Names)

**Note:** We will rename Identity Framework tables to remove the "AspNet" prefix.

```sql
Users (renamed from AspNetUsers)
├── Id (string/GUID)
├── UserName
├── Email
├── PasswordHash
├── PhoneNumber
└── ... (Identity fields)

-- Custom fields via ApplicationUser:
├── FullName (string, 100)
├── Role (string, 20) - coordinator, facilities, manager
├── OrganizationId (Guid, nullable)
└── IsActive (bool)

-- Other Identity Tables (also renamed):
Roles (from AspNetRoles)
UserRoles (from AspNetUserRoles)
UserClaims (from AspNetUserClaims)
UserLogins (from AspNetUserLogins)
UserTokens (from AspNetUserTokens)
RoleClaims (from AspNetRoleClaims)
```

### 2. Buildings
```sql
Buildings
├── Id (Guid, PK)
├── Name (string, 100)
├── Address (string, 500)
├── FloorsCount (int)
└── CreatedAt (DateTime)
```

### 3. Floors
```sql
Floors
├── Id (Guid, PK)
├── BuildingId (Guid, FK → Buildings)
├── Level (int)
├── Name (string, 50)
└── CreatedAt (DateTime)
```

### 4. Assets
```sql
Assets
├── Id (Guid, PK)
├── Name (string, 100)
├── QRCode (string, 100, UNIQUE, INDEXED)
├── SerialNumber (string, 100)
├── BuildingId (Guid, FK → Buildings)
├── FloorId (Guid, FK → Floors, nullable)
├── Status (string, 20) - active, in_repair, decommissioned
├── Criticality (string, 20) - safety, operational, low
├── LastServicedAt (DateTime, nullable)
├── NextServiceDueAt (DateTime, nullable)
└── CreatedAt (DateTime)
```

### 5. Vendors
```sql
Vendors
├── Id (Guid, PK)
├── Name (string, 100)
├── ContactName (string, 100)
├── ContactPhone (string, 20)
├── ContactEmail (string, 100)
└── CreatedAt (DateTime)
```

### 6. Tickets
```sql
Tickets
├── Id (Guid, PK)
├── TicketNumber (string, 20, UNIQUE) - auto-generated TKT-YYYY-####
├── AssetId (Guid, FK → Assets)
├── Title (string, 200)
├── Description (text)
├── Priority (string, 20) - critical, urgent, normal, low
├── Status (string, 20) - open, in_progress, closed, cancelled
├── SLAHours (int)
├── DueAt (DateTime)
├── CreatedByUserId (string, FK → AspNetUsers)
├── AssignedToUserId (string, FK → AspNetUsers, nullable)
├── AssignedVendorId (Guid, FK → Vendors, nullable)
├── ResolutionNotes (text, nullable)
├── ResolutionByUserId (string, FK → AspNetUsers, nullable)
├── ActualCost (decimal(10,2), nullable)
├── CreatedAt (DateTime)
└── ClosedAt (DateTime, nullable)

-- Indexes:
├── IX_Status
├── IX_CreatedByUserId
└── IX_DueAt
```

### 7. Attachments
```sql
Attachments
├── Id (Guid, PK)
├── TicketId (Guid, FK → Tickets)
├── Filename (string, 255)
├── Url (string, 500) - relative path
├── ContentType (string, 50)
├── SizeBytes (long)
├── UploadedByUserId (string, FK → AspNetUsers)
└── UploadedAt (DateTime)
```

### 8. TicketHistory
```sql
TicketHistory
├── Id (Guid, PK)
├── TicketId (Guid, FK → Tickets, INDEXED)
├── EventType (string, 50) - created, status_changed, assigned, closed
├── CreatedByUserId (string, FK → AspNetUsers)
├── Details (string/JSON, nullable) - stores old/new values
└── CreatedAt (DateTime)
```

---

## Implementation Phases

### PHASE 1: Project Setup & Infrastructure (Day 1)

#### 1.1 Create Solution & Projects
```bash
# Navigate to project directory
cd "C:\Users\Kareem Usama\Desktop\Facility Mangement"

# Create solution
dotnet new sln -n AssetManagement

# Create projects
dotnet new mvc -n AssetManagement.Web
dotnet new classlib -n AssetManagement.Core
dotnet new classlib -n AssetManagement.Infrastructure
dotnet new classlib -n AssetManagement.Models

# Add projects to solution
dotnet sln add AssetManagement.Web
dotnet sln add AssetManagement.Core
dotnet sln add AssetManagement.Infrastructure
dotnet sln add AssetManagement.Models
```

#### 1.2 Add Project References
```bash
# Web depends on Core and Models
cd AssetManagement.Web
dotnet add reference ../AssetManagement.Core
dotnet add reference ../AssetManagement.Models

# Core depends on Infrastructure and Models
cd ../AssetManagement.Core
dotnet add reference ../AssetManagement.Infrastructure
dotnet add reference ../AssetManagement.Models

# Infrastructure depends on Models
cd ../AssetManagement.Infrastructure
dotnet add reference ../AssetManagement.Models
```

#### 1.3 Install NuGet Packages

**AssetManagement.Web:**
```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
```

**AssetManagement.Infrastructure:**
```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

**AssetManagement.Core:**
```bash
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package Microsoft.AspNetCore.Http.Features
```

#### 1.4 Create Folder Structure

**AssetManagement.Models:**
- `Entities/` - Domain models
- `DTOs/Auth/` - Authentication DTOs
- `DTOs/Tickets/` - Ticket DTOs
- `DTOs/Assets/` - Asset DTOs
- `DTOs/Dashboard/` - Dashboard DTOs
- `ViewModels/` - MVC ViewModels
- `Enums/` - Shared enums (Priority, Status, etc.)

**AssetManagement.Infrastructure:**
- `Data/` - DbContext, Migrations
- `Repositories/` - Repository implementations
- `Identity/` - ApplicationUser

**AssetManagement.Core:**
- `Interfaces/` - Service interfaces
- `Services/` - Service implementations
- `Helpers/` - Utilities (TicketNumberGenerator, SLACalculator)

**AssetManagement.Web:**
- `Controllers/MVC/` - Dashboard controllers
- `Controllers/API/` - API controllers
- `Views/` - Razor views
- `wwwroot/uploads/tickets/` - File uploads
- `wwwroot/css/`, `wwwroot/js/` - Static assets

---

### PHASE 2: Models & Entities (Day 1)

#### 2.1 Create Entity Models (in AssetManagement.Models/Entities/)

**ApplicationUser.cs** (in Infrastructure/Identity/):
```csharp
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // coordinator, facilities, manager
    public Guid? OrganizationId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**Building.cs, Floor.cs, Asset.cs, Vendor.cs, Ticket.cs, Attachment.cs, TicketHistory.cs** - All entity models with proper navigation properties and data annotations.

#### 2.2 Create Enums (in AssetManagement.Models/Enums/)
- `TicketPriority.cs` - Critical, Urgent, Normal, Low
- `TicketStatus.cs` - Open, InProgress, Closed, Cancelled
- `AssetStatus.cs` - Active, InRepair, Decommissioned
- `AssetCriticality.cs` - Safety, Operational, Low
- `HistoryEventType.cs` - Created, StatusChanged, Assigned, Closed

#### 2.3 Create DTOs

**Auth DTOs:**
- `LoginRequestDto` - username, password
- `LoginResponseDto` - token, user info
- `UserDto` - user details for API response

**Ticket DTOs:**
- `CreateTicketDto` - ticket creation payload
- `UpdateTicketStatusDto` - status update payload
- `CloseTicketDto` - ticket closure payload
- `TicketDto` - ticket response with denormalized data
- `TicketDetailsDto` - full ticket with history and attachments

**Asset DTOs:**
- `AssetDto` - asset details response

**Dashboard DTOs:**
- `DashboardStatsDto` - KPI stats
- `BuildingPerformanceDto` - building metrics

---

### PHASE 3: Data Layer (Day 1)

#### 3.1 Create ApplicationDbContext

**ApplicationDbContext.cs** (in Infrastructure/Data/):
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Building> Buildings { get; set; }
    public DbSet<Floor> Floors { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<TicketHistory> TicketHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // RENAME IDENTITY TABLES (remove AspNet prefix)
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        // Configure relationships, indexes, constraints
        // - Unique index on Asset.QRCode
        // - Unique index on Ticket.TicketNumber
        // - Cascade delete rules
        // - Decimal precision for ActualCost
        // - Default values for timestamps

        // Seed initial data
    }
}
```

**Key configurations:**
- **Custom table names** for Identity (Users, Roles, etc. instead of AspNetUsers, AspNetRoles, etc.)
- Unique index on `Asset.QRCode`
- Unique index on `Ticket.TicketNumber`
- Cascade delete rules
- Decimal precision for `ActualCost`
- Default values for timestamps

#### 3.2 Create Repository Pattern

**IRepository.cs** (Generic repository interface):
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
}
```

**Specific repositories:**
- `ITicketRepository` - Custom queries (filter by status, priority, user, etc.)
- `IAssetRepository` - QR code lookup, building filters
- Implement these in Infrastructure/Repositories/

#### 3.3 Configure Connection String

In `AssetManagement.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AssetManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-min-32-chars",
    "Issuer": "AssetManagementAPI",
    "Audience": "AssetManagementMobile",
    "ExpirationHours": 24
  }
}
```

#### 3.4 Create Initial Migration

```bash
cd AssetManagement.Web
dotnet ef migrations add InitialCreate --project ../AssetManagement.Infrastructure
dotnet ef database update
```

#### 3.5 Create Seed Data (DbInitializer.cs)

Create seed data for:
- **5 Users** (coordinator1, coordinator2, facilities1, facilities2, manager1) - all password: `demo123`
- **2 Buildings** (Main Campus, Annex Building)
- **Floors** for each building
- **15 Assets** with QR codes (ASSET-MC-ELEV-01, etc.)
- **3 Vendors** (ACME Elevators, CoolAir HVAC, TechFix Services)

Call `DbInitializer.Initialize(context, userManager)` in `Program.cs` on app startup.

---

### PHASE 4: Business Logic Layer (Day 1-2)

#### 4.1 Authentication Service

**IAuthService.cs:**
```csharp
Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
Task<UserDto> GetCurrentUserAsync(string userId);
string GenerateJwtToken(ApplicationUser user);
```

**AuthService.cs:**
- Validate credentials using `UserManager<ApplicationUser>`
- Generate JWT tokens with user claims (userId, role, email)
- Return user info

#### 4.2 Ticket Service

**ITicketService.cs:**
```csharp
Task<TicketDetailsDto> CreateTicketAsync(CreateTicketDto dto, string userId);
Task<TicketDetailsDto> GetTicketByIdAsync(Guid id);
Task<PagedResult<TicketDto>> GetTicketsAsync(TicketFilterDto filter);
Task<TicketDetailsDto> UpdateStatusAsync(Guid id, UpdateTicketStatusDto dto, string userId);
Task<TicketDetailsDto> CloseTicketAsync(Guid id, CloseTicketDto dto, string userId);
Task<AttachmentDto> AddAttachmentAsync(Guid ticketId, IFormFile file, string userId);
```

**TicketService.cs - Key Logic:**

1. **CreateTicket:**
   - Generate unique ticket number (TKT-YYYY-0001)
   - Calculate SLA based on priority:
     - Critical → 4 hours
     - Urgent → 12 hours
     - Normal → 24 hours
     - Low → 48 hours
   - Set `DueAt = CreatedAt + SLAHours`
   - Create history entry (EventType: Created)
   - Handle attachments

2. **UpdateStatus:**
   - Validate status transitions (open → in_progress → closed)
   - Create history entry for status change
   - If assignedToUserId changed, create "Assigned" history entry

3. **CloseTicket:**
   - Set status = Closed
   - Calculate `TimeSpentMinutes = (ClosedAt - CreatedAt)`
   - Create "Closed" history entry

#### 4.3 Asset Service

**IAssetService.cs:**
```csharp
Task<AssetDto> GetAssetByQRCodeAsync(string qrCode);
Task<IEnumerable<AssetDto>> GetAssetsByBuildingAsync(Guid buildingId);
```

**AssetService.cs:**
- Case-insensitive QR code lookup
- Include denormalized building/floor names in response

#### 4.4 Dashboard Service

**IDashboardService.cs:**
```csharp
Task<DashboardStatsDto> GetDashboardStatsAsync(Guid? buildingId = null);
Task<IEnumerable<BuildingPerformanceDto>> GetBuildingPerformanceAsync();
```

**DashboardService.cs - Calculations:**
- Total assets count
- Open tickets count (status != closed)
- Critical tickets count (priority = critical && status != closed)
- Average resolution time (from closed tickets)
- Tickets by priority breakdown
- Tickets by status breakdown
- Building performance metrics

#### 4.5 File Storage Service

**IFileStorageService.cs:**
```csharp
Task<string> SaveFileAsync(IFormFile file, string folder);
Task<bool> DeleteFileAsync(string fileUrl);
bool IsImageFile(string contentType);
```

**LocalFileStorageService.cs:**
- Save files to `wwwroot/uploads/tickets/{ticketId}/`
- Generate unique filenames (avoid collisions)
- Validate file types (jpg, png, heic only)
- Validate file size (max 10MB)
- Return relative URL for database storage

---

### PHASE 5: API Controllers (Day 2)

All API controllers in `Controllers/API/` with `[Route("api/[controller]")]` and `[Authorize]`

**Note:** Since we're using JWT for both MVC and API, we don't need to specify `AuthenticationSchemes`. The API will receive tokens via `Authorization: Bearer {token}` header, while MVC receives tokens via cookie - both validated the same way.

#### 5.1 AuthController.cs
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    [HttpPost("login")] // POST /api/auth/login
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(new { message = "Invalid credentials" });

        // For API: Return token in response body (NOT in cookie)
        return Ok(result);
    }

    [HttpGet("me")] // GET /api/auth/me
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _authService.GetCurrentUserAsync(userId);
        return Ok(user);
    }
}
```

**Important:** API login returns token in JSON response. Mobile app must include it in `Authorization: Bearer {token}` header for subsequent requests.

#### 5.2 AssetsController.cs
```csharp
[HttpGet("{qrCode}")] // GET /api/assets/ASSET-MC-ELEV-01
public async Task<IActionResult> GetAssetByQRCode(string qrCode)
```

#### 5.3 TicketsController.cs
```csharp
[HttpPost] // POST /api/tickets
public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)

[HttpGet] // GET /api/tickets?status=open&priority=critical&createdByUserId=...
public async Task<IActionResult> GetTickets([FromQuery] TicketFilterDto filter)

[HttpGet("{id}")] // GET /api/tickets/{id}
public async Task<IActionResult> GetTicketById(Guid id)

[HttpPut("{id}/status")] // PUT /api/tickets/{id}/status
public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTicketStatusDto dto)

[HttpPut("{id}/close")] // PUT /api/tickets/{id}/close
public async Task<IActionResult> CloseTicket(Guid id, [FromBody] CloseTicketDto dto)

[HttpPost("{id}/attachments")] // POST /api/tickets/{id}/attachments
public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file)
```

#### 5.4 DashboardController.cs
```csharp
[HttpGet("stats")] // GET /api/dashboard/stats?buildingId=...
public async Task<IActionResult> GetStats([FromQuery] Guid? buildingId)

[HttpGet("buildings/performance")] // GET /api/dashboard/buildings/performance
public async Task<IActionResult> GetBuildingPerformance()
```

#### 5.5 BuildingsController.cs
```csharp
[HttpGet] // GET /api/buildings
public async Task<IActionResult> GetBuildings()
```

#### 5.6 VendorsController.cs
```csharp
[HttpGet] // GET /api/vendors
public async Task<IActionResult> GetVendors()
```

**API Response Format:**
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful",
  "errors": []
}
```

Or for errors:
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": ["Field X is required", "..."]
}
```

---

### PHASE 6: Authentication Configuration (Day 2)

#### Authentication Flow Overview

**Single JWT System for Both MVC and API:**

```
┌─────────────────────────────────────────────────────────────┐
│                    AUTHENTICATION FLOW                       │
└─────────────────────────────────────────────────────────────┘

API CLIENT (Flutter Mobile):
1. POST /api/auth/login { username, password }
2. ← Response: { "token": "eyJhbGc...", "user": {...} }
3. Store token in secure storage
4. Include in header: Authorization: Bearer eyJhbGc...
5. Backend reads from Authorization header ✓

MVC CLIENT (Web Dashboard):
1. POST /Account/Login (form submit)
2. AuthService.LoginAsync() generates JWT token
3. Store token in HTTP-only cookie: "AuthToken"
4. Cookie automatically sent with each request
5. Backend reads from cookie ✓

SHARED VALIDATION:
- Both use same JWT validation parameters
- Both validate against same secret key
- Both extract same user claims (userId, role, email)
- Same AuthService.LoginAsync() method
- Same token expiration (24 hours)
```

**Key Differences:**

| Aspect | API (Flutter) | MVC (Web Dashboard) |
|--------|--------------|---------------------|
| **Token Delivery** | JSON response body | HTTP-only cookie |
| **Token Storage** | App secure storage | Browser cookie |
| **Token Transmission** | Authorization header | Cookie (automatic) |
| **Login Endpoint** | `POST /api/auth/login` | `POST /Account/Login` |
| **Logout** | Delete from storage | Delete cookie |
| **Security** | Bearer token | HTTP-only + Secure + SameSite |

**Advantages:**
- ✅ Single source of truth (one `IAuthService`)
- ✅ Same JWT validation logic
- ✅ API stays stateless (no cookies)
- ✅ MVC benefits from cookie security (CSRF protection)
- ✅ No duplicate code

---

#### 6.1 Configure Program.cs

```csharp
// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false; // Simplify for demo
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

builder.Services.AddAuthentication(options =>
{
    // Default to JWT for both MVC and API
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Remove default 5 min tolerance
    };

    // For MVC: Read JWT from cookie instead of header
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // First check Authorization header (API clients)
            if (string.IsNullOrEmpty(context.Token))
            {
                // Then check cookie (MVC clients)
                context.Token = context.Request.Cookies["AuthToken"];
            }
            return Task.CompletedTask;
        }
    };
});

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Asset Management API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DbInitializer.Initialize(context, userManager);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
```

---

### PHASE 7: MVC Dashboard (Day 3-4)

#### 7.1 Account Controller (MVC)

**Controllers/MVC/AccountController.cs:**
```csharp
[Route("Account")]
public class AccountController : Controller
{
    private readonly IAuthService _authService;

    [HttpGet("Login")]
    public IActionResult Login(string returnUrl = null)

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        // Validate credentials using AuthService (same as API)
        var result = await _authService.LoginAsync(new LoginRequestDto
        {
            Username = model.Username,
            Password = model.Password
        });

        if (result != null)
        {
            // Store JWT token in HTTP-only cookie
            Response.Cookies.Append("AuthToken", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // HTTPS only in production
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(24)
            });

            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError("", "Invalid login attempt");
        return View(model);
    }

    [HttpPost("Logout")]
    public IActionResult Logout()
    {
        // Delete the auth cookie
        Response.Cookies.Delete("AuthToken");
        return RedirectToAction("Login");
    }
}
```

**Key Points:**
- MVC uses the same `IAuthService.LoginAsync()` as API
- JWT token stored in HTTP-only cookie (secure, not accessible via JavaScript)
- API clients send token in `Authorization: Bearer {token}` header
- MVC clients automatically send token via cookie
- No duplicate authentication logic - same JWT validation for both

**Views/Account/Login.cshtml** - Bootstrap login form

#### 7.2 Dashboard Controller (MVC)

**Controllers/MVC/DashboardController.cs:**
```csharp
[Authorize] // Cookie auth
public class DashboardController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(Guid? buildingId)
    // Display KPI cards, charts, recent tickets

    [HttpGet]
    public async Task<IActionResult> BuildingPerformance()
}
```

**Views/Dashboard/Index.cshtml:**
- 4 KPI cards (Total Assets, Open Tickets, Critical Tickets, Avg Resolution Time)
- 2 charts (Tickets by Priority, Tickets by Status)
- Recent tickets table
- Use Chart.js or similar for visualizations

#### 7.3 Tickets Controller (MVC)

**Controllers/MVC/TicketsController.cs:**
```csharp
[Authorize]
public class TicketsController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(TicketFilterDto filter)
    // List all tickets with filters

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(Guid id)
    // Show ticket details, history, attachments

    [HttpPost("{id}/UpdateStatus")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateTicketStatusDto dto)

    [HttpPost("{id}/Close")]
    public async Task<IActionResult> Close(Guid id, CloseTicketDto dto)
}
```

**Views:**
- `Index.cshtml` - Tickets list with filters (Bootstrap table, DataTables optional)
- `Details.cshtml` - Ticket details page with timeline
- Partial views for status update modal, close modal

#### 7.4 Assets Controller (MVC)

**Controllers/MVC/AssetsController.cs:**
```csharp
[Authorize]
public class AssetsController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(Guid? buildingId)

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(Guid id)
}
```

**Views:**
- `Index.cshtml` - Assets list grouped by building
- `Details.cshtml` - Asset details with QR code display

#### 7.5 Layout & Navigation

**Views/Shared/_Layout.cshtml:**
- Bootstrap 5 navbar
- Navigation links (Dashboard, Tickets, Assets)
- User info dropdown with logout
- Responsive design

**wwwroot/css/site.css:**
- Custom styles
- Priority/status badge colors
- KPI card styling

---

### PHASE 8: Testing & Swagger Documentation (Day 4-5)

#### 8.1 Swagger Configuration

Configure Swagger to document all API endpoints with:
- Request/response examples
- Authentication requirements
- Parameter descriptions

Access at: `https://localhost:5001/swagger`

#### 8.2 Test API Endpoints

Use Swagger UI or Postman to test:

1. **Auth Flow:**
   - POST /api/auth/login → Get JWT token
   - GET /api/auth/me → Verify token works

2. **Asset Scanning:**
   - GET /api/assets/ASSET-MC-ELEV-01 → Should return elevator details

3. **Ticket Creation:**
   - POST /api/tickets → Create ticket with attachment
   - Verify ticket appears in database

4. **Status Updates:**
   - PUT /api/tickets/{id}/status → Change to in_progress
   - Verify history entry created

5. **Ticket Closure:**
   - PUT /api/tickets/{id}/close → Close ticket
   - Verify timeSpentMinutes calculated

6. **Dashboard:**
   - GET /api/dashboard/stats → Verify KPIs
   - GET /api/dashboard/buildings/performance → Verify metrics

#### 8.3 Test MVC Dashboard

1. Login with `facilities1` / `demo123`
2. View dashboard with KPIs
3. Navigate to tickets list
4. Filter by status, priority
5. View ticket details
6. Update ticket status
7. Close ticket
8. View updated analytics

#### 8.4 Create Demo Accounts

Ensure seed data includes:
- `coordinator1` / `demo123` (Role: coordinator)
- `facilities1` / `demo123` (Role: facilities)
- `manager1` / `demo123` (Role: manager)

#### 8.5 Generate QR Codes

Use online QR generator or C# library to create QR codes for:
- ASSET-MC-ELEV-01 (Main Campus Elevator)
- ASSET-MC-AC-01 (Main Campus AC)
- ASSET-AB-ELEV-01 (Annex Building Elevator)
- (Total 15 assets)

Print and label for demo.

---

## File Upload Implementation Details

### LocalFileStorageService.cs

```csharp
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".heic" };

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        // Validate file
        if (!IsImageFile(file.ContentType))
            throw new InvalidOperationException("Only image files are allowed");

        if (file.Length > MaxFileSize)
            throw new InvalidOperationException("File size exceeds 10MB limit");

        // Create folder if not exists
        var uploadPath = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadPath);

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadPath, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative URL
        return $"/uploads/{folder}/{fileName}";
    }

    public bool IsImageFile(string contentType)
    {
        return contentType.StartsWith("image/");
    }
}
```

**Folder structure:**
```
wwwroot/
└── uploads/
    └── tickets/
        ├── {ticket-id-1}/
        │   ├── abc123.jpg
        │   └── def456.png
        └── {ticket-id-2}/
            └── ghi789.jpg
```

---

## Critical Implementation Notes

### 1. Ticket Number Generation

Create a helper class:
```csharp
public static class TicketNumberGenerator
{
    public static async Task<string> GenerateAsync(ApplicationDbContext context)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"TKT-{year}-";

        var lastTicket = await context.Tickets
            .Where(t => t.TicketNumber.StartsWith(prefix))
            .OrderByDescending(t => t.TicketNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastTicket != null)
        {
            var numberPart = lastTicket.TicketNumber.Substring(prefix.Length);
            nextNumber = int.Parse(numberPart) + 1;
        }

        return $"{prefix}{nextNumber:D4}"; // TKT-2026-0001
    }
}
```

### 2. SLA Calculation

```csharp
public static class SLACalculator
{
    public static int GetSLAHours(TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Critical => 4,
            TicketPriority.Urgent => 12,
            TicketPriority.Normal => 24,
            TicketPriority.Low => 48,
            _ => 24
        };
    }

    public static DateTime CalculateDueDate(DateTime createdAt, int slaHours)
    {
        return createdAt.AddHours(slaHours);
    }
}
```

### 3. History Logging Pattern

Every time a ticket changes, create a history entry:
```csharp
private async Task CreateHistoryEntry(Guid ticketId, HistoryEventType eventType, string userId, object? details = null)
{
    var history = new TicketHistory
    {
        Id = Guid.NewGuid(),
        TicketId = ticketId,
        EventType = eventType.ToString(),
        CreatedByUserId = userId,
        Details = details != null ? JsonSerializer.Serialize(details) : null,
        CreatedAt = DateTime.UtcNow
    };

    await _context.TicketHistory.AddAsync(history);
}
```

### 4. Status Transition Validation

```csharp
private bool IsValidStatusTransition(TicketStatus current, TicketStatus target)
{
    var validTransitions = new Dictionary<TicketStatus, List<TicketStatus>>
    {
        { TicketStatus.Open, new() { TicketStatus.InProgress, TicketStatus.Cancelled } },
        { TicketStatus.InProgress, new() { TicketStatus.Closed, TicketStatus.Open } },
        { TicketStatus.Closed, new() { } }, // Cannot reopen
        { TicketStatus.Cancelled, new() { } }
    };

    return validTransitions[current].Contains(target);
}
```

### 5. Authorization Checks

In API controllers:
```csharp
// Only coordinators can close tickets they created
if (User.FindFirst("role")?.Value != "coordinator")
    return Forbid("Only coordinators can close tickets");

// Only facilities can update status
if (User.FindFirst("role")?.Value != "facilities")
    return Forbid("Only facilities team can update ticket status");
```

---

## Handoff to Mobile Developer

### API Documentation

Once backend is ready, provide mobile developer with:

1. **Swagger URL:** `https://your-server.com/swagger`
2. **Base URL:** `https://your-server.com/api`
3. **Test Accounts:**
   - coordinator1 / demo123
   - facilities1 / demo123

### Sample API Calls

**Login:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "coordinator1",
  "password": "demo123"
}

Response:
{
  "token": "eyJhbGc...",
  "user": {
    "id": "...",
    "username": "coordinator1",
    "fullName": "Ahmed Coordinator",
    "role": "coordinator"
  }
}
```

**Get Asset by QR:**
```http
GET /api/assets/ASSET-MC-ELEV-01
Authorization: Bearer {token}

Response:
{
  "id": "...",
  "name": "Elevator A",
  "buildingName": "Main Campus",
  "floorLevel": 2,
  "criticality": "safety",
  "qrCode": "ASSET-MC-ELEV-01",
  "serialNumber": "ELEV-2020-01"
}
```

**Create Ticket:**
```http
POST /api/tickets
Authorization: Bearer {token}
Content-Type: application/json

{
  "assetId": "...",
  "title": "Elevator stuck",
  "description": "Stopped at floor 2",
  "priority": "critical",
  "attachments": [
    {
      "filename": "photo.jpg",
      "url": "/uploads/tickets/...",
      "contentType": "image/jpeg",
      "sizeBytes": 234567
    }
  ]
}
```

**File Upload:**
```http
POST /api/tickets/{id}/attachments
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [binary data]
```

---

## Testing Checklist

### Backend API Tests

- [ ] Login with valid credentials returns token
- [ ] Login with invalid credentials returns 401
- [ ] GET /api/auth/me with valid token returns user
- [ ] GET /api/auth/me without token returns 401
- [ ] GET /api/assets/{qrCode} returns correct asset
- [ ] POST /api/tickets creates ticket with correct ticket number
- [ ] Ticket SLA calculated correctly based on priority
- [ ] File upload saves to correct folder
- [ ] File upload rejects non-image files
- [ ] File upload rejects files > 10MB
- [ ] GET /api/tickets filters by createdByUserId correctly
- [ ] PUT /api/tickets/{id}/status updates status and creates history
- [ ] Invalid status transition returns error
- [ ] PUT /api/tickets/{id}/close calculates timeSpentMinutes
- [ ] Dashboard stats calculation is accurate
- [ ] Building performance metrics are correct

### MVC Dashboard Tests

- [ ] Login page displays correctly
- [ ] Login with valid credentials redirects to dashboard
- [ ] Dashboard shows correct KPI values
- [ ] Tickets list displays with filters
- [ ] Ticket details page shows full information
- [ ] Status update modal works
- [ ] Close ticket modal works
- [ ] Only facilities can see status update button
- [ ] Only coordinators can close their own tickets
- [ ] Logout clears session

### Integration Tests

- [ ] Create ticket via API → Appears in MVC dashboard
- [ ] Update status in MVC → Changes reflected in API
- [ ] Upload attachment via API → Viewable in MVC
- [ ] Close ticket in MVC → Status updated in API

---

## Deployment Preparation

### 1. Environment Configuration

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=production-server;Database=AssetManagementDb;..."
  },
  "JwtSettings": {
    "Secret": "production-secret-key-very-secure-32-chars-minimum",
    "Issuer": "AssetManagementAPI",
    "Audience": "AssetManagementMobile",
    "ExpirationHours": 24
  }
}
```

### 2. Database Migration

```bash
dotnet ef database update --project AssetManagement.Infrastructure --startup-project AssetManagement.Web
```

### 3. Publish

```bash
dotnet publish -c Release -o ./publish
```

### 4. IIS / Azure App Service Setup

- Configure app pool (.NET Core)
- Set environment variables
- Configure CORS if needed for mobile app

---

## Success Criteria

### Backend API
✅ All 13 API endpoints functional
✅ JWT authentication working
✅ File upload working
✅ Database seeded with demo data
✅ Swagger documentation accessible
✅ All business logic (SLA, ticket numbering, history) working

### MVC Dashboard
✅ Login/logout working
✅ Dashboard displays correct metrics
✅ Tickets CRUD operations working
✅ Role-based access control working
✅ Responsive design (Bootstrap)

### Ready for Mobile Integration
✅ API documentation shared
✅ Test accounts created
✅ QR codes generated and labeled
✅ Demo script prepared
✅ Backend deployed to accessible server

---

## Timeline Summary

**Day 1:** Project setup, models, database, seed data
**Day 2:** Business logic, API controllers, authentication
**Day 3:** MVC dashboard (login, dashboard, tickets list)
**Day 4:** MVC details pages, polish, testing
**Day 5:** Integration testing, deployment, documentation

---

## Reference Files Location

All code will be created in:
```
C:\Users\Kareem Usama\Desktop\Facility Mangement\
└── AssetManagement\ (solution folder)
```

This plan follows the specifications in `DEMO_SPEC_PARALLEL_DEV.md` and implements the N-tier architecture with dual authentication (Identity + JWT) as required.
