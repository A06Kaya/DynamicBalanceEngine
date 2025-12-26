
$Url = "http://localhost:5000/api"

# 1. Login to get Token
$loginBody = @{
    username = "SpamTester"
    password = "Password123!"
} | ConvertTo-Json

try {
    Write-Host "Registering SpamTester..."
    Invoke-RestMethod -Uri "$Url/Auth/register" -Method Post -Body (@{username = "SpamTester"; email = "test@test.com"; password = "Password123!" } | ConvertTo-Json) -ContentType "application/json" -ErrorAction SilentlyContinue
}
catch {}

Write-Host "Logging in..."
$loginRes = Invoke-RestMethod -Uri "$Url/Auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginRes.Token
Write-Host "Initial Risk: $($loginRes.RiskScore)"

# 2. Post SPAM
$spamBody = @{
    title   = "SPAM TITLE"
    content = "THIS IS A TEST OF THE CAPS LOCK SPAM DETECTION SYSTEM AAAAAAAAAAAAAAAA"
} | ConvertTo-Json

Write-Host "Sending Spam Post..."
try {
    $postRes = Invoke-RestMethod -Uri "$Url/Blog" -Method Post -Body $spamBody -ContentType "application/json" -Headers @{Authorization = "Bearer $token" }
    
    Write-Host "Response received."
    Write-Host "New Risk Score: $($postRes.NewRiskScore)"
    Write-Host "Message: $($postRes.Message)"
}
catch {
    Write-Host "Error: $($_.Exception.Message)"
    $stream = $_.Exception.Response.GetResponseStream()
    if ($stream) {
        $reader = New-Object System.IO.StreamReader($stream)
        Write-Host "Details: $($reader.ReadToEnd())"
    }
}
