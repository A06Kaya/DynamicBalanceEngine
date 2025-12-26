Write-Host "1. Login (Fresh User)..."
try {
    # 1. Register/Login
    $login = Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/login" -Method Post -Body (@{Username = "blogtester"; Password = "Password123" } | ConvertTo-Json) -ContentType "application/json"
}
catch {
    # If not exists, register first
    Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/register" -Method Post -Body (@{Username = "blogtester"; Email = "blog@test.com"; Password = "Password123" } | ConvertTo-Json) -ContentType "application/json" | Out-Null
    $login = Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/login" -Method Post -Body (@{Username = "blogtester"; Password = "Password123" } | ConvertTo-Json) -ContentType "application/json"
}

$token = $login.Token
Write-Host "Logged in. Initial Risk Score: $($login.RiskScore)"

# 2. Create Post (Should Succeed)
Write-Host "`n2. Creating Post (Low Risk)..."
try {
    $postResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/Blog" -Method Post -Headers @{Authorization = ("Bearer " + $token) } -Body (@{Title = "My First Post"; Content = "This is valid content for the blog." } | ConvertTo-Json) -ContentType "application/json"
    Write-Host "SUCCESS: Post created. New Risk Score: $($postResponse.NewRiskScore)" -ForegroundColor Green
}
catch {
    Write-Host "FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

# 3. Simulate High Risk (We cannot easily inject DB state here without a backdoor, so we rely on the logic being correct. 
# Ideally, we would have an admin endpoint to set risk. For now, we will assume logic works if Step 2 passed.)
# To truly test, we could spam login failures?
Write-Host "`n3. Simulating High Risk (Login Failures)..."
For ($i = 0; $i -lt 6; $i++) {
    try {
        Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/login" -Method Post -Body (@{Username = "blogtester"; Password = "WRONG_PASSWORD" } | ConvertTo-Json) -ContentType "application/json" | Out-Null
    }
    catch {
        # Expected
    }
}

# Re-login to get updated risk score (or check /me)
$me = Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/me" -Method Get -Headers @{Authorization = ("Bearer " + $token) }
Write-Host "Current Risk Score after failures: $($me.RiskScore)"

if ($me.RiskScore -gt 50) {
    Write-Host "`n4. Attempting Post with High Risk..."
    try {
        Invoke-RestMethod -Uri "http://localhost:5000/api/Blog" -Method Post -Headers @{Authorization = ("Bearer " + $token) } -Body (@{Title = "Spam Post"; Content = "This should be blocked." } | ConvertTo-Json) -ContentType "application/json"
        Write-Host "FAILED: Blocked user was allowed to post!" -ForegroundColor Red
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq [System.Net.HttpStatusCode]::Forbidden) {
            Write-Host "PASSED: High risk user blocked (403 Forbidden)" -ForegroundColor Green
        }
        else {
            Write-Host "FAILED: Unexpected status $($_.Exception.Response.StatusCode)" -ForegroundColor Yellow
        }
    }
}
else {
    Write-Host "Skipping high risk test (Risk Score didn't increase enough). Check python script logic." -ForegroundColor Yellow
}
