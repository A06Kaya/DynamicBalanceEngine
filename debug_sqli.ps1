
$Url = "http://localhost:5000/api"

# 1. Login
$loginBody = @{
    username = "SpamTester"
    password = "Password123!"
} | ConvertTo-Json

try {
    # Try register in case it was wiped
    Invoke-RestMethod -Uri "$Url/Auth/register" -Method Post -Body (@{username = "SpamTester"; email = "test@test.com"; password = "Password123!" } | ConvertTo-Json) -ContentType "application/json" -ErrorAction SilentlyContinue
}
catch {}

$loginRes = Invoke-RestMethod -Uri "$Url/Auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginRes.Token
Write-Host "Initial Risk: $($loginRes.RiskScore)"

# 2. Post SQLi
$sqliBody = @{
    title   = "Hacked"
    content = "Hello ' OR '1'='1"
} | ConvertTo-Json

Write-Host "Sending SQLi Attack..."
try {
    $postRes = Invoke-RestMethod -Uri "$Url/Blog" -Method Post -Body $sqliBody -ContentType "application/json" -Headers @{Authorization = "Bearer $token" }
    Write-Host "New Risk Score: $($postRes.NewRiskScore)"
}
catch {
    Write-Host "Error: $($_.Exception.Message)"
    $stream = $_.Exception.Response.GetResponseStream()
    if ($stream) {
        $reader = New-Object System.IO.StreamReader($stream)
        Write-Host "Details: $($reader.ReadToEnd())"
    }
}
