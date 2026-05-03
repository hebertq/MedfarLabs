using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MediatR;
using MedFarLab.Application.Features.Inventory.Models;

namespace MedFarLab.Application.Features.Inventory.Queries.GetServiceCatalogQuery
{
    public class GetServiceCatalogQuery : IRequest<BaseResponse<List<ServiceItemVM>>>
    {
        public string TenantRoute { get; set; } = string.Empty;

        public GetServiceCatalogQuery(string tenantRoute)
        {
            TenantRoute = tenantRoute;
        }
    }

    public class GetServiceCatalogQueryHandler : IRequestHandler<GetServiceCatalogQuery, BaseResponse<List<ServiceItemVM>>>
    {
        public async Task<BaseResponse<List<ServiceItemVM>>> Handle(GetServiceCatalogQuery request, CancellationToken cancellationToken)
        {
            await Task.Delay(200);

            var list = new List<ServiceItemVM>
            {
                new ServiceItemVM { Id = 1, Code = "MED-001", Name = "Consulta Medicina General", Category = "Médico", UnitPrice = 45.00m, IsTaxable = false },
                new ServiceItemVM { Id = 2, Code = "MED-002", Name = "Consulta Especialista (Cardiología)", Category = "Médico", UnitPrice = 80.00m, IsTaxable = false },
                new ServiceItemVM { Id = 3, Code = "LAB-101", Name = "Biometría Hemática Completa", Category = "Laboratorio", UnitPrice = 18.50m, IsTaxable = true },
                new ServiceItemVM { Id = 4, Code = "LAB-105", Name = "Química Sanguínea (6 elementos)", Category = "Laboratorio", UnitPrice = 25.00m, IsTaxable = true },
                new ServiceItemVM { Id = 5, Code = "LAB-210", Name = "Prueba de Embarazo en Sangre", Category = "Laboratorio", UnitPrice = 12.00m, IsTaxable = true },
                new ServiceItemVM { Id = 6, Code = "PROC-01", Name = "Curación Básica / Suturas", Category = "Procedimiento", UnitPrice = 30.00m, IsTaxable = true },
                new ServiceItemVM { Id = 7, Code = "FAR-050", Name = "Aplicación Inyectable (Intramuscular)", Category = "Farmacia", UnitPrice = 5.00m, IsTaxable = true },
                new ServiceItemVM { Id = 8, Code = "INS-001", Name = "Jeringa Descartable 5ml", Category = "Insumos Clínicos", UnitPrice = 1.50m, IsTaxable = true },
                new ServiceItemVM { Id = 9, Code = "MED-820", Name = "Dexametasona Ampolla 8mg", Category = "Farmacia", UnitPrice = 8.00m, IsTaxable = true },
                new ServiceItemVM { Id = 10, Code = "MED-821", Name = "Diclofenaco Ampolla 75mg", Category = "Farmacia", UnitPrice = 6.00m, IsTaxable = true },
                new ServiceItemVM { Id = 11, Code = "LAB-110", Name = "Examen General de Orina (EGO)", Category = "Laboratorio", UnitPrice = 10.00m, IsTaxable = true },
                new ServiceItemVM { Id = 12, Code = "LAB-115", Name = "Coproparasitoscópico (x3)", Category = "Laboratorio", UnitPrice = 15.00m, IsTaxable = true },
                new ServiceItemVM { Id = 13, Code = "LAB-120", Name = "Perfil Lipídico", Category = "Laboratorio", UnitPrice = 22.00m, IsTaxable = true },
                new ServiceItemVM { Id = 14, Code = "LAB-125", Name = "Perfil Hepático", Category = "Laboratorio", UnitPrice = 28.00m, IsTaxable = true },
                new ServiceItemVM { Id = 15, Code = "LAB-130", Name = "Perfil Tiroideo", Category = "Laboratorio", UnitPrice = 35.00m, IsTaxable = true },
                new ServiceItemVM { Id = 16, Code = "LAB-150", Name = "Prueba Rápida de VIH", Category = "Laboratorio", UnitPrice = 20.00m, IsTaxable = true },
                new ServiceItemVM { Id = 17, Code = "LAB-155", Name = "VDRL / Sífilis", Category = "Laboratorio", UnitPrice = 15.00m, IsTaxable = true }
            };

            // Filter contextually based on TenantRoute to simulate separated organization product catalogs
            if (!string.IsNullOrEmpty(request.TenantRoute) && request.TenantRoute != "admin")
            {
                if (request.TenantRoute == "laboratory")
                {
                    list = list.Where(x => x.Category == "Laboratorio").ToList();
                }
                else if (request.TenantRoute == "pharmacy")
                {
                    list = list.Where(x => x.Category == "Farmacia" || x.Category == "Insumos Clínicos").ToList();
                }
                else if (request.TenantRoute == "clinical")
                {
                    list = list.Where(x => x.Category == "Médico" || x.Category == "Procedimiento" || x.Category == "Insumos Clínicos").ToList();
                }
            }

            return BaseResponse<List<ServiceItemVM>>.Success(list, "Catálogo cargado con éxito.");
        }
    }
}
