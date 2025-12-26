# Verify Comprehensive Risk Logic
$ErrorActionPreference = "Stop"
$username = "risktestuser_$(Get-Random)"
$password = "Password123."

Write-Host "Creating Risk Test User: $username" -ForegroundColor Cyan

# Retry logic for initial connection
$maxRetries = 20
$retryCount = 0
$connected = $false

while (-not $connected -and $retryCount -lt $maxRetries) {
    try {
        # 1. Register (Should start at Risk 20)
        $regBody = @{Username = $username; Email = "$username@test.com"; Password = $password } | ConvertTo-Json
        Invoke-RestMethod -Uri "http://127.0.0.1:5000/api/Auth/register" -Method Post -Body $regBody -ContentType "application/json" | Out-Null
        $connected = $true
        Write-Host "Connected and Registered!" -ForegroundColor Green
    }
    catch {
        $retryCount++
        Write-Host "Connection attempt $retryCount failed. Retrying in 5s..." -ForegroundColor DarkGray
        Start-Sleep -Seconds 5
    }
}

if (-not $connected) {
    Write-Host "Failed to connect to API after $maxRetries attempts." -ForegroundColor Red
    exit
}

# Login
try {
    $loginBody = @{Username = $username; Password = $password } | ConvertTo-Json
    $tokenRes = Invoke-RestMethod -Uri "http://127.0.0.1:5000/api/Auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $tokenRes.token
    $initialRisk = $tokenRes.riskScore
    Write-Host "Initial Risk Score: $initialRisk (Expected: 20 -> Login Success -> 18?)" -ForegroundColor Green
}
catch {
    Write-Host "Login Failed: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

$headers = @{Authorization = "Bearer $token" }

# Get Post ID
try {
    $posts = Invoke-RestMethod -Uri "http://127.0.0.1:5000/api/Blog"
    if ($posts.Count -eq 0) {
        # Create one
        $postBody = @{Title = "Test"; Content = "Safe content" } | ConvertTo-Json
        $newPost = Invoke-RestMethod -Uri "http://127.0.0.1:5000/api/Blog" -Method Post -Headers $headers -Body $postBody -ContentType "application/json"
        $postId = $newPost.PostId
    }
    else {
        $postId = $posts[0].id
    }
}
catch {
    Write-Host "Get/Create Post Failed: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

# 2. XSS Attack Simulation
Write-Host "`n[TEST] XSS Attack Simulation (<script>)" -ForegroundColor Yellow
$xssBody = @{Content = "Hello <script>alert(1)</script>" } | ConvertTo-Json
try {
    Invoke-RestMethod -Uri "http://127.0.0.1:5000/api/Blog/$postId/comments" -Method Post -Headers $headers -Body $xssBody -ContentType "application/json"
    Write-Host "FAILURE: XSS Post accepted (Should have been blocked)" -ForegroundColor Red
}
catch {
    $ex = $_.Exception.Response
    if ($ex.StatusCode -eq [System.Net.HttpStatusCode]::BadRequest) {
        Write-Host "SUCCESS: XSS Post Blocked (400 Bad Request)" -ForegroundColor Green
        
        # Parse response to verify risk score increased
        $stream = $ex.GetResponseStream()
        $reader = New-Object System.IO.StreamReader $stream
        $respContent = $reader.ReadToEnd()
        $reader.Close()
        
        $json = $respContent | ConvertFrom-Json
        Write-Host "Message: $($json.Message)" -ForegroundColor Yellow
        Write-Host "New Risk Score: $($json.NewRiskScore)" -ForegroundColor Magenta
    }
    else {
        Write-Host "XSS Request Failed with unexpected code: $($ex.StatusCode)" -ForegroundColor Red
    }
}

# 3. Flood Simulation (Rapid Comments)
Write-Host "`n[TEST] Flood Simulation (8 comments to trigger block)" -ForegroundColor Yellow
for ($i = 1; $i -le 8; $i++) {
    $body = @{Content = "Spam comment $i" } | ConvertTo-Json
    try {
        $res = Invoke-RestMethod -Uri "http://127.0.0.1:5000/api/Blog/$postId/comments" -Method Post -Headers $headers -Body $body -ContentType "application/json"
        Write-Host "Comment $i Risk Score: $($res.NewRiskScore)"
        if ($res.Message -match "Warning") {
            Write-Host "WARNING RECEIVED: $($res.Message)" -ForegroundColor DarkYellow
        }
    }
    catch {
        Write-Host "Comment $i Failed: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            # Check if it was blocked (403)
            $statusCode = $_.Exception.Response.StatusCode
            if ($statusCode -eq [System.Net.HttpStatusCode]::Forbidden) {
                Write-Host "BLOCKED (403) - Expected if risk high." -ForegroundColor Green
            }
        }
    }
    Start-Sleep -Milliseconds 200
}

# 4. Check Block Status (if Score > 60)
Write-Host "`n[TEST] Check Block Status (>60)" -ForegroundColor Yellow
# Try one more
$body = @{Content = "Trying to post when high risk" } | ConvertTo-Json
try {
    Invoke-RestMethod -Uri "http://127.0.0.1:5000/api/Blog/$postId/comments" -Method Post -Headers $headers -Body $body -ContentType "application/json"
    Write-Host "Allowed? Should be blocked if risk > 60." -ForegroundColor Red
}
catch {
    $ex = $_.Exception.Response
    if ($ex.StatusCode -eq [System.Net.HttpStatusCode]::Forbidden) {
        Write-Host "BLOCKED (403): User successfully restricted." -ForegroundColor Green
        $reader = New-Object System.IO.StreamReader $ex.GetResponseStream()
        Write-Host "Block Message: $($reader.ReadToEnd())" -ForegroundColor DarkGreen
    }
    else {
        Write-Host "Failed with unexpected code: $($ex.StatusCode)" -ForegroundColor Red
    }
}
