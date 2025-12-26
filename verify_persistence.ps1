# Test DB Persistence
$ErrorActionPreference = "Stop"

# 1. Create Data
Write-Host "Creating Post..." -ForegroundColor Cyan

$maxRetries = 20
$retryCount = 0
$connected = $false

while (-not $connected -and $retryCount -lt $maxRetries) {
    try {
        $loginBody = @{Username = "persist_user"; Password = "Password123." } | ConvertTo-Json
        try {
            Invoke-RestMethod -Uri "http://127.0.0.1:5000/api/Auth/register" -Method Post -Body $loginBody -ContentType "application/json" | Out-Null
        }
        catch {}

        $tokenRes = Invoke-RestMethod -Uri "http://127.0.0.1:5000/api/Auth/login" -Method Post -Body $loginBody -ContentType "application/json"
        
        if ($tokenRes.token) {
            $connected = $true
        }
        else {
            throw "No token received"
        }
    }
    catch {
        $retryCount++
        Write-Host "Connection attempt $retryCount failed. Retrying in 5s..." -ForegroundColor DarkGray
        Start-Sleep -Seconds 5
    }
}

if (-not $connected) { exit }

$token = $tokenRes.token
$headers = @{Authorization = "Bearer $token" }

$postBody = @{Title = "Persistence Test"; Content = "This should survive restart." } | ConvertTo-Json
$res = Invoke-RestMethod -Uri "http://127.0.0.1:5000/api/Blog" -Method Post -Headers $headers -Body $postBody -ContentType "application/json"
$postId = $res.PostId

Write-Host "Post Created! ID: $postId" -ForegroundColor Green

# 2. Restart Docker
Write-Host "Restarting Docker Containers..." -ForegroundColor Yellow
docker-compose restart

# Wait for DB
Start-Sleep -Seconds 10

# 3. Check Data
Write-Host "Checking Data..." -ForegroundColor Cyan
try {
    $posts = Invoke-RestMethod -Uri "http://127.0.0.1:5000/api/Blog"
    $found = $posts | Where-Object { $_.id -eq $postId }
    if ($found) {
        Write-Host "SUCCESS: Post found after restart." -ForegroundColor Green
    }
    else {
        Write-Host "FAILURE: Post NOT found." -ForegroundColor Red
    }
}
catch {
    Write-Host "Check Failed: $_" -ForegroundColor Red
}
