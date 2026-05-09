# Decode JWT and show all claims
$loginR = Invoke-WebRequest -Uri "http://localhost:5161/api/auth/login" -Method POST -Body '{"username":"facilities1","password":"Pass@123"}' -ContentType "application/json" -UseBasicParsing
$token = ($loginR.Content | ConvertFrom-Json).data.token

$parts = $token.Split(".")
$payload = $parts[1]
while ($payload.Length % 4 -ne 0) { $payload += "=" }
$bytes = [System.Convert]::FromBase64String($payload.Replace("-","+").Replace("_","/"))
$decoded = [System.Text.Encoding]::UTF8.GetString($bytes)
Write-Host "JWT Payload claims:"
Write-Host $decoded

# Now decode using System.IdentityModel.Tokens.Jwt if available
# Check what .NET says about the claim type mapping
$handler = New-Object System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler
Write-Host "`nDefault inbound claim type map:"
$map = $handler.InboundClaimTypeMap
foreach ($k in $map.Keys) {
    if ($k -eq "role") { Write-Host "  role -> $($map[$k])" }
}
