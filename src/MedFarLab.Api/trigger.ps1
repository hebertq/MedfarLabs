Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue

$proc = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru
$maxRetries = 20
$url = "http://localhost:5152/generate-docs"
for ($i=0; $i -lt $maxRetries; $i++) {
    Start-Sleep -Seconds 3
    try {
        $response = Invoke-WebRequest -Uri $url -Method Get -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Output "Success: $($response.Content)"
            break
        }
    } catch {
        Write-Output "Waiting for server... ($i)"
    }
}
Stop-Process -Id $proc.Id -Force
