try {
    Write-Host "Testing Login with Non-Existent User..."
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/login" -Method Post -Body (@{Username = "nonexistentuser"; Password = "Password123" } | ConvertTo-Json) -ContentType "application/json"
    Write-Host "FAILED: Should have returned an error." -ForegroundColor Red
}
catch {
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $body = $reader.ReadToEnd()
    
    Write-Host "Response Body: $body"
    
    try {
        $json = $body | ConvertFrom-Json
        if ($json.message -eq "User not found.") {
            Write-Host "PASSED: Response is valid JSON with correct message." -ForegroundColor Green
        }
        else {
            Write-Host "FAILED: JSON message is incorrect: $($json.message)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "FAILED: Response is NOT valid JSON." -ForegroundColor Red
    }
}
