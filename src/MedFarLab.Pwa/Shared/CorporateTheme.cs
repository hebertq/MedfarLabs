using MudBlazor;

namespace MedFarLab.Pwa.Shared
{
    public static class CorporateTheme
    {
        public static MudTheme ModernTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = "#1A56DB", // Corporate Trust Blue
                Secondary = "#059669", // Emerald Fresh
                Tertiary = "#8B5CF6", // Purple accent
                Background = "#F3F4F6", // Light modern gray
                AppbarBackground = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                Surface = "#FFFFFF",
                Warning = "#F59E0B",
                Error = "#E11D48",
                Success = "#10B981"
            },
            LayoutProperties = new LayoutProperties()
            {
                DefaultBorderRadius = "12px",
            },
            Typography = new Typography()
            {
                Default = new Default() { FontFamily = new[] { "Inter", "Helvetica", "Arial", "sans-serif" } },
                Button = new Button() { TextTransform = "none" }
            }
        };
    }
}
