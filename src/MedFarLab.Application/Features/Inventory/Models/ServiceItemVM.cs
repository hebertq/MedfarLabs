namespace MedFarLab.Application.Features.Inventory.Models
{
    public class ServiceItemVM
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public bool IsTaxable { get; set; } = true;
        public string Status { get; set; } = "Activo";

        public override string ToString()
        {
            return $"{Code} - {Name}";
        }
    }
}
