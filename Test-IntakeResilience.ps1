# Test-IntakeResilience.ps1

# PS 5.1 Workarounds
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::DefaultConnectionLimit = 1000 
[System.Net.ServicePointManager]::Expect100Continue = $true # Force the robust handshake

$baseUrl = "https://localhost:7443/api/intake"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " RATE LIMITING STRESS TEST (105 GETs)   " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# --- THE WARM-UP POST ---
# We send an intentionally invalid JSON payload. 
# ASP.NET Core will reject it with a 400 Bad Request before hitting your DB.
# But it forces the stable TLS POST handshake that PowerShell 5.1 requires.
Write-Host "Establishing secure TLS tunnel via POST..." -ForegroundColor DarkGray
$headers = @{ "Content-Type" = "application/json" }
$invalidBody = '{"ThisIs": "Wrong"}'

try {
    $null = Invoke-RestMethod -Uri $baseUrl -Method Post -Headers $headers -Body $invalidBody -ErrorAction SilentlyContinue
} catch { }

Start-Sleep -Milliseconds 500
# ------------------------

# The main DDoS simulation
for ($i = 1; $i -le 105; $i++) {
    try {
        $response = Invoke-RestMethod -Uri $baseUrl -Method Get -ErrorAction Stop 
        Write-Host "Request $i : 200 OK" -ForegroundColor Green
    }
    catch {
        $response = $_.Exception.Response
        if ($null -ne $response) {
            $statusCode = [int]$response.StatusCode
            if ($statusCode -eq 429) {
                Write-Host "Request $i : 429 Too Many Requests (Rate Limiter kicked in!)" -ForegroundColor Yellow
            } else {
                Write-Host "Request $i : HTTP $statusCode" -ForegroundColor Red
            }
        } else {
            Write-Host "Request $i : Connection Failed ($($_.Exception.Message))" -ForegroundColor Red
        }
    }
}

Write-Host "`nTest Complete." -ForegroundColor Green