using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MediatR;

namespace MedFarLab.Application.Features.Pharmacy.Queries.GetMedicationCatalog
{
    public class MedicationItemDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string GenericComponent { get; set; } = string.Empty;
        public string Presentation { get; set; } = string.Empty;
        public string MedicationType { get; set; } = string.Empty;
    }

    public class GetMedicationCatalogQuery : IRequest<BaseResponse<List<MedicationItemDTO>>>
    {
    }

    public class GetMedicationCatalogQueryHandler : IRequestHandler<GetMedicationCatalogQuery, BaseResponse<List<MedicationItemDTO>>>
    {
        public async Task<BaseResponse<List<MedicationItemDTO>>> Handle(GetMedicationCatalogQuery request, CancellationToken cancellationToken)
        {
            await Task.Delay(200);

            var list = new List<MedicationItemDTO>
            {
                new MedicationItemDTO { Id = 1, Name = "Paracetamol 500mg Tableta", Brand = "Panadol", GenericComponent = "Paracetamol", Presentation = "Caja x 100", MedicationType = "Tableta" },
                new MedicationItemDTO { Id = 2, Name = "Ibuprofeno 400mg Tableta", Brand = "Advil", GenericComponent = "Ibuprofeno", Presentation = "Caja x 50", MedicationType = "Tableta" },
                new MedicationItemDTO { Id = 3, Name = "Amoxicilina 500mg Cápsula", Brand = "Amoxil", GenericComponent = "Amoxicilina", Presentation = "Caja x 21", MedicationType = "Cápsula" },
                new MedicationItemDTO { Id = 4, Name = "Omeprazol 20mg Cápsula", Brand = "Prilosec", GenericComponent = "Omeprazol", Presentation = "Caja x 14", MedicationType = "Cápsula" },
                new MedicationItemDTO { Id = 5, Name = "Diclofenaco 75mg Ampolla", Brand = "Voltaren", GenericComponent = "Diclofenaco Sódico", Presentation = "Caja x 5 Ampollas", MedicationType = "Inyección" },
                new MedicationItemDTO { Id = 6, Name = "Loratadina 10mg Tableta", Brand = "Claritin", GenericComponent = "Loratadina", Presentation = "Caja x 20", MedicationType = "Tableta" },
                new MedicationItemDTO { Id = 7, Name = "Dexametasona 8mg Ampolla", Brand = "Alin", GenericComponent = "Dexametasona Fosfato", Presentation = "Caja x 3 Ampollas", MedicationType = "Inyección" },
                new MedicationItemDTO { Id = 8, Name = "Salbutamol 100mcg Inhalador", Brand = "Ventolin", GenericComponent = "Salbutamol Sulfato", Presentation = "Frasco 200 dosis", MedicationType = "Aerosol" },
                new MedicationItemDTO { Id = 9, Name = "Metformina 850mg Tableta", Brand = "Glucophage", GenericComponent = "Metformina Clorhidrato", Presentation = "Caja x 30", MedicationType = "Tableta" }
            };

            return BaseResponse<List<MedicationItemDTO>>.Success(list, "Catálogo de medicamentos cargado con éxito.");
        }
    }
}
