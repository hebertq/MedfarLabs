using MudBlazor;

namespace MedFarLab.Pwa.Services
{
    public static class MedFarIconService
    {
        public static string Resolve(string? iconName)
        {
            if (string.IsNullOrEmpty(iconName)) return Icons.Material.Filled.Circle;
            var key = iconName.Replace("Icons.Material.Filled.", "");
            return key switch
            {
                "Dashboard"        => Icons.Material.Filled.Dashboard,
                "CalendarMonth"    => Icons.Material.Filled.CalendarMonth,
                "CalendarToday"    => Icons.Material.Filled.CalendarToday,
                "FolderShared"     => Icons.Material.Filled.FolderShared,
                "People"           => Icons.Material.Filled.People,
                "Person"           => Icons.Material.Filled.Person,
                "PersonAdd"        => Icons.Material.Filled.PersonAdd,
                "LocalPharmacy"    => Icons.Material.Filled.LocalPharmacy,
                "LocalHospital"    => Icons.Material.Filled.LocalHospital,
                "MedicalServices"  => Icons.Material.Filled.MedicalServices,
                "Inventory"        => Icons.Material.Filled.Inventory,
                "Science"          => Icons.Material.Filled.Science,
                "Biotech"          => Icons.Material.Filled.Biotech,
                "Receipt"          => Icons.Material.Filled.Receipt,
                "ReceiptLong"      => Icons.Material.Filled.ReceiptLong,
                "Medication"       => Icons.Material.Filled.Medication,
                "MonitorHeart"     => Icons.Material.Filled.MonitorHeart,
                "Notifications"    => Icons.Material.Filled.Notifications,
                "Settings"         => Icons.Material.Filled.Settings,
                "Lock"             => Icons.Material.Filled.Lock,
                "Business"         => Icons.Material.Filled.Business,
                "Group"            => Icons.Material.Filled.Group,
                "Print"            => Icons.Material.Filled.Print,
                "Schedule"         => Icons.Material.Filled.Schedule,
                "NoteAlt"          => Icons.Material.Filled.NoteAlt,
                "PriceChange"      => Icons.Material.Filled.PriceChange,
                "CreditCard"       => Icons.Material.Filled.CreditCard,
                "SupervisorAccount"=> Icons.Material.Filled.SupervisorAccount,
                "AccountCircle"    => Icons.Material.Filled.AccountCircle,
                "Key"              => Icons.Material.Filled.Key,
                "Speed"            => Icons.Material.Filled.Speed,
                "Loyalty"          => Icons.Material.Filled.Loyalty,
                _                  => Icons.Material.Filled.Circle
            };
        }
    }
}
