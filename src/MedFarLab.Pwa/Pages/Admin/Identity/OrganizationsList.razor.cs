using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedFarLab.Pwa.Services;

namespace MedFarLab.Pwa.Pages.Admin.Identity
{
    public partial class OrganizationsList : ComponentBase
    {
        [Inject] private NavigationManager NavManager { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private MedFarSnackbarService SnackbarService { get; set; } = default!;

        protected List<MockOrg> mockOrgs = new()
        {
            new MockOrg { Id = 1, Title = "Hospital General San Lucas", Domain = "sanlucas.medfarlab.com", IsActive = true },
            new MockOrg { Id = 2, Title = "Laboratorios BioTest", Domain = "biotest.medfarlab.com", IsActive = true },
            new MockOrg { Id = 3, Title = "Clínica Dental Sonrisas", Domain = "dental.medfarlab.com", IsActive = false }
        };

        protected bool IsLoading { get; set; } = false;

        public class MockOrg
        {
            public long Id { get; set; }
            public string Title { get; set; } = "";
            public string Domain { get; set; } = "";
            public bool IsActive { get; set; }
        }

        protected bool FilterFunc(MockOrg item, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString)) return true;
            if (item.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
            if (item.Domain.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        protected async Task OpenAddModal()
        {
            var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Pages.Admin.Components.OrganizationDialog>("Crear Organización");
            var result = await dialog.Result;

            if (!result.Canceled && result.Data != null)
            {
                dynamic data = result.Data;
                mockOrgs.Add(new MockOrg { Id = mockOrgs.Max(x => x.Id) + 1, Title = data.Title, Domain = data.Domain, IsActive = data.IsActive });
                SnackbarService.ShowSuccess("Organización creada exitosamente");
                StateHasChanged();
            }
        }

        protected async Task OpenEditModal(MockOrg org)
        {
            var parameters = new DialogParameters { ["Title"] = org.Title, ["Domain"] = org.Domain, ["IsActive"] = org.IsActive };
            var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Pages.Admin.Components.OrganizationDialog>("Editar Organización", parameters);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data != null)
            {
                dynamic data = result.Data;
                org.Title = data.Title;
                org.Domain = data.Domain;
                org.IsActive = data.IsActive;
                SnackbarService.ShowSuccess("Se han guardado los cambios");
                StateHasChanged();
            }
        }

        protected async Task DeleteOrganization(MockOrg org)
        {
            bool? result = await DialogService.ShowMessageBox(
                "Confirmar Eliminación", 
                $"¿Estás seguro que deseas borrar la organización {org.Title}?", 
                yesText: "Borrar", cancelText: "Cancelar");

            if (result == true)
            {
                mockOrgs.Remove(org);
                SnackbarService.ShowError("Organización eliminada");
                StateHasChanged();
            }
        }
    }
}
