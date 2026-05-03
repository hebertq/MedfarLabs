namespace MedFarLab.Application.Features.Pharmacy.Models
{
    public class PharmacyDashboardVM
    {
        public int LowStockAlerts { get; set; } = 0;
        public int NearExpiryAlerts { get; set; } = 0;
        public int PrescriptionsDispensedToday { get; set; } = 0;
        public List<InventoryAlertVM> InventoryAlerts { get; set; } = new();
    }

    public class InventoryAlertVM
    {
        public long ItemId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public string AlertType { get; set; } = string.Empty; // "LowStock", "Expiry"
        public string ExpiryDate { get; set; } = string.Empty;
    }
}
