# 1. Get a Valid Token
Write-Host "1. Getting valid token..."
$auth = Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/login" -Method Post -Body (@{Username="ahmet"; Password="password"} | ConvertTo-Json) -ContentType "application/json"
$validToken = $auth.Token
Write-Host "Valid Token: $($validToken.Substring(0, 10))..."

# 2. Test Validity
try {
    Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/me" -Method Get -Headers @{Authorization=("Bearer " + $validToken)}
    Write-Host "Test 1 (Valid Token): PASSED (200 OK)" -ForegroundColor Green
} catch {
    Write-Host "Test 1 (Valid Token): FAILED" -ForegroundColor Red
}

# 3. Create a TAMPERED Token (Change payload but keep same signature - this invalidates the signature mathematically)
# We will just change the last character of the token string.
$tamperedToken = $validToken.Substring(0, $validToken.Length - 5) + "xxxxx"
Write-Host "`n2. Testing Tampered Token..."

try {
    Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/me" -Method Get -Headers @{Authorization=("Bearer " + $tamperedToken)}
    Write-Host "Test 2 (Tampered Token): FAILED (Accepted invalid token!)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq [System.Net.HttpStatusCode]::Unauthorized) {
        Write-Host "Test 2 (Tampered Token): PASSED (401 Unauthorized)" -ForegroundColor Green
    } else {
        Write-Host "Test 2 (Tampered Token): UNEXPECTED ($($_.Exception.Response.StatusCode))" -ForegroundColor Yellow
    }
}
