using AssetManagement.Infrastructure.Identity;
using AssetManagement.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        await context.Database.MigrateAsync();

        if (await context.Buildings.AnyAsync()) return;

        // ── BUILDINGS ──────────────────────────────────────────────────────
        var mainCampus = new Building
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            Name = "Main Campus",
            Address = "123 University Avenue, Cairo",
            FloorsCount = 3
        };
        var annexBuilding = new Building
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
            Name = "Annex Building",
            Address = "456 Education Street, Cairo",
            FloorsCount = 2
        };
        context.Buildings.AddRange(mainCampus, annexBuilding);
        await context.SaveChangesAsync();

        // ── FLOORS ─────────────────────────────────────────────────────────
        var mcFloor1 = new Floor { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), BuildingId = mainCampus.Id,    Level = 1, Name = "Ground Floor" };
        var mcFloor2 = new Floor { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), BuildingId = mainCampus.Id,    Level = 2, Name = "First Floor"  };
        var mcFloor3 = new Floor { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), BuildingId = mainCampus.Id,    Level = 3, Name = "Second Floor" };
        var abFloor1 = new Floor { Id = Guid.Parse("20000000-0000-0000-0000-000000000004"), BuildingId = annexBuilding.Id, Level = 1, Name = "Ground Floor" };
        var abFloor2 = new Floor { Id = Guid.Parse("20000000-0000-0000-0000-000000000005"), BuildingId = annexBuilding.Id, Level = 2, Name = "First Floor"  };
        context.Floors.AddRange(mcFloor1, mcFloor2, mcFloor3, abFloor1, abFloor2);
        await context.SaveChangesAsync();

        // ── ASSETS (15 total) ──────────────────────────────────────────────
        var assets = new List<Asset>
        {
            // Main Campus — Elevators (safety)
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), Name = "Elevator A",          QRCode = "ASSET-MC-ELEV-01",   SerialNumber = "ELEV-2020-01", BuildingId = mainCampus.Id,    FloorId = mcFloor1.Id, Criticality = "safety",      Status = "operational",       LastServicedAt = new DateTime(2026,1,15),  NextServiceDueAt = new DateTime(2026,7,15)  },
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000002"), Name = "Elevator B",          QRCode = "ASSET-MC-ELEV-02",   SerialNumber = "ELEV-2021-01", BuildingId = mainCampus.Id,    FloorId = mcFloor2.Id, Criticality = "safety",      Status = "operational",       LastServicedAt = new DateTime(2026,1,20),  NextServiceDueAt = new DateTime(2026,7,20)  },
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000003"), Name = "Elevator C",          QRCode = "ASSET-MC-ELEV-03",   SerialNumber = "ELEV-2022-01", BuildingId = mainCampus.Id,    FloorId = mcFloor3.Id, Criticality = "safety",      Status = "under_maintenance", LastServicedAt = new DateTime(2026,2,1),   NextServiceDueAt = new DateTime(2026,8,1)   },
            // Main Campus — Air Conditioners (operational)
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000004"), Name = "AC Unit 1",           QRCode = "ASSET-MC-AC-01",     SerialNumber = "AC-2021-01",   BuildingId = mainCampus.Id,    FloorId = mcFloor1.Id, Criticality = "operational", Status = "operational",       LastServicedAt = new DateTime(2025,12,10)                                              },
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000005"), Name = "AC Unit 2",           QRCode = "ASSET-MC-AC-02",     SerialNumber = "AC-2021-02",   BuildingId = mainCampus.Id,    FloorId = mcFloor2.Id, Criticality = "operational", Status = "operational",       LastServicedAt = new DateTime(2025,12,10)                                              },
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000006"), Name = "AC Unit 3",           QRCode = "ASSET-MC-AC-03",     SerialNumber = "AC-2022-01",   BuildingId = mainCampus.Id,    FloorId = mcFloor3.Id, Criticality = "operational", Status = "under_maintenance", LastServicedAt = new DateTime(2025,12,15)                                              },
            // Main Campus — Printers (low)
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000007"), Name = "Printer 1",           QRCode = "ASSET-MC-PRINT-01",  SerialNumber = "PRN-2022-01",  BuildingId = mainCampus.Id,    FloorId = mcFloor1.Id, Criticality = "low",         Status = "operational"                                                                                         },
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000008"), Name = "Printer 2",           QRCode = "ASSET-MC-PRINT-02",  SerialNumber = "PRN-2022-02",  BuildingId = mainCampus.Id,    FloorId = mcFloor2.Id, Criticality = "low",         Status = "operational"                                                                                         },
            // Main Campus — Fire Extinguisher (safety)
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000009"), Name = "Fire Extinguisher 1", QRCode = "ASSET-MC-FIRE-01",   SerialNumber = "FIRE-2023-01", BuildingId = mainCampus.Id,    FloorId = mcFloor1.Id, Criticality = "safety",      Status = "operational",       LastServicedAt = new DateTime(2025,11,1),  NextServiceDueAt = new DateTime(2026,11,1)  },
            // Main Campus — Water Dispenser (low)
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000010"), Name = "Water Dispenser 1",   QRCode = "ASSET-MC-WATER-01",  SerialNumber = "WTR-2023-01",  BuildingId = mainCampus.Id,    FloorId = mcFloor2.Id, Criticality = "low",         Status = "operational"                                                                                         },
            // Annex Building — Elevator (safety)
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000011"), Name = "Elevator D",          QRCode = "ASSET-AB-ELEV-01",   SerialNumber = "ELEV-2021-02", BuildingId = annexBuilding.Id, FloorId = abFloor1.Id, Criticality = "safety",      Status = "operational",       LastServicedAt = new DateTime(2026,1,10),  NextServiceDueAt = new DateTime(2026,7,10)  },
            // Annex Building — AC (operational)
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000012"), Name = "AC Unit 4",           QRCode = "ASSET-AB-AC-01",     SerialNumber = "AC-2022-03",   BuildingId = annexBuilding.Id, FloorId = abFloor1.Id, Criticality = "operational", Status = "operational"                                                                                         },
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000013"), Name = "AC Unit 5",           QRCode = "ASSET-AB-AC-02",     SerialNumber = "AC-2022-04",   BuildingId = annexBuilding.Id, FloorId = abFloor2.Id, Criticality = "operational", Status = "operational"                                                                                         },
            // Annex Building — Printer (low)
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000014"), Name = "Printer 3",           QRCode = "ASSET-AB-PRINT-01",  SerialNumber = "PRN-2023-01",  BuildingId = annexBuilding.Id, FloorId = abFloor1.Id, Criticality = "low",         Status = "operational"                                                                                         },
            // Annex Building — Projector (low)
            new Asset { Id = Guid.Parse("30000000-0000-0000-0000-000000000015"), Name = "Projector 1",         QRCode = "ASSET-AB-PROJ-01",   SerialNumber = "PROJ-2023-01", BuildingId = annexBuilding.Id, FloorId = abFloor2.Id, Criticality = "low",         Status = "operational"                                                                                         },
        };
        context.Assets.AddRange(assets);
        await context.SaveChangesAsync();

        // ── VENDORS ────────────────────────────────────────────────────────
        var vendorAcme    = new Vendor { Id = Guid.Parse("40000000-0000-0000-0000-000000000001"), Name = "ACME Elevators",  ContactName = "John Smith",  ContactPhone = "+20123456789", ContactEmail = "john@acme-elevators.com" };
        var vendorCoolair = new Vendor { Id = Guid.Parse("40000000-0000-0000-0000-000000000002"), Name = "CoolAir HVAC",    ContactName = "Sara Ahmed",  ContactPhone = "+20123456790", ContactEmail = "sara@coolair.com"         };
        var vendorTechfix = new Vendor { Id = Guid.Parse("40000000-0000-0000-0000-000000000003"), Name = "TechFix Services", ContactName = "Omar Hassan", ContactPhone = "+20123456791", ContactEmail = "omar@techfix.com"         };
        context.Vendors.AddRange(vendorAcme, vendorCoolair, vendorTechfix);
        await context.SaveChangesAsync();

        // ── USERS ──────────────────────────────────────────────────────────
        var usersToCreate = new[]
        {
            new { Username = "coordinator1", FullName = "Ahmed Coordinator",  Email = "ahmed@demo.com",   Role = "coordinator" },
            new { Username = "coordinator2", FullName = "Sara Coordinator",   Email = "sara@demo.com",    Role = "coordinator" },
            new { Username = "facilities1",  FullName = "Mohamed Facilities", Email = "mohamed@demo.com", Role = "facilities"  },
            new { Username = "facilities2",  FullName = "Laila Facilities",   Email = "laila@demo.com",   Role = "facilities"  },
            new { Username = "manager1",     FullName = "Khaled Manager",     Email = "khaled@demo.com",  Role = "manager"     },
        };

        string? coordinatorId = null;
        string? facilitiesId  = null;
        string? managerId     = null;

        foreach (var u in usersToCreate)
        {
            var existing = await userManager.FindByNameAsync(u.Username);
            if (existing == null)
            {
                var user = new ApplicationUser
                {
                    UserName       = u.Username,
                    Email          = u.Email,
                    FullName       = u.FullName,
                    Role           = u.Role,
                    IsActive       = true,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, "demo123");
                existing = user;
            }

            if (u.Username == "coordinator1") coordinatorId = existing.Id;
            if (u.Username == "facilities1")  facilitiesId  = existing.Id;
            if (u.Username == "manager1")     managerId     = existing.Id;
        }

        // ── TICKETS ────────────────────────────────────────────────────────
        // Ticket 1: Critical / open — Elevator A fault
        var t1Id = Guid.Parse("50000000-0000-0000-0000-000000000001");
        var t1 = new Ticket
        {
            Id             = t1Id,
            TicketNumber   = "TKT-2026-0001",
            AssetId        = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            Title          = "Elevator A making loud grinding noise",
            Description    = "Residents reported a loud grinding noise from Elevator A every time it moves between floors. Needs urgent inspection.",
            Priority       = "critical",
            Status         = "open",
            SLAHours       = 4,
            CreatedAt      = DateTime.UtcNow.AddDays(-2),
            DueAt          = DateTime.UtcNow.AddDays(-2).AddHours(4),
            CreatedByUserId = coordinatorId!,
        };

        // Ticket 2: Urgent / in_progress — AC Unit 3 not cooling
        var t2Id = Guid.Parse("50000000-0000-0000-0000-000000000002");
        var t2 = new Ticket
        {
            Id               = t2Id,
            TicketNumber     = "TKT-2026-0002",
            AssetId          = Guid.Parse("30000000-0000-0000-0000-000000000006"),
            Title            = "AC Unit 3 — no cooling on Second Floor",
            Description      = "The AC unit on the second floor stopped cooling. Room temperature exceeds 32°C.",
            Priority         = "urgent",
            Status           = "in_progress",
            SLAHours         = 12,
            CreatedAt        = DateTime.UtcNow.AddDays(-1),
            DueAt            = DateTime.UtcNow.AddDays(-1).AddHours(12),
            CreatedByUserId  = coordinatorId!,
            AssignedToUserId = facilitiesId,
            AssignedVendorId = vendorCoolair.Id,
        };

        // Ticket 3: Normal / closed — Printer 1 paper jam
        var t3Id = Guid.Parse("50000000-0000-0000-0000-000000000003");
        var t3 = new Ticket
        {
            Id               = t3Id,
            TicketNumber     = "TKT-2026-0003",
            AssetId          = Guid.Parse("30000000-0000-0000-0000-000000000007"),
            Title            = "Printer 1 — paper jam error",
            Description      = "Printer 1 on Ground Floor showing paper jam error. Paper is stuck inside the roller.",
            Priority         = "normal",
            Status           = "closed",
            SLAHours         = 24,
            CreatedAt        = DateTime.UtcNow.AddDays(-5),
            DueAt            = DateTime.UtcNow.AddDays(-5).AddHours(24),
            CreatedByUserId  = coordinatorId!,
            AssignedToUserId = facilitiesId,
            ResolutionNotes  = "Removed jammed paper from the roller mechanism. Tested — printer working normally.",
            ResolutionByUserId = facilitiesId,
            ActualCost       = 0,
            ClosedAt         = DateTime.UtcNow.AddDays(-4),
        };

        // Ticket 4: Low / open — Water Dispenser leaking
        var t4Id = Guid.Parse("50000000-0000-0000-0000-000000000004");
        var t4 = new Ticket
        {
            Id              = t4Id,
            TicketNumber    = "TKT-2026-0004",
            AssetId         = Guid.Parse("30000000-0000-0000-0000-000000000010"),
            Title           = "Water Dispenser 1 — slow leak at base",
            Description     = "Small water puddle found under Water Dispenser 1 on First Floor. Suspected loose connection.",
            Priority        = "low",
            Status          = "open",
            SLAHours        = 48,
            CreatedAt       = DateTime.UtcNow.AddHours(-6),
            DueAt           = DateTime.UtcNow.AddHours(-6).AddHours(48),
            CreatedByUserId = coordinatorId!,
        };

        // Ticket 5: Critical / in_progress — Elevator C out of service
        var t5Id = Guid.Parse("50000000-0000-0000-0000-000000000005");
        var t5 = new Ticket
        {
            Id               = t5Id,
            TicketNumber     = "TKT-2026-0005",
            AssetId          = Guid.Parse("30000000-0000-0000-0000-000000000003"),
            Title            = "Elevator C — complete breakdown, doors not opening",
            Description      = "Elevator C is stuck at the Second Floor. Doors will not open or close. Requires immediate attention.",
            Priority         = "critical",
            Status           = "in_progress",
            SLAHours         = 4,
            CreatedAt        = DateTime.UtcNow.AddHours(-3),
            DueAt            = DateTime.UtcNow.AddHours(-3).AddHours(4),
            CreatedByUserId  = managerId!,
            AssignedToUserId = facilitiesId,
            AssignedVendorId = vendorAcme.Id,
        };

        // Ticket 6: Urgent / closed — Elevator B inspection overdue
        var t6Id = Guid.Parse("50000000-0000-0000-0000-000000000006");
        var t6 = new Ticket
        {
            Id                 = t6Id,
            TicketNumber       = "TKT-2026-0006",
            AssetId            = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            Title              = "Elevator B — annual safety inspection",
            Description        = "Annual safety inspection for Elevator B is overdue by 2 weeks.",
            Priority           = "urgent",
            Status             = "closed",
            SLAHours           = 12,
            CreatedAt          = DateTime.UtcNow.AddDays(-10),
            DueAt              = DateTime.UtcNow.AddDays(-10).AddHours(12),
            CreatedByUserId    = managerId!,
            AssignedVendorId   = vendorAcme.Id,
            ResolutionNotes    = "Annual inspection completed by ACME Elevators. All safety checks passed. Next inspection due July 2026.",
            ResolutionByUserId = coordinatorId,
            ActualCost         = 1500,
            ClosedAt           = DateTime.UtcNow.AddDays(-9),
        };

        // Ticket 7: Normal / open — AC Unit 1 filter cleaning
        var t7Id = Guid.Parse("50000000-0000-0000-0000-000000000007");
        var t7 = new Ticket
        {
            Id              = t7Id,
            TicketNumber    = "TKT-2026-0007",
            AssetId         = Guid.Parse("30000000-0000-0000-0000-000000000004"),
            Title           = "AC Unit 1 — filter cleaning required",
            Description     = "AC Unit 1 air filter is visibly dirty and needs cleaning. Airflow reduced significantly.",
            Priority        = "normal",
            Status          = "open",
            SLAHours        = 24,
            CreatedAt       = DateTime.UtcNow.AddHours(-12),
            DueAt           = DateTime.UtcNow.AddHours(-12).AddHours(24),
            CreatedByUserId = coordinatorId!,
        };

        // Ticket 8: Low / closed — Projector 1 bulb replacement
        var t8Id = Guid.Parse("50000000-0000-0000-0000-000000000008");
        var t8 = new Ticket
        {
            Id                 = t8Id,
            TicketNumber       = "TKT-2026-0008",
            AssetId            = Guid.Parse("30000000-0000-0000-0000-000000000015"),
            Title              = "Projector 1 — bulb replacement",
            Description        = "Projector 1 in Annex Building First Floor showing low brightness warning. Bulb needs replacement.",
            Priority           = "low",
            Status             = "closed",
            SLAHours           = 48,
            CreatedAt          = DateTime.UtcNow.AddDays(-7),
            DueAt              = DateTime.UtcNow.AddDays(-7).AddHours(48),
            CreatedByUserId    = coordinatorId!,
            AssignedToUserId   = facilitiesId,
            AssignedVendorId   = vendorTechfix.Id,
            ResolutionNotes    = "Replaced projector bulb with new compatible model. Tested — brightness back to normal.",
            ResolutionByUserId = facilitiesId,
            ActualCost         = 250,
            ClosedAt           = DateTime.UtcNow.AddDays(-6),
        };

        // Ticket 9: Urgent / open — Fire Extinguisher inspection overdue
        var t9Id = Guid.Parse("50000000-0000-0000-0000-000000000009");
        var t9 = new Ticket
        {
            Id              = t9Id,
            TicketNumber    = "TKT-2026-0009",
            AssetId         = Guid.Parse("30000000-0000-0000-0000-000000000009"),
            Title           = "Fire Extinguisher 1 — inspection overdue",
            Description     = "Fire Extinguisher 1 on Ground Floor has missed its scheduled quarterly inspection. Compliance risk.",
            Priority        = "urgent",
            Status          = "open",
            SLAHours        = 12,
            CreatedAt       = DateTime.UtcNow.AddHours(-8),
            DueAt           = DateTime.UtcNow.AddHours(-8).AddHours(12),
            CreatedByUserId = managerId!,
        };

        // Ticket 10: Normal / in_progress — Printer 2 connectivity issue
        var t10Id = Guid.Parse("50000000-0000-0000-0000-000000000010");
        var t10 = new Ticket
        {
            Id               = t10Id,
            TicketNumber     = "TKT-2026-0010",
            AssetId          = Guid.Parse("30000000-0000-0000-0000-000000000008"),
            Title            = "Printer 2 — cannot connect to network",
            Description      = "Printer 2 on First Floor dropped off the network. Staff unable to print. Possible NIC or driver issue.",
            Priority         = "normal",
            Status           = "in_progress",
            SLAHours         = 24,
            CreatedAt        = DateTime.UtcNow.AddDays(-1).AddHours(-2),
            DueAt            = DateTime.UtcNow.AddDays(-1).AddHours(-2).AddHours(24),
            CreatedByUserId  = coordinatorId!,
            AssignedToUserId = facilitiesId,
            AssignedVendorId = vendorTechfix.Id,
        };

        context.Tickets.AddRange(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
        await context.SaveChangesAsync();

        // ── TICKET HISTORY ─────────────────────────────────────────────────
        var histories = new List<TicketHistory>
        {
            // T1: open — just created
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t1Id, EventType = "created",        CreatedByUserId = coordinatorId, CreatedAt = t1.CreatedAt,                 Details = "Ticket created with priority: critical" },

            // T2: in_progress — created then assigned
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t2Id, EventType = "created",        CreatedByUserId = coordinatorId, CreatedAt = t2.CreatedAt,                 Details = "Ticket created with priority: urgent" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t2Id, EventType = "status_changed", CreatedByUserId = facilitiesId,  CreatedAt = t2.CreatedAt.AddMinutes(30),  Details = "Status changed from open to in_progress" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t2Id, EventType = "assigned",       CreatedByUserId = coordinatorId, CreatedAt = t2.CreatedAt.AddMinutes(35),  Details = "Assigned to Mohamed Facilities. Vendor: CoolAir HVAC" },

            // T3: closed — full lifecycle
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t3Id, EventType = "created",        CreatedByUserId = coordinatorId, CreatedAt = t3.CreatedAt,                 Details = "Ticket created with priority: normal" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t3Id, EventType = "status_changed", CreatedByUserId = facilitiesId,  CreatedAt = t3.CreatedAt.AddHours(1),     Details = "Status changed from open to in_progress" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t3Id, EventType = "assigned",       CreatedByUserId = coordinatorId, CreatedAt = t3.CreatedAt.AddHours(1),     Details = "Assigned to Mohamed Facilities" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t3Id, EventType = "closed",         CreatedByUserId = facilitiesId,  CreatedAt = t3.ClosedAt!.Value,           Details = "Ticket closed. Resolution: Removed jammed paper from the roller mechanism." },

            // T4: open — just created
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t4Id, EventType = "created",        CreatedByUserId = coordinatorId, CreatedAt = t4.CreatedAt,                 Details = "Ticket created with priority: low" },

            // T5: in_progress — critical, assigned quickly
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t5Id, EventType = "created",        CreatedByUserId = managerId,     CreatedAt = t5.CreatedAt,                 Details = "Ticket created with priority: critical" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t5Id, EventType = "status_changed", CreatedByUserId = facilitiesId,  CreatedAt = t5.CreatedAt.AddMinutes(10),  Details = "Status changed from open to in_progress" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t5Id, EventType = "assigned",       CreatedByUserId = managerId,     CreatedAt = t5.CreatedAt.AddMinutes(10),  Details = "Assigned to Mohamed Facilities. Vendor: ACME Elevators" },

            // T6: closed — full lifecycle
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t6Id, EventType = "created",        CreatedByUserId = managerId,     CreatedAt = t6.CreatedAt,                 Details = "Ticket created with priority: urgent" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t6Id, EventType = "status_changed", CreatedByUserId = facilitiesId,  CreatedAt = t6.CreatedAt.AddHours(2),     Details = "Status changed from open to in_progress" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t6Id, EventType = "assigned",       CreatedByUserId = managerId,     CreatedAt = t6.CreatedAt.AddHours(2),     Details = "Vendor assigned: ACME Elevators" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t6Id, EventType = "closed",         CreatedByUserId = coordinatorId, CreatedAt = t6.ClosedAt!.Value,           Details = "Inspection completed successfully. Cost: $1500" },

            // T7: open — just created
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t7Id, EventType = "created",        CreatedByUserId = coordinatorId, CreatedAt = t7.CreatedAt,                 Details = "Ticket created with priority: normal" },

            // T8: closed — full lifecycle
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t8Id, EventType = "created",        CreatedByUserId = coordinatorId, CreatedAt = t8.CreatedAt,                 Details = "Ticket created with priority: low" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t8Id, EventType = "status_changed", CreatedByUserId = facilitiesId,  CreatedAt = t8.CreatedAt.AddHours(4),     Details = "Status changed from open to in_progress" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t8Id, EventType = "assigned",       CreatedByUserId = coordinatorId, CreatedAt = t8.CreatedAt.AddHours(4),     Details = "Assigned to Mohamed Facilities. Vendor: TechFix Services" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t8Id, EventType = "closed",         CreatedByUserId = facilitiesId,  CreatedAt = t8.ClosedAt!.Value,           Details = "Bulb replaced. Cost: $250" },

            // T9: open — just created
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t9Id, EventType = "created",        CreatedByUserId = managerId,     CreatedAt = t9.CreatedAt,                 Details = "Ticket created with priority: urgent" },

            // T10: in_progress
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t10Id, EventType = "created",       CreatedByUserId = coordinatorId, CreatedAt = t10.CreatedAt,                Details = "Ticket created with priority: normal" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t10Id, EventType = "status_changed",CreatedByUserId = facilitiesId,  CreatedAt = t10.CreatedAt.AddHours(1),    Details = "Status changed from open to in_progress" },
            new TicketHistory { Id = Guid.NewGuid(), TicketId = t10Id, EventType = "assigned",      CreatedByUserId = coordinatorId, CreatedAt = t10.CreatedAt.AddHours(1),    Details = "Assigned to Mohamed Facilities. Vendor: TechFix Services" },
        };
        context.TicketHistory.AddRange(histories);
        await context.SaveChangesAsync();

        // ── ATTACHMENTS ────────────────────────────────────────────────────
        var attachments = new List<Attachment>
        {
            // T1 — photo of elevator damage
            new Attachment
            {
                Id                = Guid.NewGuid(),
                TicketId          = t1Id,
                Filename          = "elevator_a_grinding_noise.jpg",
                Url               = "/uploads/tickets/elevator_a_grinding_noise.jpg",
                ContentType       = "image/jpeg",
                SizeBytes         = 204800,
                UploadedByUserId  = coordinatorId,
                UploadedAt        = t1.CreatedAt.AddMinutes(5),
            },
            // T2 — AC temperature reading photo
            new Attachment
            {
                Id                = Guid.NewGuid(),
                TicketId          = t2Id,
                Filename          = "ac_unit3_temp_reading.jpg",
                Url               = "/uploads/tickets/ac_unit3_temp_reading.jpg",
                ContentType       = "image/jpeg",
                SizeBytes         = 153600,
                UploadedByUserId  = coordinatorId,
                UploadedAt        = t2.CreatedAt.AddMinutes(3),
            },
            // T5 — elevator C stuck door photo
            new Attachment
            {
                Id                = Guid.NewGuid(),
                TicketId          = t5Id,
                Filename          = "elevator_c_stuck_door.jpg",
                Url               = "/uploads/tickets/elevator_c_stuck_door.jpg",
                ContentType       = "image/jpeg",
                SizeBytes         = 307200,
                UploadedByUserId  = managerId,
                UploadedAt        = t5.CreatedAt.AddMinutes(2),
            },
            // T6 — closed inspection report PDF
            new Attachment
            {
                Id                = Guid.NewGuid(),
                TicketId          = t6Id,
                Filename          = "elevator_b_inspection_report.pdf",
                Url               = "/uploads/tickets/elevator_b_inspection_report.pdf",
                ContentType       = "application/pdf",
                SizeBytes         = 512000,
                UploadedByUserId  = coordinatorId,
                UploadedAt        = t6.ClosedAt!.Value,
            },
            // T8 — closed, photo of replaced bulb
            new Attachment
            {
                Id                = Guid.NewGuid(),
                TicketId          = t8Id,
                Filename          = "projector1_new_bulb.jpg",
                Url               = "/uploads/tickets/projector1_new_bulb.jpg",
                ContentType       = "image/jpeg",
                SizeBytes         = 102400,
                UploadedByUserId  = facilitiesId,
                UploadedAt        = t8.ClosedAt!.Value,
            },
        };
        context.Attachments.AddRange(attachments);
        await context.SaveChangesAsync();
    }
}
