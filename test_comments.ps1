# Test Comments Feature with Registration
$ErrorActionPreference = "Stop"

$username = "commentuser2"
$password = "Password123."

function Get-Token {
    try {
        # Try login
        $body = @{Username = $username; Password = $password } | ConvertTo-Json
        $response = Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/login" -Method Post -Body $body -ContentType "application/json"
        return $response.token
    }
    catch {
        # If login fails, try register
        Write-Host "Login failed, attempting to register..." -ForegroundColor Yellow
        try {
            $regBody = @{Username = $username; Email = "$username@test.com"; Password = $password } | ConvertTo-Json
            Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/register" -Method Post -Body $regBody -ContentType "application/json"
            Write-Host "Registered new user: $username" -ForegroundColor Green
            
            # Login again
            $body = @{Username = $username; Password = $password } | ConvertTo-Json
            $response = Invoke-RestMethod -Uri "http://localhost:5000/api/Auth/login" -Method Post -Body $body -ContentType "application/json"
            return $response.token
        }
        catch {
            Write-Host "Registration/Login Failed: $_" -ForegroundColor Red
            return $null
        }
    }
}

# 1. Login/Register
$token = Get-Token
if (!$token) { exit }

# 2. Get a Post ID (Create one if none exist)
$posts = Invoke-RestMethod -Uri "http://localhost:5000/api/Blog"
if ($posts.Count -eq 0) {
    Write-Host "No posts found, creating one..."
    $headers = @{Authorization = "Bearer $token" }
    $postBody = @{Title = "Test Post"; Content = "Testing comments..." } | ConvertTo-Json
    $newPost = Invoke-RestMethod -Uri "http://localhost:5000/api/Blog" -Method Post -Headers $headers -Body $postBody -ContentType "application/json"
    $postId = $newPost.PostId
}
else {
    $postId = $posts[0].id
}

Write-Host "Testing on Post ID: $postId" -ForegroundColor Cyan

# 3. Add Comment
Write-Host "Adding Comment..."
$headers = @{Authorization = "Bearer $token" }
$commentBody = @{Content = "Automated comment via script." } | ConvertTo-Json

try {
    $commentRes = Invoke-RestMethod -Uri "http://localhost:5000/api/Blog/$postId/comments" -Method Post -Headers $headers -Body $commentBody -ContentType "application/json"
    Write-Host "Comment Added! ID: $($commentRes.CommentId), New Risk: $($commentRes.NewRiskScore)" -ForegroundColor Green
}
catch {
    Write-Host "Failed to add comment: $_" -ForegroundColor Red
    exit
}

# 4. List Comments
Write-Host "Listing Comments..."
$comments = Invoke-RestMethod -Uri "http://localhost:5000/api/Blog/$postId/comments"
if ($comments.Count -gt 0) {
    Write-Host "Found $($comments.Count) comments." -ForegroundColor Green
    $comments | Format-Table AuthorUsername, Content, CreatedAt
}
else {
    Write-Host "No comments found!" -ForegroundColor Red
}
