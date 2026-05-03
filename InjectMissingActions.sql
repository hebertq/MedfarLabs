-- Modules
INSERT INTO security.mst_module (id, name, is_active) VALUES
(9, 'Patient', true),
(10, 'Pharmacy', true),
(12, 'System', true)
ON CONFLICT (id) DO UPDATE SET name = EXCLUDED.name;

-- Actions
INSERT INTO security.mst_action (id, module_id, name) VALUES
(3010, 3, 'GetAllInvoices'),
(3011, 3, 'GetInvoiceById'),
(3012, 3, 'ActualizarFactura'),
(4010, 4, 'GetPatientDirectory'),
(4011, 4, 'GetPatientRecord'),
(10005, 4, 'SearchDiagnoses'),
(5005, 5, 'GetConsultationContext'),
(5006, 5, 'GetConsultationDetails'),
(6003, 6, 'AddCatalogDetail'),
(8002, 8, 'GetSamples'),
(8003, 8, 'ReceiveSample'),
(8004, 8, 'RejectSample'),
(8005, 8, 'GetServiceSampleConfigs'),
(8006, 8, 'SaveServiceSampleConfigs'),
(8010, 8, 'CreateSample'),
(9001, 9, 'ConsultarPaciente'),
(9002, 9, 'ActualizarPaciente'),
(10001, 10, 'DespacharReceta'),
(10002, 10, 'ConsultarInventarioFarmacia'),
(11003, 11, 'PrescriptionPDF'),
(11004, 11, 'LabOrderPDF'),
(12001, 12, 'ConsultarMenus'),
(12002, 12, 'AgregarMenu'),
(12003, 12, 'ActualizarMenu'),
(12004, 12, 'EliminarMenu')
ON CONFLICT (id) DO NOTHING;

-- Map to Roles
INSERT INTO security.mst_role (name, description)
SELECT 'Admin-' || name, 'Administrador total del módulo ' || name
FROM security.mst_module
ON CONFLICT (name) DO NOTHING;

INSERT INTO security.map_role_action (role_id, action_id)
SELECT r.id, a.id
FROM security.mst_role r
JOIN security.mst_module m ON r.name = 'Admin-' || m.name
JOIN security.mst_action a ON a.module_id = m.id
ON CONFLICT DO NOTHING;

-- Update role groups for SaaS Master to have the new roles (Patient, Pharmacy, System)
INSERT INTO security.map_role_group_role (role_group_id, role_id)
SELECT 1, id FROM security.mst_role WHERE name LIKE 'Admin-%'
ON CONFLICT DO NOTHING;

-- Update Group 2 (Clínica)
INSERT INTO security.map_role_group_role (role_group_id, role_id)
SELECT 2, id FROM security.mst_role WHERE name IN ('Admin-Patient', 'Admin-Pharmacy', 'Admin-System')
ON CONFLICT DO NOTHING;

-- Update Group 3 (Lab)
INSERT INTO security.map_role_group_role (role_group_id, role_id)
SELECT 3, id FROM security.mst_role WHERE name IN ('Admin-Patient', 'Admin-System')
ON CONFLICT DO NOTHING;

-- Update Group 4 (Farmacia)
INSERT INTO security.map_role_group_role (role_group_id, role_id)
SELECT 4, id FROM security.mst_role WHERE name IN ('Admin-Pharmacy', 'Admin-System')
ON CONFLICT DO NOTHING;

-- Update Group 5 (Políclínico)
INSERT INTO security.map_role_group_role (role_group_id, role_id)
SELECT 5, id FROM security.mst_role WHERE name LIKE 'Admin-%'
ON CONFLICT DO NOTHING;
