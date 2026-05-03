$csharpDir = "C:\Users\GLOBALPRO\.gemini\antigravity\scratch\MedfarLabs.Core\src\Domain\Const"
$sqlFile = "C:\Users\GLOBALPRO\.gemini\antigravity\scratch\MedfarLabs.Core\src\Migrations\Scripts\002_Insert_Seguridad.sql"

# 1. Parse SQL file to find existing actions
$sqlContent = Get-Content $sqlFile -Raw
# Regex to find (ID, ModuleID, 'Name')
$sqlRegex = "\((\d{4,5}),\s*\d+,\s*'[^']+'\)"
$existingIds = @()
$matches = [regex]::Matches($sqlContent, $sqlRegex)
foreach ($match in $matches) {
    $existingIds += $match.Groups[1].Value
}

# 2. Parse C# files to find all defined actions
$csharpFiles = Get-ChildItem -Path $csharpDir -Filter AppAction*.cs
$definedActions = @()

foreach ($file in $csharpFiles) {
    # Extract module name from file name: AppAction.Module.cs
    $moduleName = $file.Name.Split('.')[1]
    $content = Get-Content $file.FullName
    
    foreach ($line in $content) {
        if ($line -match "public const int (\w+)\s*=\s*(\d+);") {
            $actionName = $matches[1]
            $actionId = $matches[2]
            
            # Map module name to module ID (approximate based on AppModules enum)
            $moduleId = 0
            switch ($moduleName) {
                "Security" { $moduleId = 1 }
                "Identity" { $moduleId = 2 }
                "Billling" { $moduleId = 3 } # typo in filename?
                "Billing" { $moduleId = 3 }
                "Clinical" { $moduleId = 4 }
                "Care" { $moduleId = 5 }
                "Common" { $moduleId = 6 }
                "Inventory" { $moduleId = 7 }
                "Laboratory" { $moduleId = 8 }
                "Report" { $moduleId = 11 }
                "Patient" { $moduleId = 9 } # Assuming
                "Pharmacy" { $moduleId = 10 } # Assuming
                "System" { $moduleId = 12 } # Assuming
            }
            
            $definedActions += [PSCustomObject]@{
                Id = $actionId
                Name = $actionName
                Module = $moduleName
                ModuleId = $moduleId
            }
        }
    }
}

# 3. Find missing actions
$missingActions = @()
foreach ($action in $definedActions) {
    if ($existingIds -notcontains $action.Id) {
        $missingActions += $action
    }
}

# Output missing actions as SQL insert statements
Write-Host "--- MISSING ACTIONS ---"
foreach ($action in $missingActions) {
    Write-Host "($($action.Id), $($action.ModuleId), '$($action.Name)'), -- Module: $($action.Module)"
}
