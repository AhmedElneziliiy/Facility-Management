$loginR = Invoke-WebRequest -Uri "http://localhost:5161/api/auth/login" -Method POST -Body '{"username":"manager1","password":"Pass@123"}' -ContentType "application/json" -UseBasicParsing
$token = ($loginR.Content | ConvertFrom-Json).data.token

$r = Invoke-WebRequest -Uri "http://localhost:5161/api/buildings/10000000-0000-0000-0000-000000000002/floors" -Method GET -Headers @{ Authorization="Bearer $token" } -UseBasicParsing
Write-Host "Annex floors:"
Write-Host $r.Content

$data = ($r.Content | ConvertFrom-Json).data
Write-Host "Count: $($data.Count)"
foreach ($f in $data) {
    Write-Host "  level=$($f.level) name=$($f.name)"
}
