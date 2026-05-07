using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MedFarLab.Pwa.Services
{
    public class MedFarThemeService
    {
        public bool IsDarkMode { get; private set; } = false;
        public event Action? OnChange;

        public void Toggle()
        {
            IsDarkMode = !IsDarkMode;
            OnChange?.Invoke();
        }
    }

    public static class MedFarTheme
    {
        public static MudTheme Build() => new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#10B981",
                PrimaryDarken = "#059669",
                PrimaryLighten = "#D1FAE5",
                Secondary = "#6366F1",
                Tertiary = "#F97316",
                Background = "#F0FDF4",
                Surface = "#FFFFFF",
                AppbarBackground = "rgba(255,255,255,0.80)",
                DrawerBackground = "rgba(255,255,255,0.75)",
                Success = "#22C55E",
                Warning = "#F97316",
                Error = "#EF4444",
                Info = "#3B82F6",
                TextPrimary = "#111827",
                TextSecondary = "#6B7280"
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#4ADE80",
                PrimaryDarken = "#16A34A",
                PrimaryLighten = "rgba(74,222,128,0.15)",
                Secondary = "#818CF8",
                Background = "#121218",
                Surface = "#1E1E2E",
                AppbarBackground = "rgba(30,30,46,0.85)",
                DrawerBackground = "rgba(30,30,46,0.80)",
                TextPrimary = "#F9FAFB",
                TextSecondary = "#9CA3AF"
            },
            Typography = new Typography
            {
                Default = new Default
                {
                    FontFamily = new[] { "Inter", "-apple-system", "sans-serif" },
                    FontSize = "0.875rem",
                    LineHeight = 1.5
                },
                H5 = new H5 { FontWeight = 700, FontSize = "1.125rem" },
                H6 = new H6 { FontWeight = 600, FontSize = "1rem" }
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "12px",
                DrawerWidthLeft = "260px",
                AppbarHeight = "64px"
            }
        };
    }
}
