# Asset Management Platform - 3-5 Day Demo Specification
**For Parallel Flutter & Backend Development**

**Date:** February 26, 2026  
**Timeline:** 3-5 days  
**Goal:** Working demo showing QR scan → create ticket → assign → resolve workflow

---

## 📖 VISUAL STORYBOARD - What This Demo Will Achieve

This storyboard shows the complete end-to-end workflow that will be demonstrated:

```
┌─────────────────────────────────────────────────────────────────────┐
│                        ACT 1: PROBLEM DISCOVERY                      │
│                         (Building Coordinator)                       │
└─────────────────────────────────────────────────────────────────────┘

👤 Ahmed (Building Coordinator) walks into his building
   
   [🚪 Enters Main Campus Building]
         ↓
   [🔍 Notices elevator stuck between floors]
         ↓
   [📱 Opens Asset Management App on phone]
         ↓
   [🔐 Logs in: coordinator1 / demo123]
         ↓
   [📷 Taps "Scan Asset" button]
         ↓
   [📊 Scans QR code on elevator panel]
         ↓
   
   📱 SCREEN: Asset Details Appear
   ┌──────────────────────────────┐
   │ Elevator A                   │
   │ Building: Main Campus        │
   │ Floor: 2                     │
   │ Criticality: 🔴 SAFETY       │
   │ Serial: ELEV-2020-01         │
   │                              │
   │ [Create Ticket] ← Taps this  │
   └──────────────────────────────┘
         ↓
   
   📱 SCREEN: Create Ticket Form
   ┌──────────────────────────────┐
   │ Title: Elevator stuck        │
   │ Description: Stopped at      │
   │   floor 2, won't move        │
   │ Priority: 🔴 Critical        │
   │ 📷 [Photo attached]          │
   │                              │
   │ [Submit Ticket]  ← Taps this │
   └──────────────────────────────┘
         ↓
   
   ✅ Ticket TKT-2026-0001 Created!
   📱 Shows: "Ticket created successfully"
   
   [Returns to "My Tickets" list]
         ↓
   
   📱 SCREEN: My Tickets
   ┌──────────────────────────────┐
   │ TKT-2026-0001               │
   │ Elevator stuck              │
   │ 🔴 Critical | ⏰ Due: 4hrs  │
   │ Status: Open                │
   └──────────────────────────────┘


┌─────────────────────────────────────────────────────────────────────┐
│                        ACT 2: RESPONSE                               │
│                        (Facilities Team)                             │
└─────────────────────────────────────────────────────────────────────┘

👤 Mohamed (Facilities Team) at his desk
   
   [💻 Opens Web Dashboard OR 📱 Mobile App]
         ↓
   [🔐 Logs in: facilities1 / demo123]
         ↓
   [↻ Pulls down to refresh ticket list]
         ↓
   
   💻/📱 SCREEN: All Tickets Dashboard
   ┌──────────────────────────────────────────────┐
   │ 🔍 Filter: [All] [Critical] [Open]          │
   │                                               │
   │ TKT-2026-0001  NEW!                          │
   │ Elevator stuck between floors                │
   │ 🔴 Critical | Asset: Elevator A              │
   │ 🏢 Main Campus, Floor 2                      │
   │ ⏰ Due in 3h 45m                             │
   │ 🔴 SAFETY CRITICAL                           │
   │ Reported by: Ahmed Coordinator               │
   │ [View Details] ← Clicks this                 │
   └──────────────────────────────────────────────┘
         ↓
   
   💻/📱 SCREEN: Ticket Details
   ┌──────────────────────────────────────────────┐
   │ TKT-2026-0001 | 🔴 Critical | Open          │
   │                                               │
   │ Elevator stuck between floors                │
   │ Description: Stopped at floor 2, won't move  │
   │                                               │
   │ 📷 [Shows photo attachment]                  │
   │                                               │
   │ Asset: Elevator A (SAFETY CRITICAL)          │
   │ Location: Main Campus, Floor 2               │
   │                                               │
   │ Timeline:                                     │
   │ • Created by Ahmed - 10 min ago              │
   │                                               │
  │ ─────────────────────────────                │
  │ 👤 Created by: Ahmed (Coordinator)           │
  │ 🏢 Vendor: ACME Elevators                    │
  │ 📊 Status: [In Progress] ▼                   │
  │                                               │
  │ [Update Status] ← Clicks this                │
   └──────────────────────────────────────────────┘
         ↓
   
  ✅ Ticket Updated!
  Status: Open → In Progress
  (Vendor and Created by remain unchanged)
   
   📞 Mohamed calls ACME Elevators
   🚗 Technician dispatched


┌─────────────────────────────────────────────────────────────────────┐
│                      ACT 3: VERIFICATION                             │
│                    (Back to Coordinator)                             │
└─────────────────────────────────────────────────────────────────────┘

👤 Ahmed (2 hours later, technician has fixed the elevator)
   
   [📱 Opens app again]
         ↓
   [↻ Pulls down to refresh "My Tickets"]
         ↓
   
   📱 SCREEN: My Tickets (Updated)
   ┌──────────────────────────────┐
   │ TKT-2026-0001               │
   │ Elevator stuck              │
   │ 🟡 Critical                 │
   │ Status: In Progress ← NEW!  │
  │ Assigned: —                 │
   │ Vendor: ACME Elevators      │
   └──────────────────────────────┘
         ↓
   
   [Taps on ticket to see details]
         ↓
   
   📱 SCREEN: Ticket Details (Updated)
   ┌──────────────────────────────┐
   │ Timeline:                    │
   │ • Created - 2h ago          │
   │ • Assigned to Mohamed - 1h  │
   │ • In Progress - 1h ago      │
   │ • Vendor: ACME assigned     │
   └──────────────────────────────┘
         ↓
   
   [🚶 Ahmed walks to elevator, tests it]
   [✅ Elevator working perfectly!]
         ↓
   
   [📱 Opens ticket again]
   [Taps "Verify & Close Ticket"]
         ↓
   
   📱 SCREEN: Close Ticket
   ┌──────────────────────────────┐
   │ Resolution Notes:            │
   │ "Technician reset the        │
   │  controller. Elevator        │
   │  tested and working."        │
   │                              │
   │ [Close Ticket] ← Taps this   │
   └──────────────────────────────┘
         ↓
   
   ✅ Ticket Closed!
   Status: In Progress → Closed
   Resolution time: 2 hours 15 minutes


┌─────────────────────────────────────────────────────────────────────┐
│                      ACT 4: ANALYTICS                                │
│                        (Management View)                             │
└─────────────────────────────────────────────────────────────────────┘

👤 Khaled (Manager) reviews performance
   
   [💻 Opens Web Dashboard]
         ↓
   [🔐 Logs in: manager1 / demo123]
         ↓
   
   💻 SCREEN: Management Dashboard
   ┌─────────────────────────────────────────────────────┐
   │  📊 ASSET MANAGEMENT DASHBOARD                      │
   │                                                      │
   │  ┌───────────┐ ┌───────────┐ ┌───────────┐        │
   │  │ 📦 Total  │ │ 🎫 Open   │ │ 🔴 Critical│        │
   │  │   Assets  │ │  Tickets  │ │  Tickets  │        │
   │  │    15     │ │     2     │ │     0     │        │
   │  └───────────┘ └───────────┘ └───────────┘        │
   │                                                      │
   │  ┌─────────────────────────────────┐               │
   │  │ ⏱️ Avg Resolution Time           │               │
   │  │        62 minutes                │               │
   │  └─────────────────────────────────┘               │
   │                                                      │
   │  📊 Tickets by Priority:                            │
   │  ┌─────────────────────────┐                       │
   │  │ 🔴 Critical:    0  ████ │                       │
   │  │ 🟠 Urgent:      1  ████ │                       │
   │  │ 🟡 Normal:      1  ████ │                       │
   │  │ 🟢 Low:         0       │                       │
   │  └─────────────────────────┘                       │
   │                                                      │
   │  🏢 Building Performance:                           │
   │  ┌─────────────────────────────────────┐           │
   │  │ Main Campus:   1 open  | Avg: 60min │           │
   │  │ Annex Building: 1 open | Avg: 90min │           │
   │  └─────────────────────────────────────┘           │
   │                                                      │
   │  ✅ Recent Resolutions:                             │
   │  • TKT-2026-0001 - Elevator (2h 15m) ← NEW!        │
   │  • TKT-2026-0025 - AC Unit (1h 30m)                │
   │  • TKT-2026-0024 - Printer (45m)                   │
   └─────────────────────────────────────────────────────┘
   
   💡 Insights visible:
   - Fast response time (2h 15min for critical)
   - Safety issues prioritized
   - All critical tickets resolved
   - Building performance tracked


═══════════════════════════════════════════════════════════════════════
                            🎬 END OF DEMO
═══════════════════════════════════════════════════════════════════════

WHAT WAS ACCOMPLISHED:
✅ Coordinator quickly reported a critical safety issue via mobile
✅ Issue was documented with photo evidence
✅ Facilities team saw the issue, assigned resources, and coordinated repair
✅ Coordinator verified the repair completion on-site
✅ Management gained visibility into response times and building performance
✅ Complete audit trail of the entire process

TIME TO RESOLUTION: 2 hours 15 minutes
(From report → repair → verification)

BUSINESS VALUE DEMONSTRATED:
- 70% faster than traditional phone/email reporting
- Photo evidence reduces miscommunication
- Complete accountability and audit trail
- Data-driven insights for management decisions
- Scalable across multiple buildings and assets
```

---

## 🎯 Demo Scope (What We're Building)

### Core Features ONLY (Demo MVP)
1. ✅ **Login** - Simple username/password (no registration)
2. ✅ **QR Scanning** - Scan asset QR code to fetch asset details
3. ✅ **Create Ticket** - With photo upload (attachment), priority selection
4. ✅ **Ticket List** - View tickets (coordinator sees own, facilities sees all)
5. ✅ **Ticket Details** - View full ticket with timeline and photo attachments
6. ✅ **Update Status** - Facilities can change status from `open` → `in_progress` only (vendor and creator are shown and not changed by facilities)
7. ✅ **Close Ticket** - Coordinator verifies and closes after repair
8. ✅ **Dashboard Analytics** - Basic KPIs (4 cards, 2 charts)

**What are Attachments?**
Attachments are photos that users take with their phone camera when reporting an issue. For example, if the elevator is stuck, the coordinator takes a photo of the error display and attaches it to the ticket. This helps the facilities team see the problem before they arrive.

### NOT in Demo (Phase 2)
- ❌ Push notifications (real-time alerts)
- ❌ Preventive maintenance scheduling
- ❌ Vendor management (only basic vendor assignment)
- ❌ Depreciation tracking
- ❌ Asset transfer/checkout
- ❌ Offline mode
- ❌ Multi-language
- ❌ Complex reporting

---

## 📱 Flutter Screens (Mobile App)

### Required Screens (Priority Order)

#### 1. Login Screen (Day 1)
**File:** `lib/screens/auth/login_screen.dart`

**UI Elements:**
- Username text field
- Password text field
- Login button
- Logo at top
- Loading indicator

**API Call:**
```dart
POST /auth/login
Body: {"username": "coordinator1", "password": "demo123"}
Response: {"token": "jwt...", "user": {...}}
```

**State Management:**
- Store JWT token in secure storage
- Navigate to Home on success

---

#### 2. Home Screen / Navigation (Day 1)
**File:** `lib/screens/home/home_screen.dart`

**For Coordinator:**
- "Scan Asset" button (primary action)
- "My Tickets" list

**For Facilities:**
- "All Tickets" list with filters
- Dashboard link

**Bottom Nav:**
- Home
- Tickets
- Profile

---

#### 3. QR Scanner Screen (Day 2)
**File:** `lib/screens/scanner/qr_scanner_screen.dart`

**UI Elements:**
- Camera view with overlay
- "Tap to scan" instruction
- Cancel button

**Package:** `mobile_scanner` or `qr_code_scanner`

**API Call on Scan:**
```dart
GET /assets/{qrCode}
Response: Asset object with details
```

**Navigation:**
- On success → Asset Details Screen
- On error → Show snackbar

---

#### 4. Asset Details Screen (Day 2)
**File:** `lib/screens/assets/asset_details_screen.dart`

**Display:**
- Asset name
- Building / Floor
- Criticality badge (Safety/Operational/Low)
- Serial number
- Last serviced date
- "Create Ticket" button (primary action)

**Navigation:**
- Tap "Create Ticket" → Create Ticket Screen

---

#### 5. Create Ticket Screen (Day 2)
**File:** `lib/screens/tickets/create_ticket_screen.dart`

**Form Fields:**
- Title (pre-filled from asset name)
- Description (multiline text)
- Priority dropdown (Critical / Urgent / Normal / Low)
- Photo attachment (camera or gallery)
- Submit button

**API Call:**
```dart
POST /tickets
Body: {
  "assetId": "uuid",
  "title": "...",
  "description": "...",
  "priority": "critical",
  "attachments": [...]
}
Response: Created ticket object
```

**After Submit:**
- Show success message
- Navigate to My Tickets

---

#### 6. Tickets List Screen (Day 3)
**File:** `lib/screens/tickets/tickets_list_screen.dart`

**For Coordinator:**
```dart
GET /tickets?createdByUserId={userId}
```

**For Facilities:**
```dart
GET /tickets
// Optional filters: status, priority, buildingId
```

**List Item UI:**
- Ticket number
- Asset name
- Priority badge (color-coded)
- Status
- SLA countdown (if approaching due time)
- Created date

**Pull-to-refresh**

---

#### 7. Ticket Details Screen (Day 3)
**File:** `lib/screens/tickets/ticket_details_screen.dart`

**Sections:**
1. **Header:**
   - Ticket number
   - Status badge
   - Priority badge

2. **Asset Info:**
   - Asset name (tappable to see asset details)
   - Building / Floor
   - Criticality badge

3. **Ticket Info:**
   - Title
   - Description
   - Created by
   - Created at
   - Due at (with countdown)

4. **Attachments:**
   - Photo thumbnails (tappable to view full)

5. **Timeline:**
   - Created
   - Status changes
   - Assigned to
   - Closed (if applicable)

6. **Actions (Role-Based):**
  - **Facilities:** 
    - "Change Status" button (only: `open` → `in_progress`)
   - **Coordinator (if status is resolved):**
     - "Verify & Close" button

**API Calls:**
```dart
GET /tickets/{id}

// Update status (facilities only)
PUT /tickets/{id}/status
Body: {"status": "in_progress"}

// Close ticket (coordinator only)
PUT /tickets/{id}/close
Body: {"resolutionNotes": "Verified repair complete"}
```

---

#### 8. Profile Screen (Day 4)
**File:** `lib/screens/profile/profile_screen.dart`

**Display:**
- User name
- Email
- Role badge
- Logout button

---

### Flutter State Management
**Recommendation:** Use **Provider** or **Riverpod**

**Required Providers:**
- `AuthProvider` - JWT token, current user
- `TicketsProvider` - Ticket list, ticket details
- `AssetsProvider` - Asset details

---

### Flutter Packages Needed

```yaml
dependencies:
  flutter:
    sdk: flutter
  
  # Network & API
  http: ^1.1.0
  dio: ^5.4.0  # Alternative to http (choose one)
  
  # State Management
  provider: ^6.1.0
  # OR riverpod: ^2.4.0
  
  # Storage
  flutter_secure_storage: ^9.0.0
  shared_preferences: ^2.2.0
  
  # QR Scanning
  mobile_scanner: ^3.5.0
  # OR qr_code_scanner: ^1.0.1
  
  # Image Handling
  image_picker: ^1.0.5
  cached_network_image: ^3.3.0
  
  # UI Components
  flutter_svg: ^2.0.9
  shimmer: ^3.0.0  # Loading skeleton
  
  # Utilities
  intl: ^0.18.1  # Date formatting
  timeago: ^3.5.0  # Relative time
```

---

## 🔧 Backend API Endpoints (ASP.NET Core)

### Priority Order for Implementation

### DAY 1 - Foundation

#### 1. POST /auth/login ⭐️ CRITICAL
```csharp
Request:
{
  "username": "coordinator1",
  "password": "demo123"
}

Response 200:
{
  "token": "eyJhbGc...",
  "user": {
    "id": "uuid-1",
    "username": "coordinator1",
    "fullName": "Ahmed Coordinator",
    "role": "coordinator",
    "organizationId": "uuid-org-1"
  }
}
```

**Implementation Notes:**
- Use JWT tokens with 24hr expiration
- Hash passwords with BCrypt or Identity
- Include user role in JWT claims
- Return user object with role for frontend routing

---

#### 2. GET /users/me ⭐️ CRITICAL
```csharp
Headers: Authorization: Bearer {token}

Response 200:
{
  "id": "uuid-1",
  "username": "coordinator1",
  "fullName": "Ahmed Coordinator",
  "role": "coordinator",
  "email": "coordinator@demo.com"
}
```

---

### DAY 2 - Core Features

#### 3. GET /assets/{qrCode} ⭐️ CRITICAL
```csharp
GET /assets/ASSET-ELEV-01

Response 200:
{
  "id": "asset-uuid-1",
  "name": "Elevator A",
  "assetTypeId": "type-elev",
  "buildingId": "bld-1",
  "buildingName": "Main Campus",
  "floorId": "floor-2",
  "floorLevel": 2,
  "qrCode": "ASSET-ELEV-01",
  "serialNumber": "ELEV-2020-01",
  "status": "active",
  "criticality": "safety",
  "lastServicedAt": "2026-01-15T09:00:00Z",
  "nextServiceDueAt": "2026-07-15T09:00:00Z"
}

Response 404:
{
  "error": "Asset not found"
}
```

**Implementation Notes:**
- Case-insensitive QR code lookup
- Return denormalized building/floor names
- Include criticality for mobile display

---

#### 4. POST /tickets ⭐️ CRITICAL
```csharp
Request:
{
  "assetId": "asset-uuid-1",
  "title": "Elevator stuck between floors",
  "description": "Elevator stopped at floor 2 and won't move.",
  "priority": "critical",
  "attachments": [
    {
      "filename": "photo1.jpg",
      "url": "https://storage.example.com/photo1.jpg",
      "contentType": "image/jpeg",
      "sizeBytes": 234567
    }
  ]
}

Response 201:
{
  "id": "ticket-uuid-123",
  "ticketNumber": "TKT-2026-0001",
  "assetId": "asset-uuid-1",
  "title": "Elevator stuck between floors",
  "description": "...",
  "priority": "critical",
  "status": "open",
  "slaHours": 4,
  "dueAt": "2026-02-26T18:30:00Z",
  "createdAt": "2026-02-26T14:30:00Z",
  "createdByUserId": "uuid-1",
  "createdByUserName": "Ahmed Coordinator"
}
```

**Backend Logic:**
- Auto-generate ticket number (TKT-YYYY-####)
- Set status = "open"
- Calculate SLA:
  - critical → 4 hours
  - urgent → 12 hours
  - normal → 24 hours
  - low → 48 hours
- Set dueAt = createdAt + slaHours
- Create TicketHistory entry (eventType: "created")

---

#### 5. POST /tickets/{id}/attachments
```csharp
// For uploading additional photos after ticket creation

Request: Multipart form-data with file

Response 200:
{
  "id": "attachment-uuid",
  "ticketId": "ticket-uuid-123",
  "filename": "photo2.jpg",
  "url": "https://storage.example.com/photo2.jpg",
  "contentType": "image/jpeg",
  "sizeBytes": 156789,
  "uploadedAt": "2026-02-26T14:35:00Z"
}
```

**Implementation:**
- Accept image files only (jpg, png, heic)
- Max 10MB per file
- Upload to Azure Blob / AWS S3 / Local storage
- Return URL for mobile to display

---

### DAY 3 - Status Management

#### 6. GET /tickets ⭐️ CRITICAL
```csharp
Query Params:
- createdByUserId (optional) - for coordinator's "My Tickets"
- status (optional) - open, in_progress, closed
- priority (optional)
- buildingId (optional)
- assignedToUserId (optional)

Response 200:
{
  "tickets": [
    {
      "id": "ticket-uuid-123",
      "ticketNumber": "TKT-2026-0001",
      "title": "Elevator stuck between floors",
      "assetId": "asset-uuid-1",
      "assetName": "Elevator A",
      "buildingId": "bld-1",
      "buildingName": "Main Campus",
      "floorLevel": 2,
      "priority": "critical",
      "status": "open",
      "criticality": "safety",
      "createdAt": "2026-02-26T14:30:00Z",
      "dueAt": "2026-02-26T18:30:00Z",
      "createdByUserName": "Ahmed Coordinator",
      "assignedToUserName": null
    }
  ],
  "meta": {
    "total": 1,
    "page": 1,
    "pageSize": 20
  }
}
```

**Implementation Notes:**
- Paginate results (20 per page)
- Order by createdAt DESC by default
- Include denormalized asset/building names
- Calculate if ticket is overdue

---

#### 7. GET /tickets/{id} ⭐️ CRITICAL
```csharp
Response 200:
{
  "id": "ticket-uuid-123",
  "ticketNumber": "TKT-2026-0001",
  "title": "Elevator stuck between floors",
  "description": "Elevator stopped at floor 2 and won't move.",
  "assetSnapshot": {
    "id": "asset-uuid-1",
    "name": "Elevator A",
    "buildingName": "Main Campus",
    "floorLevel": 2,
    "criticality": "safety",
    "serialNumber": "ELEV-2020-01"
  },
  "priority": "critical",
  "status": "open",
  "slaHours": 4,
  "dueAt": "2026-02-26T18:30:00Z",
  "createdAt": "2026-02-26T14:30:00Z",
  "createdByUserId": "uuid-1",
  "createdByUserName": "Ahmed Coordinator",
  "assignedToUserId": null,
  "assignedToUserName": null,
  "assignedVendorId": null,
  "assignedVendorName": null,
  "closedAt": null,
  "resolutionNotes": null,
  "attachments": [
    {
      "id": "att-1",
      "filename": "photo1.jpg",
      "url": "https://storage.example.com/photo1.jpg",
      "contentType": "image/jpeg"
    }
  ],
  "history": [
    {
      "id": "hist-1",
      "eventType": "created",
      "createdAt": "2026-02-26T14:30:00Z",
      "createdByUserId": "uuid-1",
      "createdByUserName": "Ahmed Coordinator",
      "details": null
    }
  ]
}
```

---

#### 8. PUT /tickets/{id}/status ⭐️ CRITICAL
```csharp
Request:
{
  "status": "in_progress",
  "assignedToUserId": "uuid-facilities-1",
  "assignedVendorId": "vendor-uuid-1"  // optional
}

Response 200:
{
  "id": "ticket-uuid-123",
  "status": "in_progress",
  "assignedToUserId": "uuid-facilities-1",
  "assignedToUserName": "Mohamed Facilities",
  "assignedVendorId": "vendor-uuid-1",
  "assignedVendorName": "ACME Elevators",
  "updatedAt": "2026-02-26T14:40:00Z"
}
```

**Backend Logic:**
- Validate status transition (open → in_progress → closed)
- Create TicketHistory entry (eventType: "status_changed")
- If assignedToUserId changed, create history entry (eventType: "assigned")

---

#### 9. PUT /tickets/{id}/close ⭐️ CRITICAL
```csharp
Request:
{
  "resolutionNotes": "Reset the elevator controller; tested under full load.",
  "actualCost": 250.00  // optional
}

Response 200:
{
  "id": "ticket-uuid-123",
  "status": "closed",
  "closedAt": "2026-02-26T15:10:00Z",
  "resolutionNotes": "Reset the elevator controller; tested under full load.",
  "resolutionByUserId": "uuid-1",
  "resolutionByUserName": "Ahmed Coordinator",
  "timeSpentMinutes": 40,
  "actualCost": 250.00
}
```

**Backend Logic:**
- Set status = "closed"
- Set closedAt = now
- Set resolutionByUserId = current user
- Calculate timeSpentMinutes = (closedAt - createdAt) in minutes
- Create TicketHistory entry (eventType: "closed")

---

### DAY 3-4 - Dashboard & Analytics

#### 10. GET /dashboard/stats
```csharp
Query Params:
- buildingId (optional)

Response 200:
{
  "totalAssets": 15,
  "openTickets": 3,
  "criticalTickets": 1,
  "avgResolutionTimeMinutes": 62,
  "ticketsByPriority": {
    "critical": 1,
    "urgent": 1,
    "normal": 1,
    "low": 0
  },
  "ticketsByStatus": {
    "open": 2,
    "in_progress": 1,
    "closed": 10
  }
}
```

---

#### 11. GET /dashboard/buildings/performance
```csharp
Response 200:
[
  {
    "buildingId": "bld-1",
    "buildingName": "Main Campus",
    "openTickets": 2,
    "criticalTickets": 1,
    "avgResolutionTimeMinutes": 50
  },
  {
    "buildingId": "bld-2",
    "buildingName": "Annex Building",
    "openTickets": 1,
    "criticalTickets": 0,
    "avgResolutionTimeMinutes": 120
  }
]
```

---

### DAY 4 - Supporting Endpoints

#### 12. GET /buildings
```csharp
Response 200:
[
  {
    "id": "bld-1",
    "name": "Main Campus",
    "address": "123 Main St",
    "floorsCount": 3
  },
  {
    "id": "bld-2",
    "name": "Annex Building",
    "address": "456 Side Ave",
    "floorsCount": 2
  }
]
```

---

#### 13. GET /vendors
```csharp
Response 200:
[
  {
    "id": "vendor-1",
    "name": "ACME Elevators",
    "contactName": "John Smith",
    "contactPhone": "+20123456789"
  },
  {
    "id": "vendor-2",
    "name": "CoolAir HVAC",
    "contactName": "Sara Ahmed",
    "contactPhone": "+20123456790"
  }
]
```

---

## 🗄️ Database Schema (Minimal for Demo)

### Users Table
```sql
CREATE TABLE Users (
    Id UUID PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100),
    Role VARCHAR(20) NOT NULL,  -- coordinator, facilities, manager
    OrganizationId UUID,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT NOW()
);
```

### Buildings Table
```sql
CREATE TABLE Buildings (
    Id UUID PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Address TEXT,
    FloorsCount INTEGER NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW()
);
```

### Floors Table
```sql
CREATE TABLE Floors (
    Id UUID PRIMARY KEY,
    BuildingId UUID NOT NULL REFERENCES Buildings(Id),
    Level INTEGER NOT NULL,
    Name VARCHAR(50),
    CreatedAt TIMESTAMP DEFAULT NOW()
);
```

### Assets Table
```sql
CREATE TABLE Assets (
    Id UUID PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    QRCode VARCHAR(100) UNIQUE NOT NULL,
    SerialNumber VARCHAR(100),
    BuildingId UUID NOT NULL REFERENCES Buildings(Id),
    FloorId UUID REFERENCES Floors(Id),
    Status VARCHAR(20) DEFAULT 'active',  -- active, in_repair, decommissioned
    Criticality VARCHAR(20) DEFAULT 'low',  -- safety, operational, low
    LastServicedAt TIMESTAMP,
    NextServiceDueAt TIMESTAMP,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    
    INDEX idx_qrcode (QRCode)
);
```

### Vendors Table
```sql
CREATE TABLE Vendors (
    Id UUID PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    ContactName VARCHAR(100),
    ContactPhone VARCHAR(20),
    ContactEmail VARCHAR(100),
    CreatedAt TIMESTAMP DEFAULT NOW()
);
```

### Tickets Table
```sql
CREATE TABLE Tickets (
    Id UUID PRIMARY KEY,
    TicketNumber VARCHAR(20) UNIQUE NOT NULL,
    AssetId UUID NOT NULL REFERENCES Assets(Id),
    Title VARCHAR(200) NOT NULL,
    Description TEXT NOT NULL,
    Priority VARCHAR(20) NOT NULL,  -- critical, urgent, normal, low
    Status VARCHAR(20) DEFAULT 'open',  -- open, in_progress, closed, cancelled
    SLAHours INTEGER,
    DueAt TIMESTAMP,
    CreatedByUserId UUID NOT NULL REFERENCES Users(Id),
    AssignedToUserId UUID REFERENCES Users(Id),
    AssignedVendorId UUID REFERENCES Vendors(Id),
    ResolutionNotes TEXT,
    ResolutionByUserId UUID REFERENCES Users(Id),
    ActualCost DECIMAL(10,2),
    CreatedAt TIMESTAMP DEFAULT NOW(),
    ClosedAt TIMESTAMP,
    
    INDEX idx_status (Status),
    INDEX idx_created_by (CreatedByUserId),
    INDEX idx_due_at (DueAt)
);
```

### Attachments Table
```sql
CREATE TABLE Attachments (
    Id UUID PRIMARY KEY,
    TicketId UUID NOT NULL REFERENCES Tickets(Id),
    Filename VARCHAR(255) NOT NULL,
    Url TEXT NOT NULL,
    ContentType VARCHAR(50),
    SizeBytes INTEGER,
    UploadedByUserId UUID REFERENCES Users(Id),
    UploadedAt TIMESTAMP DEFAULT NOW()
);
```

### TicketHistory Table
```sql
CREATE TABLE TicketHistory (
    Id UUID PRIMARY KEY,
    TicketId UUID NOT NULL REFERENCES Tickets(Id),
    EventType VARCHAR(50) NOT NULL,  -- created, status_changed, assigned, closed
    CreatedByUserId UUID REFERENCES Users(Id),
    Details JSONB,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    
    INDEX idx_ticket_id (TicketId)
);
```

---

## 📊 Sample Data (Seed Database)

### Users (5 users)
```json
[
  {
    "username": "coordinator1",
    "password": "demo123",
    "fullName": "Ahmed Coordinator",
    "email": "ahmed@demo.com",
    "role": "coordinator"
  },
  {
    "username": "coordinator2",
    "password": "demo123",
    "fullName": "Sara Coordinator",
    "email": "sara@demo.com",
    "role": "coordinator"
  },
  {
    "username": "facilities1",
    "password": "demo123",
    "fullName": "Mohamed Facilities",
    "email": "mohamed@demo.com",
    "role": "facilities"
  },
  {
    "username": "facilities2",
    "password": "demo123",
    "fullName": "Laila Facilities",
    "email": "laila@demo.com",
    "role": "facilities"
  },
  {
    "username": "manager1",
    "password": "demo123",
    "fullName": "Khaled Manager",
    "email": "khaled@demo.com",
    "role": "manager"
  }
]
```

### Buildings (2 buildings)
```json
[
  {
    "name": "Main Campus",
    "address": "123 University Avenue, Cairo",
    "floorsCount": 3
  },
  {
    "name": "Annex Building",
    "address": "456 Education Street, Cairo",
    "floorsCount": 2
  }
]
```

### Assets (15 total: 10 in Main Campus, 5 in Annex)
**Main Campus:**
- 3 Elevators (Floor 1, 2, 3) - Criticality: Safety
- 3 Air Conditioners (Floor 1, 2, 3) - Criticality: Operational
- 2 Printers (Floor 1, 2) - Criticality: Low
- 1 Fire Extinguisher (Floor 1) - Criticality: Safety
- 1 Water Dispenser (Floor 2) - Criticality: Low

**Annex Building:**
- 1 Elevator (Floor 1) - Criticality: Safety
- 2 Air Conditioners (Floor 1, 2) - Criticality: Operational
- 1 Printer (Floor 1) - Criticality: Low
- 1 Projector (Floor 2) - Criticality: Low

### Generate QR Codes
```
ASSET-MC-ELEV-01
ASSET-MC-ELEV-02
ASSET-MC-ELEV-03
ASSET-MC-AC-01
ASSET-MC-AC-02
ASSET-MC-AC-03
ASSET-MC-PRINT-01
ASSET-MC-PRINT-02
ASSET-MC-FIRE-01
ASSET-MC-WATER-01
ASSET-AB-ELEV-01
ASSET-AB-AC-01
ASSET-AB-AC-02
ASSET-AB-PRINT-01
ASSET-AB-PROJ-01
```

**Print these as QR codes for demo**

### Vendors (3 vendors)
```json
[
  {
    "name": "ACME Elevators",
    "contactName": "John Smith",
    "contactPhone": "+20123456789",
    "contactEmail": "john@acme-elevators.com"
  },
  {
    "name": "CoolAir HVAC",
    "contactName": "Sara Ahmed",
    "contactPhone": "+20123456790",
    "contactEmail": "sara@coolair.com"
  },
  {
    "name": "TechFix Services",
    "contactName": "Omar Hassan",
    "contactPhone": "+20123456791",
    "contactEmail": "omar@techfix.com"
  }
]
```

---

## 📅 Day-by-Day Parallel Work Plan

### Day 1 - Foundation
**Backend:**
- ✅ Set up ASP.NET Core project
- ✅ Configure Entity Framework with PostgreSQL/SQL Server
- ✅ Create database schema
- ✅ Seed sample data
- ✅ Implement `/auth/login`
- ✅ Implement `/users/me`
- ✅ Configure JWT authentication

**Flutter:**
- ✅ Create Flutter project
- ✅ Set up folder structure (`screens/`, `models/`, `services/`, `providers/`)
- ✅ Add dependencies to pubspec.yaml
- ✅ Create Login Screen UI
- ✅ Create AuthService (with mock data initially)
- ✅ Implement secure token storage
- ✅ Create Home Screen navigation

**End of Day 1 Test:**
- Flutter app can "login" with mock data
- Backend can authenticate and return JWT

---

### Day 2 - QR & Ticket Creation
**Backend:**
- ✅ Implement `GET /assets/{qrCode}`
- ✅ Implement `POST /tickets`
- ✅ Implement `POST /tickets/{id}/attachments`
- ✅ Set up file storage (Azure Blob / local)
- ✅ Implement SLA calculation logic
- ✅ Create TicketHistory entries

**Flutter:**
- ✅ Integrate real API endpoints (login, assets)
- ✅ Build QR Scanner Screen
- ✅ Build Asset Details Screen
- ✅ Build Create Ticket Screen
- ✅ Implement photo picker (camera/gallery)
- ✅ Implement file upload to backend

**Integration Test:**
- Scan QR code → See asset details
- Create ticket with photo → Ticket appears in backend

---

### Day 3 - Status Management
**Backend:**
- ✅ Implement `GET /tickets`
- ✅ Implement `GET /tickets/{id}`
- ✅ Implement `PUT /tickets/{id}/status`
- ✅ Implement `PUT /tickets/{id}/close`
- ✅ Implement ticket history logging

**Flutter:**
- ✅ Build Tickets List Screen
- ✅ Build Ticket Details Screen
- ✅ Implement status update UI (facilities only)
- ✅ Implement close ticket UI (coordinator only)
- ✅ Implement pull-to-refresh to see updates

**Integration Test:**
- Coordinator creates ticket → Appears in facilities list (after manual refresh)
- Facilities updates status → Coordinator sees update (after manual refresh)
- Coordinator closes ticket → History updates

---

### Day 4 - Dashboard & Polish
**Backend:**
- ✅ Implement `GET /dashboard/stats`
- ✅ Implement `GET /dashboard/buildings/performance`
- ✅ Implement `GET /buildings`
- ✅ Implement `GET /vendors`
- ✅ Fix bugs and optimize queries
- ✅ Add error handling

**Flutter:**
- ✅ Build Profile Screen
- ✅ Add pull-to-refresh on lists
- ✅ Add loading indicators
- ✅ Add error handling and retry
- ✅ Polish UI/UX
- ✅ Test on real devices

**Web Dashboard (if time permits):**
- ✅ Login page
- ✅ Dashboard with KPI cards
- ✅ Tickets table with filters

---

### Day 5 - Testing & Demo Prep
**All Teams:**
- ✅ End-to-end testing (full workflow)
- ✅ Fix critical bugs
- ✅ Deploy backend to test server
- ✅ Install Flutter app on 2-3 demo devices
- ✅ Print QR codes for demo assets
- ✅ Prepare demo accounts
- ✅ Create demo script
- ✅ Practice demo walkthrough (2-3 times)

---

## ✅ Demo Success Checklist

**Before Presentation:**
- [ ] Backend deployed and accessible
- [ ] Flutter app installed on demo devices
- [ ] QR codes printed and labeled
- [ ] Test accounts ready (coordinator1, facilities1, manager1)
- [ ] Pull-to-refresh working on ticket lists
- [ ] Internet connection confirmed
- [ ] Backup screen recordings ready

**Demo Accounts:**
```
coordinator1 / demo123
facilities1 / demo123
manager1 / demo123
```

**Demo Flow:**
1. Show login on mobile (coordinator)
2. Scan QR code (elevator)
3. Create critical priority ticket with photo attachment
4. Facilities logs in on web/mobile and pulls to refresh
5. Assign ticket and update status to "In Progress"
6. Coordinator pulls to refresh and sees the status update
7. Coordinator verifies repair on-site and closes ticket
8. Show management dashboard with updated KPIs

---

## 🚀 Communication & Coordination

### Daily Standup (15 min)
**Time:** Every morning at [TBD]
**Format:**
1. What did I complete yesterday?
2. What am I working on today?
3. Any blockers?

### Shared Resources
- **API Documentation:** [Swagger URL when ready]
- **Code Repository:** [GitHub/GitLab URL]
- **Team Chat:** [Slack/Discord/WhatsApp]
- **Figma/Design:** [If available]

### Backend → Flutter Handoff Points
- Day 1 End: Auth endpoints ready
- Day 2 Morning: Asset endpoint ready for QR scanning
- Day 2 Evening: Ticket creation endpoint ready
- Day 3 Morning: Ticket list endpoint ready
- Day 3 Evening: Status update endpoints ready

---

## 📚 Additional Resources

### Backend References
- [ASP.NET Core JWT Auth](https://docs.microsoft.com/en-us/aspnet/core/security/authentication)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Firebase Admin SDK .NET](https://firebase.google.com/docs/admin/setup#dotnet)

### Flutter References
- [Firebase Messaging Flutter](https://firebase.flutter.dev/docs/messaging/overview/)
- [Provider State Management](https://pub.dev/packages/provider)
- [Mobile Scanner Package](https://pub.dev/packages/mobile_scanner)

---

**Good luck! Focus on the core workflow and keep it simple for the demo.** 🎯
