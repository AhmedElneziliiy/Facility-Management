<#
  test-api.ps1  —  Facility Management API Test Suite
  Tests all 17 Swagger-visible endpoints with real seeded data.
  Run:  powershell -ExecutionPolicy Bypass -File .\test-api.ps1
  Compatible with Windows PowerShell 5.1+
#>

# ── Config ───────────────────────────────────────────────────────────────────
$BASE    = "http://localhost:5161"
$passed  = 0
$failed  = 0
$skipped = 0

# ── Seeded IDs ────────────────────────────────────────────────────────────────
$BUILDING_MAIN  = "10000000-0000-0000-0000-000000000001"   # Main Campus
$BUILDING_ANNEX = "10000000-0000-0000-0000-000000000002"   # Annex Building
$ASSET_ELEV_A   = "30000000-0000-0000-0000-000000000001"   # Elevator A  (Main Campus)
$ASSET_AC1      = "30000000-0000-0000-0000-000000000004"   # AC Unit 1   (Main Campus)
$ASSET_PRINTER1 = "30000000-0000-0000-0000-000000000007"   # Printer 1   (Main Campus)
$ASSET_ELEV_D   = "30000000-0000-0000-0000-000000000011"   # Elevator D  (Annex)
$VENDOR_ACME    = "40000000-0000-0000-0000-000000000001"   # ACME Elevators
$VENDOR_COOLAIR = "40000000-0000-0000-0000-000000000002"   # CoolAir HVAC
$TICKET_OPEN    = "50000000-0000-0000-0000-000000000001"   # TKT-2026-0001 open/critical
$TICKET_INPROG  = "50000000-0000-0000-0000-000000000002"   # TKT-2026-0002 in_progress
$TICKET_CLOSED  = "50000000-0000-0000-0000-000000000003"   # TKT-2026-0003 closed
$TICKET_OPEN2   = "50000000-0000-0000-0000-000000000004"   # TKT-2026-0004 open/low

# State shared across tests
$script:manToken   = $null
$script:coordToken = $null
$script:facToken   = $null
$script:newTicketId = $null
$script:page1Ids   = @()

# ── Helpers ───────────────────────────────────────────────────────────────────

function Section([string]$title) {
    Write-Host ""
    Write-Host ("=" * 60) -ForegroundColor DarkCyan
    Write-Host "  $title" -ForegroundColor Cyan
    Write-Host ("=" * 60) -ForegroundColor DarkCyan
}

function Pass([string]$label) {
    Write-Host "  [PASS] $label" -ForegroundColor Green
    $script:passed++
}

function Fail([string]$label, [string]$reason) {
    Write-Host "  [FAIL] $label" -ForegroundColor Red
    Write-Host "         => $reason" -ForegroundColor DarkRed
    $script:failed++
}

function Skip([string]$label, [string]$reason) {
    Write-Host "  [SKIP] $label ($reason)" -ForegroundColor Yellow
    $script:skipped++
}

# PS5-compatible HTTP helper: executes a web request and returns @{Status;Body}
# regardless of whether the server returns a 4xx/5xx error code.
function Exec-Request {
    param([hashtable]$Splat)
    try {
        $resp = Invoke-WebRequest @Splat -UseBasicParsing
        return @{ Status = [int]$resp.StatusCode; Body = ($resp.Content | ConvertFrom-Json -ErrorAction SilentlyContinue) }
    }
    catch [System.Net.WebException] {
        $webEx = $_.Exception
        if ($webEx.Response) {
            $stream  = $webEx.Response.GetResponseStream()
            $reader  = New-Object System.IO.StreamReader($stream)
            $content = $reader.ReadToEnd()
            $code    = [int]$webEx.Response.StatusCode
            $parsed  = $content | ConvertFrom-Json -ErrorAction SilentlyContinue
            return @{ Status = $code; Body = $parsed }
        }
        return @{ Status = 0; Body = $null; Error = $webEx.Message }
    }
    catch {
        return @{ Status = 0; Body = $null; Error = $_.Exception.Message }
    }
}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [object]$Body  = $null,
        [string]$Token = $null,
        [System.Collections.Hashtable]$Form = $null
    )

    $url     = "$BASE$Path"
    $headers = @{}
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }

    if ($Form) {
        # Multipart form upload
        $boundary = [System.Guid]::NewGuid().ToString()
        $CRLF     = "`r`n"
        $bodyList = New-Object System.Collections.Generic.List[byte]

        foreach ($key in $Form.Keys) {
            $val = $Form[$key]
            if ($val -is [System.IO.FileInfo]) {
                $fileBytes = [System.IO.File]::ReadAllBytes($val.FullName)
                # Determine MIME type from extension
                $ext = $val.Extension.ToLowerInvariant()
                $mime = switch ($ext) {
                    ".jpg"  { "image/jpeg" }
                    ".jpeg" { "image/jpeg" }
                    ".png"  { "image/png"  }
                    ".webp" { "image/webp" }
                    default { "image/jpeg" }
                }
                $partHeader = "--$boundary$CRLF" +
                              "Content-Disposition: form-data; name=`"$key`"; filename=`"$($val.Name)`"$CRLF" +
                              "Content-Type: $mime$CRLF$CRLF"
                $enc = [System.Text.Encoding]::UTF8
                foreach ($b in $enc.GetBytes($partHeader)) { $bodyList.Add($b) }
                foreach ($b in $fileBytes)                 { $bodyList.Add($b) }
                foreach ($b in $enc.GetBytes($CRLF))       { $bodyList.Add($b) }
            }
        }
        $enc2 = [System.Text.Encoding]::UTF8
        foreach ($b in $enc2.GetBytes("--$boundary--$CRLF")) { $bodyList.Add($b) }

        return Exec-Request -Splat @{
            Uri         = $url
            Method      = $Method
            Headers     = $headers
            Body        = $bodyList.ToArray()
            ContentType = "multipart/form-data; boundary=$boundary"
        }
    }
    elseif ($Body) {
        $json = $Body | ConvertTo-Json -Depth 10
        return Exec-Request -Splat @{
            Uri         = $url
            Method      = $Method
            Headers     = $headers
            Body        = $json
            ContentType = "application/json"
        }
    }
    else {
        return Exec-Request -Splat @{
            Uri     = $url
            Method  = $Method
            Headers = $headers
        }
    }
}

# ── Pre-flight check ──────────────────────────────────────────────────────────
Section "PRE-FLIGHT: Checking API is reachable"

$ping = Invoke-Api -Method GET -Path "/swagger/v1/swagger.json"
if ($ping.Status -eq 200) {
    Pass "API is reachable at $BASE"
} else {
    Write-Host ""
    Write-Host "  FATAL: Cannot reach $BASE (status=$($ping.Status))" -ForegroundColor Red
    Write-Host "  Make sure the app is running:  dotnet run --project AssetManagement.Web" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

# ── GROUP 1: Auth ─────────────────────────────────────────────────────────────
Section "GROUP 1 — Auth"

# 1.1 Valid manager login
$r = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{ username="manager1"; password="Pass@123" }
if ($r.Status -eq 200 -and $r.Body.success -eq $true -and $r.Body.data.token) {
    $script:manToken = $r.Body.data.token
    Pass "1.1  Valid manager login -> token acquired, role=$($r.Body.data.user.role)"
} else {
    Fail "1.1  Valid manager login" "status=$($r.Status) success=$($r.Body.success)"
}

# 1.2 Valid coordinator login
$r = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{ username="coordinator1"; password="Pass@123" }
if ($r.Status -eq 200 -and $r.Body.success -eq $true -and $r.Body.data.token) {
    $script:coordToken = $r.Body.data.token
    Pass "1.2  Valid coordinator login -> token acquired"
} else {
    Fail "1.2  Valid coordinator login" "status=$($r.Status)"
}

# 1.3 Valid facilities login
$r = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{ username="facilities1"; password="Pass@123" }
if ($r.Status -eq 200 -and $r.Body.success -eq $true -and $r.Body.data.token) {
    $script:facToken = $r.Body.data.token
    Pass "1.3  Valid facilities login -> token acquired"
} else {
    Fail "1.3  Valid facilities login" "status=$($r.Status)"
}

# 1.4 Wrong password -> 401
$r = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{ username="manager1"; password="wrongpass" }
if ($r.Status -eq 401 -or ($r.Status -eq 200 -and $r.Body.success -eq $false)) {
    Pass "1.4  Wrong password -> rejected (status=$($r.Status))"
} else {
    Fail "1.4  Wrong password should be rejected" "got status=$($r.Status)"
}

# 1.5 Unknown username -> 401
$r = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{ username="nobody"; password="Pass@123" }
if ($r.Status -eq 401 -or ($r.Status -eq 200 -and $r.Body.success -eq $false)) {
    Pass "1.5  Unknown username -> rejected"
} else {
    Fail "1.5  Unknown username should be rejected" "got status=$($r.Status)"
}

# 1.6 Missing fields -> 400
$r = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{}
if ($r.Status -eq 400 -or ($r.Status -eq 200 -and $r.Body.success -eq $false)) {
    Pass "1.6  Missing fields -> validation error"
} else {
    Fail "1.6  Missing fields should fail validation" "got status=$($r.Status)"
}

# 1.7 GET /api/auth/me with valid token
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/auth/me" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.username -eq "manager1" -and $r.Body.data.role -eq "manager") {
        Pass "1.7  GET /api/auth/me -> username=manager1, role=manager"
    } else {
        Fail "1.7  GET /api/auth/me" "status=$($r.Status) username=$($r.Body.data.username) role=$($r.Body.data.role)"
    }
} else { Skip "1.7  GET /api/auth/me" "no manager token" }

# 1.8 GET /api/auth/me without token -> 401
$r = Invoke-Api -Method GET -Path "/api/auth/me"
if ($r.Status -eq 401) {
    Pass "1.8  GET /api/auth/me (no token) -> 401"
} else {
    Fail "1.8  No-token me should return 401" "got status=$($r.Status)"
}

# ── GROUP 2: Assets ───────────────────────────────────────────────────────────
Section "GROUP 2 — Assets"

if (-not $script:manToken) {
    Write-Host "  [WARN] No manager token — skipping Group 2" -ForegroundColor Yellow
    $script:skipped += 9
} else {

    # 2.1 Valid QR code
    $r = Invoke-Api -Method GET -Path "/api/assets/ASSET-MC-ELEV-01" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.name -eq "Elevator A" -and $r.Body.data.buildingName -eq "Main Campus") {
        Pass "2.1  GET /api/assets/ASSET-MC-ELEV-01 -> Elevator A, Main Campus"
    } else {
        Fail "2.1  QR code lookup" "status=$($r.Status) name=$($r.Body.data.name)"
    }

    # 2.2 Case-insensitive QR
    $r = Invoke-Api -Method GET -Path "/api/assets/asset-mc-elev-01" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.name -eq "Elevator A") {
        Pass "2.2  Lowercase QR code -> same asset (case-insensitive)"
    } else {
        Fail "2.2  Lowercase QR lookup" "status=$($r.Status)"
    }

    # 2.3 Annex asset
    $r = Invoke-Api -Method GET -Path "/api/assets/ASSET-AB-ELEV-01" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.buildingName -like "*Annex*") {
        Pass "2.3  Annex asset -> buildingName='$($r.Body.data.buildingName)'"
    } else {
        Fail "2.3  Annex asset QR lookup" "status=$($r.Status) buildingName=$($r.Body.data.buildingName)"
    }

    # 2.4 Non-existent QR -> 404
    $r = Invoke-Api -Method GET -Path "/api/assets/ASSET-FAKE-999" -Token $script:manToken
    if ($r.Status -eq 404) {
        Pass "2.4  Non-existent QR -> 404"
    } else {
        Fail "2.4  Non-existent QR should return 404" "got status=$($r.Status)"
    }

    # 2.5 No token -> 401
    $r = Invoke-Api -Method GET -Path "/api/assets/ASSET-MC-ELEV-01"
    if ($r.Status -eq 401) {
        Pass "2.5  No token -> 401"
    } else {
        Fail "2.5  No-token asset lookup should return 401" "got status=$($r.Status)"
    }

    # 2.6 List Main Campus assets
    $r = Invoke-Api -Method GET -Path "/api/assets?buildingId=$BUILDING_MAIN" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.Count -ge 10) {
        $allMain = ($r.Body.data | Where-Object { $_.buildingName -ne "Main Campus" }).Count -eq 0
        if ($allMain) {
            Pass "2.6  Main Campus assets -> $($r.Body.data.Count) assets, all in Main Campus"
        } else {
            Fail "2.6  Main Campus assets" "some assets have wrong buildingName"
        }
    } else {
        Fail "2.6  Main Campus assets list" "status=$($r.Status) count=$($r.Body.data.Count)"
    }

    # 2.7 List Annex assets
    $r = Invoke-Api -Method GET -Path "/api/assets?buildingId=$BUILDING_ANNEX" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.Count -ge 5) {
        Pass "2.7  Annex assets -> $($r.Body.data.Count) assets"
    } else {
        Fail "2.7  Annex assets list" "status=$($r.Status) count=$($r.Body.data.Count)"
    }

    # 2.8 Non-existent building -> empty array
    $fakeGuid = "99999999-9999-9999-9999-999999999999"
    $r = Invoke-Api -Method GET -Path "/api/assets?buildingId=$fakeGuid" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.Count -eq 0) {
        Pass "2.8  Non-existent building -> empty array"
    } else {
        Fail "2.8  Non-existent building assets" "status=$($r.Status) count=$($r.Body.data.Count)"
    }

    # 2.9 Missing buildingId -> 400
    $r = Invoke-Api -Method GET -Path "/api/assets" -Token $script:manToken
    if ($r.Status -eq 400) {
        Pass "2.9  Missing buildingId -> 400"
    } else {
        Fail "2.9  Missing buildingId should return 400" "got status=$($r.Status)"
    }
}

# ── GROUP 3: Tickets ──────────────────────────────────────────────────────────
Section "GROUP 3 — Tickets"

# 3.1 Create normal ticket (coordinator)
if ($script:coordToken) {
    $r = Invoke-Api -Method POST -Path "/api/tickets" -Token $script:coordToken -Body @{
        assetId     = $ASSET_AC1
        title       = "Test ticket - AC Unit 1 filter check"
        description = "Routine filter check required. Created by automated test script."
        priority    = "normal"
    }
    if ($r.Status -eq 201 -and $r.Body.success -eq $true -and $r.Body.data.id -and $r.Body.data.ticketNumber -like "TKT-*") {
        $script:newTicketId = $r.Body.data.id
        Pass "3.1  Create ticket -> id=$($script:newTicketId), number=$($r.Body.data.ticketNumber), status=$($r.Body.data.status)"
    } else {
        Fail "3.1  Create normal ticket" "status=$($r.Status) message=$($r.Body.message)"
    }
} else { Skip "3.1  Create normal ticket" "no coordinator token" }

# 3.2 Create critical ticket (manager)
if ($script:manToken) {
    $r = Invoke-Api -Method POST -Path "/api/tickets" -Token $script:manToken -Body @{
        assetId     = $ASSET_ELEV_A
        title       = "Critical test - Elevator A emergency check"
        description = "Automated test for critical priority SLA verification."
        priority    = "critical"
    }
    if ($r.Status -eq 201 -and $r.Body.data.priority -eq "critical" -and $r.Body.data.slaHours -eq 4) {
        Pass "3.2  Create critical ticket -> priority=critical, slaHours=$($r.Body.data.slaHours)"
    } else {
        Fail "3.2  Create critical ticket" "status=$($r.Status) priority=$($r.Body.data.priority) slaHours=$($r.Body.data.slaHours)"
    }
} else { Skip "3.2  Create critical ticket" "no manager token" }

# 3.3 Missing title -> 400
if ($script:coordToken) {
    $r = Invoke-Api -Method POST -Path "/api/tickets" -Token $script:coordToken -Body @{
        assetId     = $ASSET_AC1
        description = "No title provided"
    }
    if ($r.Status -eq 400) {
        Pass "3.3  Missing title -> 400"
    } else {
        Fail "3.3  Missing title should fail" "got status=$($r.Status)"
    }
} else { Skip "3.3" "no coordinator token" }

# 3.4 Missing description -> 400
if ($script:coordToken) {
    $r = Invoke-Api -Method POST -Path "/api/tickets" -Token $script:coordToken -Body @{
        assetId = $ASSET_AC1
        title   = "No description"
    }
    if ($r.Status -eq 400) {
        Pass "3.4  Missing description -> 400"
    } else {
        Fail "3.4  Missing description should fail" "got status=$($r.Status)"
    }
} else { Skip "3.4" "no coordinator token" }

# 3.5 Invalid assetId (null GUID) -> 400 or 404
if ($script:coordToken) {
    $r = Invoke-Api -Method POST -Path "/api/tickets" -Token $script:coordToken -Body @{
        assetId     = "00000000-0000-0000-0000-000000000000"
        title       = "Bad asset test"
        description = "Should fail"
    }
    if ($r.Status -eq 400 -or $r.Status -eq 404) {
        Pass "3.5  Invalid assetId -> $($r.Status)"
    } else {
        Fail "3.5  Invalid assetId should return 400/404" "got status=$($r.Status)"
    }
} else { Skip "3.5" "no coordinator token" }

# 3.6 No token -> 401
$r = Invoke-Api -Method POST -Path "/api/tickets" -Body @{
    assetId = $ASSET_AC1; title = "unauth"; description = "unauth"
}
if ($r.Status -eq 401) {
    Pass "3.6  No token -> 401"
} else {
    Fail "3.6  No-token create should return 401" "got status=$($r.Status)"
}

# 3.7 List all tickets
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/tickets" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.Count -ge 10 -and $r.Body.meta.total -ge 10) {
        Pass "3.7  List all tickets -> count=$($r.Body.data.Count), total=$($r.Body.meta.total)"
    } else {
        Fail "3.7  List tickets" "status=$($r.Status) count=$($r.Body.data.Count) total=$($r.Body.meta.total)"
    }
} else { Skip "3.7" "no manager token" }

# 3.8 Filter by status=open
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/tickets?status=open" -Token $script:manToken
    $wrongStatus = ($r.Body.data | Where-Object { $_.status -ne "open" }).Count
    if ($r.Status -eq 200 -and $r.Body.data.Count -ge 1 -and $wrongStatus -eq 0) {
        Pass "3.8  Filter status=open -> $($r.Body.data.Count) tickets, all open"
    } else {
        Fail "3.8  Filter by status=open" "status=$($r.Status) count=$($r.Body.data.Count) wrongStatus=$wrongStatus"
    }
} else { Skip "3.8" "no token" }

# 3.9 Filter by status=closed
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/tickets?status=closed" -Token $script:manToken
    $wrongStatus = ($r.Body.data | Where-Object { $_.status -ne "closed" }).Count
    if ($r.Status -eq 200 -and $r.Body.data.Count -ge 3 -and $wrongStatus -eq 0) {
        Pass "3.9  Filter status=closed -> $($r.Body.data.Count) closed tickets"
    } else {
        Fail "3.9  Filter by status=closed" "status=$($r.Status) count=$($r.Body.data.Count) wrongStatus=$wrongStatus"
    }
} else { Skip "3.9" "no token" }

# 3.10 Filter by priority=critical
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/tickets?priority=critical" -Token $script:manToken
    $wrongPrio = ($r.Body.data | Where-Object { $_.priority -ne "critical" }).Count
    if ($r.Status -eq 200 -and $r.Body.data.Count -ge 2 -and $wrongPrio -eq 0) {
        Pass "3.10 Filter priority=critical -> $($r.Body.data.Count) critical tickets"
    } else {
        Fail "3.10 Filter by priority=critical" "count=$($r.Body.data.Count) wrongPrio=$wrongPrio"
    }
} else { Skip "3.10" "no token" }

# 3.11 Filter by buildingId=Main Campus
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/tickets?buildingId=$BUILDING_MAIN" -Token $script:manToken
    $wrongBuilding = ($r.Body.data | Where-Object { $_.buildingName -ne "Main Campus" }).Count
    if ($r.Status -eq 200 -and $r.Body.data.Count -ge 1 -and $wrongBuilding -eq 0) {
        Pass "3.11 Filter buildingId=Main Campus -> $($r.Body.data.Count) tickets"
    } else {
        Fail "3.11 Filter by buildingId" "count=$($r.Body.data.Count) wrongBuilding=$wrongBuilding"
    }
} else { Skip "3.11" "no token" }

# 3.12 Pagination page=1 pageSize=3
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/tickets?page=1&pageSize=3" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.Count -eq 3 -and $r.Body.meta.page -eq 1 -and $r.Body.meta.pageSize -eq 3) {
        $script:page1Ids = $r.Body.data | Select-Object -ExpandProperty id
        Pass "3.12 Pagination page=1 pageSize=3 -> 3 items, page=1"
    } else {
        Fail "3.12 Pagination" "status=$($r.Status) count=$($r.Body.data.Count) page=$($r.Body.meta.page)"
    }
} else { Skip "3.12" "no token" }

# 3.13 Pagination page=2 pageSize=3 — different IDs
if ($script:manToken -and $script:page1Ids.Count -gt 0) {
    $r = Invoke-Api -Method GET -Path "/api/tickets?page=2&pageSize=3" -Token $script:manToken
    $page2Ids  = $r.Body.data | Select-Object -ExpandProperty id
    $overlap   = ($page2Ids | Where-Object { $script:page1Ids -contains $_ }).Count
    if ($r.Status -eq 200 -and $r.Body.data.Count -ge 1 -and $r.Body.meta.page -eq 2 -and $overlap -eq 0) {
        Pass "3.13 Pagination page=2 -> $($r.Body.data.Count) items, no overlap with page 1"
    } else {
        Fail "3.13 Pagination page 2" "status=$($r.Status) count=$($r.Body.data.Count) overlap=$overlap"
    }
} else { Skip "3.13" "page 1 data unavailable" }

# 3.14 GET ticket by ID (seeded open ticket)
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/tickets/$TICKET_OPEN" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.ticketNumber -eq "TKT-2026-0001" -and $r.Body.data.history -ne $null -and $r.Body.data.assetSnapshot -ne $null) {
        Pass "3.14 GET ticket/$TICKET_OPEN -> TKT-2026-0001, has history + assetSnapshot"
    } else {
        Fail "3.14 GET ticket by ID" "status=$($r.Status) number=$($r.Body.data.ticketNumber)"
    }
} else { Skip "3.14" "no token" }

# 3.15 GET closed ticket — has resolutionNotes
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/tickets/$TICKET_CLOSED" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.status -eq "closed" -and $r.Body.data.resolutionNotes) {
        Pass "3.15 GET closed ticket -> status=closed, resolutionNotes present, cost=$($r.Body.data.actualCost)"
    } else {
        Fail "3.15 GET closed ticket" "status=$($r.Status) ticketStatus=$($r.Body.data.status)"
    }
} else { Skip "3.15" "no token" }

# 3.16 GET newly created ticket
if ($script:manToken -and $script:newTicketId) {
    $r = Invoke-Api -Method GET -Path "/api/tickets/$($script:newTicketId)" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.status -eq "open") {
        Pass "3.16 GET newly created ticket -> status=open"
    } else {
        Fail "3.16 GET new ticket" "status=$($r.Status) ticketStatus=$($r.Body.data.status)"
    }
} else { Skip "3.16" "no new ticket ID" }

# 3.17 GET non-existent ticket -> 404
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/tickets/99999999-9999-9999-9999-999999999999" -Token $script:manToken
    if ($r.Status -eq 404) {
        Pass "3.17 GET non-existent ticket -> 404"
    } else {
        Fail "3.17 Non-existent ticket should return 404" "got status=$($r.Status)"
    }
} else { Skip "3.17" "no token" }

# 3.18 Update status to in_progress (facilities) — uses newly created ticket
if ($script:facToken -and $script:newTicketId) {
    $r = Invoke-Api -Method PUT -Path "/api/tickets/$($script:newTicketId)/status" -Token $script:facToken -Body @{
        status           = "in_progress"
        assignedVendorId = $VENDOR_COOLAIR
    }
    if ($r.Status -eq 200 -and $r.Body.data.status -eq "in_progress") {
        Pass "3.18 Update status -> in_progress (facilities token), vendor assigned"
    } else {
        Fail "3.18 Update ticket status" "status=$($r.Status) ticketStatus=$($r.Body.data.status) msg=$($r.Body.message)"
    }
} else { Skip "3.18" "missing facilities token or new ticket ID" }

# 3.19 Coordinator denied for status update -> 403
if ($script:coordToken -and $script:newTicketId) {
    $r = Invoke-Api -Method PUT -Path "/api/tickets/$($script:newTicketId)/status" -Token $script:coordToken -Body @{
        status = "open"
    }
    if ($r.Status -eq 403) {
        Pass "3.19 Coordinator -> 403 on status update (role-based restriction)"
    } else {
        Fail "3.19 Coordinator should be denied status update" "got status=$($r.Status)"
    }
} else { Skip "3.19" "missing tokens" }

# 3.20 Invalid status value -> 400
if ($script:facToken -and $script:newTicketId) {
    $r = Invoke-Api -Method PUT -Path "/api/tickets/$($script:newTicketId)/status" -Token $script:facToken -Body @{
        status = "banana"
    }
    if ($r.Status -eq 400) {
        Pass "3.20 Invalid status 'banana' -> 400"
    } else {
        Fail "3.20 Invalid status should return 400" "got status=$($r.Status)"
    }
} else { Skip "3.20" "missing tokens" }

# 3.21 No token on status update -> 401
$r = Invoke-Api -Method PUT -Path "/api/tickets/$TICKET_OPEN/status" -Body @{ status = "in_progress" }
if ($r.Status -eq 401) {
    Pass "3.21 No token on status update -> 401"
} else {
    Fail "3.21 No-token status update should return 401" "got status=$($r.Status)"
}

# 3.22 Close ticket (coordinator)
if ($script:coordToken -and $script:newTicketId) {
    $r = Invoke-Api -Method PUT -Path "/api/tickets/$($script:newTicketId)/close" -Token $script:coordToken -Body @{
        resolutionNotes = "Fixed and tested. Automated test script close."
        actualCost      = 150
    }
    if ($r.Status -eq 200 -and $r.Body.data.status -eq "closed" -and $r.Body.data.closedAt -ne $null -and $r.Body.data.actualCost -eq 150) {
        Pass "3.22 Close ticket -> status=closed, closedAt set, actualCost=150"
    } else {
        Fail "3.22 Close ticket" "status=$($r.Status) ticketStatus=$($r.Body.data.status) cost=$($r.Body.data.actualCost) msg=$($r.Body.message)"
    }
} else { Skip "3.22" "missing coordinator token or new ticket ID" }

# 3.23 Facilities denied on close -> 403
if ($script:facToken -and $script:newTicketId) {
    $r = Invoke-Api -Method PUT -Path "/api/tickets/$($script:newTicketId)/close" -Token $script:facToken -Body @{
        resolutionNotes = "Attempting close as facilities"
    }
    if ($r.Status -eq 403) {
        Pass "3.23 Facilities -> 403 on close (role-based restriction)"
    } else {
        Fail "3.23 Facilities should be denied close" "got status=$($r.Status)"
    }
} else { Skip "3.23" "missing tokens" }

# 3.24 Missing resolutionNotes -> 400
if ($script:coordToken -and $script:newTicketId) {
    $r = Invoke-Api -Method PUT -Path "/api/tickets/$($script:newTicketId)/close" -Token $script:coordToken -Body @{}
    if ($r.Status -eq 400) {
        Pass "3.24 Missing resolutionNotes -> 400"
    } else {
        Fail "3.24 Missing resolutionNotes should fail" "got status=$($r.Status)"
    }
} else { Skip "3.24" "missing tokens" }

# 3.25 Close already-closed ticket -> 400
if ($script:coordToken) {
    $r = Invoke-Api -Method PUT -Path "/api/tickets/$TICKET_CLOSED/close" -Token $script:coordToken -Body @{
        resolutionNotes = "Already closed"
    }
    if ($r.Status -eq 400) {
        Pass "3.25 Close already-closed ticket -> 400"
    } else {
        Fail "3.25 Already-closed ticket should return 400" "got status=$($r.Status) msg=$($r.Body.message)"
    }
} else { Skip "3.25" "no coordinator token" }

# 3.26 Upload attachment (multipart) — write a minimal valid JPEG (SOI + APP0 + EOI)
if ($script:coordToken -and $script:newTicketId) {
    $tmpFile = [System.IO.Path]::Combine($env:TEMP, "test-attach-api.jpg")
    # Minimal JFIF-compliant JPEG: SOI(FFD8) + APP0 marker + EOI(FFD9)
    $jpegBytes = [byte[]](
        0xFF,0xD8,                          # SOI
        0xFF,0xE0,0x00,0x10,                # APP0 marker + length=16
        0x4A,0x46,0x49,0x46,0x00,           # "JFIF\0"
        0x01,0x01,                           # version 1.1
        0x00,                                # aspect ratio units: 0=none
        0x00,0x01,0x00,0x01,                 # Xdensity=1, Ydensity=1
        0x00,0x00,                           # thumbnail size 0x0
        0xFF,0xD9                             # EOI
    )
    [System.IO.File]::WriteAllBytes($tmpFile, $jpegBytes)
    $fileInfo = [System.IO.FileInfo]$tmpFile

    $r = Invoke-Api -Method POST -Path "/api/tickets/$($script:newTicketId)/attachments" `
                    -Token $script:coordToken -Form @{ file = $fileInfo }

    Remove-Item $tmpFile -ErrorAction SilentlyContinue

    if ($r.Status -eq 200 -or $r.Status -eq 201) {
        if ($r.Body.data.filename -and $r.Body.data.sizeBytes -gt 0) {
            Pass "3.26 Upload attachment -> filename=$($r.Body.data.filename), size=$($r.Body.data.sizeBytes)"
        } else {
            Fail "3.26 Upload attachment" "status=$($r.Status) body=$($r.Body | ConvertTo-Json -Depth 2 -Compress)"
        }
    } else {
        Fail "3.26 Upload attachment" "status=$($r.Status) msg=$($r.Body.message)"
    }
} else { Skip "3.26" "missing coordinator token or new ticket ID" }

# 3.27 No file -> 400 (send empty multipart, not JSON)
if ($script:coordToken -and $script:newTicketId) {
    # Send a proper multipart/form-data with no file part — server should return 400
    $boundary = [System.Guid]::NewGuid().ToString()
    $emptyBody = [System.Text.Encoding]::UTF8.GetBytes("--$boundary--`r`n")
    $r = Exec-Request -Splat @{
        Uri         = "$BASE/api/tickets/$($script:newTicketId)/attachments"
        Method      = "POST"
        Headers     = @{ Authorization = "Bearer $($script:coordToken)" }
        Body        = $emptyBody
        ContentType = "multipart/form-data; boundary=$boundary"
    }
    if ($r.Status -eq 400) {
        Pass "3.27 No file uploaded -> 400"
    } else {
        Fail "3.27 No file should return 400" "got status=$($r.Status) msg=$($r.Body.message)"
    }
} else { Skip "3.27" "missing tokens" }

# ── GROUP 4: Dashboard ────────────────────────────────────────────────────────
Section "GROUP 4 — Dashboard"

# 4.1 Overall stats
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/dashboard/stats" -Token $script:manToken
    $d = $r.Body.data
    if ($r.Status -eq 200 -and $d.totalAssets -ge 15 -and $d.totalTickets -ge 10 -and $d.last7DaysTrend.Count -eq 7) {
        Pass "4.1  Overall stats -> assets=$($d.totalAssets), tickets=$($d.totalTickets), open=$($d.openTickets), 7-day trend present"
    } else {
        Fail "4.1  Dashboard stats" "status=$($r.Status) assets=$($d.totalAssets) tickets=$($d.totalTickets) trend=$($d.last7DaysTrend.Count)"
    }
} else { Skip "4.1" "no token" }

# 4.2 Stats filtered by building
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/dashboard/stats?buildingId=$BUILDING_MAIN" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.totalAssets -ge 1) {
        Pass "4.2  Dashboard stats by building -> assets=$($r.Body.data.totalAssets)"
    } else {
        Fail "4.2  Dashboard stats by building" "status=$($r.Status) assets=$($r.Body.data.totalAssets)"
    }
} else { Skip "4.2" "no token" }

# 4.3 Invalid buildingId format -> 400
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/dashboard/stats?buildingId=not-a-guid" -Token $script:manToken
    if ($r.Status -eq 400) {
        Pass "4.3  Invalid buildingId -> 400"
    } else {
        Fail "4.3  Invalid buildingId should return 400" "got status=$($r.Status)"
    }
} else { Skip "4.3" "no token" }

# 4.4 Not authenticated -> 401
$r = Invoke-Api -Method GET -Path "/api/dashboard/stats"
if ($r.Status -eq 401) {
    Pass "4.4  No token -> 401"
} else {
    Fail "4.4  No-token dashboard should return 401" "got status=$($r.Status)"
}

# 4.5 Building performance
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/dashboard/buildings/performance" -Token $script:manToken
    $hasFields = $r.Body.data.Count -ge 2 -and $r.Body.data[0].buildingId -and $r.Body.data[0].buildingName
    if ($r.Status -eq 200 -and $hasFields) {
        Pass "4.5  Building performance -> $($r.Body.data.Count) buildings, fields present"
    } else {
        Fail "4.5  Building performance" "status=$($r.Status) count=$($r.Body.data.Count)"
    }
} else { Skip "4.5" "no token" }

# ── GROUP 5: Buildings ────────────────────────────────────────────────────────
Section "GROUP 5 — Buildings"

# 5.1 List all buildings
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/buildings" -Token $script:manToken
    $names = $r.Body.data | Select-Object -ExpandProperty name
    if ($r.Status -eq 200 -and $r.Body.data.Count -ge 2 -and ($names -contains "Main Campus") -and ($names -like "*Annex*")) {
        Pass "5.1  List buildings -> $($r.Body.data.Count) buildings (Main Campus + Annex)"
    } else {
        Fail "5.1  List buildings" "status=$($r.Status) count=$($r.Body.data.Count) names=$($names -join ', ')"
    }
} else { Skip "5.1" "no token" }

# 5.2 No token -> 401
$r = Invoke-Api -Method GET -Path "/api/buildings"
if ($r.Status -eq 401) {
    Pass "5.2  No token -> 401"
} else {
    Fail "5.2  No-token buildings should return 401" "got status=$($r.Status)"
}

# 5.3 Main Campus floors
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/buildings/$BUILDING_MAIN/floors" -Token $script:manToken
    $levels = $r.Body.data | Select-Object -ExpandProperty level
    if ($r.Status -eq 200 -and $r.Body.data.Count -eq 3 -and ($levels -contains 1) -and ($levels -contains 2) -and ($levels -contains 3)) {
        Pass "5.3  Main Campus floors -> 3 floors, levels 1,2,3"
    } else {
        Fail "5.3  Main Campus floors" "status=$($r.Status) count=$($r.Body.data.Count) levels=$($levels -join ',')"
    }
} else { Skip "5.3" "no token" }

# 5.4 Annex floors
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/buildings/$BUILDING_ANNEX/floors" -Token $script:manToken
    $levels = $r.Body.data | Select-Object -ExpandProperty level
    if ($r.Status -eq 200 -and $r.Body.data.Count -eq 2) {
        Pass "5.4  Annex floors -> 2 floors (levels: $($levels -join ','))"
    } else {
        Fail "5.4  Annex floors" "status=$($r.Status) count=$($r.Body.data.Count)"
    }
} else { Skip "5.4" "no token" }

# 5.5 Non-existent building floors
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/buildings/99999999-9999-9999-9999-999999999999/floors" -Token $script:manToken
    if ($r.Status -eq 200 -or $r.Status -eq 404) {
        Pass "5.5  Non-existent building -> $($r.Status) (empty or 404 acceptable)"
    } else {
        Fail "5.5  Non-existent building floors" "got status=$($r.Status)"
    }
} else { Skip "5.5" "no token" }

# ── GROUP 6: Vendors ──────────────────────────────────────────────────────────
Section "GROUP 6 — Vendors"

# 6.1 List all vendors
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/vendors" -Token $script:manToken
    $hasFields = $r.Body.data[0].id -and $r.Body.data[0].name -and $r.Body.data[0].contactName
    if ($r.Status -eq 200 -and $r.Body.data.Count -ge 3 -and $hasFields) {
        Pass "6.1  List vendors -> $($r.Body.data.Count) vendors with id/name/contactName"
    } else {
        Fail "6.1  List vendors" "status=$($r.Status) count=$($r.Body.data.Count)"
    }
} else { Skip "6.1" "no token" }

# 6.2 No token -> 401
$r = Invoke-Api -Method GET -Path "/api/vendors"
if ($r.Status -eq 401) {
    Pass "6.2  No token -> 401"
} else {
    Fail "6.2  No-token vendors should return 401" "got status=$($r.Status)"
}

# ── GROUP 7: Users ────────────────────────────────────────────────────────────
Section "GROUP 7 — Users"

# 7.1 Manager /me
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/users/me" -Token $script:manToken
    if ($r.Status -eq 200 -and $r.Body.data.username -eq "manager1" -and $r.Body.data.role -eq "manager") {
        Pass "7.1  GET /api/users/me (manager) -> username=manager1, role=manager"
    } else {
        Fail "7.1  Users/me (manager)" "status=$($r.Status) username=$($r.Body.data.username)"
    }
} else { Skip "7.1" "no token" }

# 7.2 Coordinator /me
if ($script:coordToken) {
    $r = Invoke-Api -Method GET -Path "/api/users/me" -Token $script:coordToken
    if ($r.Status -eq 200 -and $r.Body.data.role -eq "coordinator") {
        Pass "7.2  GET /api/users/me (coordinator) -> role=coordinator"
    } else {
        Fail "7.2  Users/me (coordinator)" "status=$($r.Status) role=$($r.Body.data.role)"
    }
} else { Skip "7.2" "no token" }

# 7.3 No token -> 401
$r = Invoke-Api -Method GET -Path "/api/users/me"
if ($r.Status -eq 401) {
    Pass "7.3  No token -> 401"
} else {
    Fail "7.3  No-token users/me should return 401" "got status=$($r.Status)"
}

# 7.4 Facilities staff list
if ($script:manToken) {
    $r = Invoke-Api -Method GET -Path "/api/users/facilities" -Token $script:manToken
    $nonFacilities = ($r.Body.data | Where-Object { $_.role -ne "facilities" }).Count
    if ($r.Status -eq 200 -and $r.Body.data.Count -ge 2 -and $nonFacilities -eq 0) {
        Pass "7.4  GET /api/users/facilities -> $($r.Body.data.Count) staff, all role=facilities"
    } else {
        Fail "7.4  Facilities staff list" "status=$($r.Status) count=$($r.Body.data.Count) nonFacilities=$nonFacilities"
    }
} else { Skip "7.4" "no token" }

# 7.5 No token on facilities -> 401
$r = Invoke-Api -Method GET -Path "/api/users/facilities"
if ($r.Status -eq 401) {
    Pass "7.5  No token -> 401"
} else {
    Fail "7.5  No-token facilities list should return 401" "got status=$($r.Status)"
}

# ── SUMMARY ───────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host ("=" * 60) -ForegroundColor DarkCyan
Write-Host "  TEST SUMMARY" -ForegroundColor Cyan
Write-Host ("=" * 60) -ForegroundColor DarkCyan

$total = $script:passed + $script:failed + $script:skipped
Write-Host ""
Write-Host ("  PASSED : {0,3}" -f $script:passed)  -ForegroundColor Green
Write-Host ("  FAILED : {0,3}" -f $script:failed)  -ForegroundColor Red
Write-Host ("  SKIPPED: {0,3}" -f $script:skipped) -ForegroundColor Yellow
Write-Host ("  TOTAL  : {0,3}" -f $total)           -ForegroundColor White
Write-Host ""

if ($script:failed -eq 0) {
    Write-Host "  ALL TESTS PASSED" -ForegroundColor Green
} else {
    Write-Host "  $($script:failed) TEST(S) FAILED" -ForegroundColor Red
}
Write-Host ""
