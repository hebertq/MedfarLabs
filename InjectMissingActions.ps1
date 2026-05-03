$file = "C:\Users\GLOBALPRO\.gemini\antigravity\scratch\MedfarLabs.Core\src\Migrations\Scripts\002_Insert_Seguridad.sql"
$content = Get-Content $file -Raw

# 1. Add Modules 9, 10, 12
$content = $content -replace "\(11, 'Report', true\)", "(9, 'Patient', true),`n(10, 'Pharmacy', true),`n(11, 'Report', true),`n(12, 'System', true)"

# 2. Add Actions
$content = $content -replace "-- --- BILLING \(Module 3\) ---", "-- --- BILLING (Module 3) ---`n(3010, 3, 'GetAllInvoices'),`n(3011, 3, 'GetInvoiceById'),`n(3012, 3, 'ActualizarFactura'),"
$content = $content -replace "-- --- CLINICAL \(Module 4\) ---", "-- --- CLINICAL (Module 4) ---`n(4010, 4, 'GetPatientDirectory'),`n(4011, 4, 'GetPatientRecord'),`n(10005, 4, 'SearchDiagnoses'),"
$content = $content -replace "-- --- CARE \(Module 5\) ---", "-- --- CARE (Module 5) ---`n(5005, 5, 'GetConsultationContext'),`n(5006, 5, 'GetConsultationDetails'),"
$content = $content -replace "-- --- COMMON \(Module 6\) ---", "-- --- COMMON (Module 6) ---`n(6003, 6, 'AddCatalogDetail'),"
$content = $content -replace "-- --- LABORATORY \(Module 8\) ---", "-- --- LABORATORY (Module 8) ---`n(8002, 8, 'GetSamples'),`n(8003, 8, 'ReceiveSample'),`n(8004, 8, 'RejectSample'),`n(8005, 8, 'GetServiceSampleConfigs'),`n(8006, 8, 'SaveServiceSampleConfigs'),`n(8010, 8, 'CreateSample'),"
$content = $content -replace "-- --- REPORT \(Module 11\) ---", "-- --- PATIENT (Module 9) ---`n(9001, 9, 'ConsultarPaciente'),`n(9002, 9, 'ActualizarPaciente'),`n`n-- --- PHARMACY (Module 10) ---`n(10001, 10, 'DespacharReceta'),`n(10002, 10, 'ConsultarInventarioFarmacia'),`n`n-- --- SYSTEM (Module 12) ---`n(12001, 12, 'ConsultarMenus'),`n(12002, 12, 'AgregarMenu'),`n(12003, 12, 'ActualizarMenu'),`n(12004, 12, 'EliminarMenu'),`n`n-- --- REPORT (Module 11) ---`n(11003, 11, 'PrescriptionPDF'),`n(11004, 11, 'LabOrderPDF'),"

Set-Content $file $content
Write-Host "File updated successfully"
