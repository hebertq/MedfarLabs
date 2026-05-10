$files = Get-ChildItem -Recurse -Filter *.csproj
foreach ($f in $files) {
    $content = Get-Content $f.FullName
    $content = $content -replace '1.2.196', '1.2.197'
    Set-Content -Path $f.FullName -Value $content
}
