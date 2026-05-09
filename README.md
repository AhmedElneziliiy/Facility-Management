# Facility Management System

A full-stack **ASP.NET Core 8** web application for managing physical assets, buildings, maintenance tickets, and vendors across facilities — with both an **MVC web interface** and a **REST API**.

## What it does

This system helps facility teams track all physical assets (equipment, devices, infrastructure) across buildings and floors, manage maintenance requests through a ticketing system, and coordinate with vendors for repairs.

## Key Features

- **Asset Management** — Track assets per building/floor with status, criticality level (Low/Medium/High/Critical), and full history
- **Ticket System** — Create, assign, and resolve maintenance tickets with SLA tracking, priorities, attachments, and status history
- **Building & Floor Management** — Organize assets hierarchically: Organization → Building → Floor → Asset
- **Vendor Management** — Register and manage vendors that handle maintenance work
- **Dashboard** — Overview stats: open tickets, assets by status, building performance metrics
- **Dual Interface** — MVC views for browser-based use + REST API for mobile/external integrations
- **JWT + Cookie Auth** — JWT for API clients, cookie-based auth for the MVC web interface
- **PDF Reports** — Export maintenance reports as PDF

## Tech Stack

- ASP.NET Core 8 (MVC + Web API)
- Entity Framework Core + SQL Server
- ASP.NET Identity + JWT Bearer + Cookie Authentication
- Clean Architecture (Models / Infrastructure / Core / Web layers)
- Swagger / OpenAPI

## Architecture

```
AssetManagement.Models/         → Entities, DTOs, ViewModels, Enums
AssetManagement.Infrastructure/ → DbContext, Identity, Migrations, Repositories
AssetManagement.Core/           → Interfaces, Services (business logic), Helpers
AssetManagement.Web/            → MVC Controllers, API Controllers, Views, wwwroot
```

## Core Entities

| Entity | Description |
|---|---|
| `Building` | A physical facility location |
| `Floor` | A floor within a building |
| `Asset` | A tracked item (equipment, device, etc.) |
| `Ticket` | A maintenance request linked to an asset |
| `TicketHistory` | Audit trail of every status change |
| `Vendor` | External service provider |
| `Attachment` | Files/photos attached to tickets |

## Getting Started

1. Set the SQL Server connection string in `appsettings.json`
2. Configure `JwtSettings:Secret` in `appsettings.json`
3. Run: `dotnet ef database update --project AssetManagement.Infrastructure`
4. Run the app — the DB initializer seeds default admin credentials
5. Access the web UI at `/` or the API docs at `/swagger`
