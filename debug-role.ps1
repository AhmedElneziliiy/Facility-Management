$loginR = Invoke-WebRequest -Uri "http://localhost:5161/api/auth/login" -Method POST -Body '{"username":"facilities1","password":"Pass@123"}' -ContentType "application/json" -UseBasicParsing
$facToken = ($loginR.Content | ConvertFrom-Json).data.token
Write-Host "Got facilities token"

$body = '{"status":"in_progress"}'
try {
    $r = Invoke-WebRequest -Uri "http://localhost:5161/api/tickets/50000000-0000-0000-0000-000000000001/status" -Method PUT -Headers @{ Authorization="Bearer $facToken" } -Body $body -ContentType "application/json" -UseBasicParsing
    Write-Host "SUCCESS status=$($r.StatusCode)"
    Write-Host $r.Content
} catch [System.Net.WebException] {
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    Write-Host "FAIL status=$([int]$_.Exception.Response.StatusCode)"
    Write-Host $reader.ReadToEnd()
}

# Also test with manager
$loginR2 = Invoke-WebRequest -Uri "http://localhost:5161/api/auth/login" -Method POST -Body '{"username":"manager1","password":"Pass@123"}' -ContentType "application/json" -UseBasicParsing
$manToken = ($loginR2.Content | ConvertFrom-Json).data.token
Write-Host "`nGot manager token"

try {
    $r2 = Invoke-WebRequest -Uri "http://localhost:5161/api/tickets/50000000-0000-0000-0000-000000000001/status" -Method PUT -Headers @{ Authorization="Bearer $manToken" } -Body $body -ContentType "application/json" -UseBasicParsing
    Write-Host "SUCCESS status=$($r2.StatusCode)"
    Write-Host $r2.Content
} catch [System.Net.WebException] {
    $stream2 = $_.Exception.Response.GetResponseStream()
    $reader2 = New-Object System.IO.StreamReader($stream2)
    Write-Host "FAIL status=$([int]$_.Exception.Response.StatusCode)"
    Write-Host $reader2.ReadToEnd()
}
