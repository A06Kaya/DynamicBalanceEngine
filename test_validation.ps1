try {
    Write-Host "Testing invalid registration (Password too short, no uppercase)..."
    Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/register" -Method Post -Body (@{Username = "testuser"; Email = "test@test.com"; Password = "low" } | ConvertTo-Json) -ContentType "application/json"
    Write-Host "FAILED: Request should have been rejected." -ForegroundColor Red
}
catch {
    if ($_.Exception.Response.StatusCode -eq [System.Net.HttpStatusCode]::BadRequest) {
        Write-Host "PASSED: Request rejected with 400 Bad Request" -ForegroundColor Green
        $errorResponse = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorResponse)
        $responseBody = $reader.ReadToEnd()
        Write-Host "Validation Errors: $responseBody"
    }
    else {
        Write-Host "FAILED: Unexpected status code $($_.Exception.Response.StatusCode)" -ForegroundColor Yellow
    }
}
