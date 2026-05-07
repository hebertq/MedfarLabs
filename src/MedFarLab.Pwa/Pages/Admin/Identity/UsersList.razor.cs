using Microsoft.AspNetCore.Components;
using MudBlazor;
using MedFarLab.Pwa.Services;

namespace MedFarLab.Pwa.Pages.Admin.Identity
{
    public partial class UsersList : ComponentBase
    {
        [Inject] private NavigationManager NavManager { get; set; } = default!;
        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private MedFarSnackbarService SnackbarService { get; set; } = default!;

        protected List<MockUser> mockUsers = new()
        {
            new MockUser { Id = 1, Email = "admin@medfarlab.com", Name = "Super Admin Internal", PhoneNumber = "555-0000", OrgId = 1 },
            new MockUser { Id = 2, Email = "dr.lucas@sanlucas.com", Name = "Dr. Lucas P.", PhoneNumber = "555-1234", OrgId = 1 },
            new MockUser { Id = 3, Email = "qfb.ana@biotest.com", Name = "Ana Martinez (QFB)", PhoneNumber = "555-9876", OrgId = 2 }
        };

        protected bool IsLoading { get; set; } = false;

        public class MockUser
        {
            public long Id { get; set; }
            public string Email { get; set; } = "";
            public string Name { get; set; } = "";
            public string PhoneNumber { get; set; } = "";
            public long OrgId { get; set; }
        }

        protected bool FilterFunc(MockUser item, string searchString)
        {
            if (string.IsNullOrWhiteSpace(searchString)) return true;
            if (item.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
            if (item.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        protected async Task OpenAddModal()
        {
            var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Pages.Admin.Components.UserDialog>("Crear Usuario Interno");
            var result = await dialog.Result;

            if (!result.Canceled && result.Data != null)
            {
                dynamic data = result.Data;
                mockUsers.Add(new MockUser { Id = mockUsers.Max(x => x.Id) + 1, Email = data.Email, Name = data.Name, PhoneNumber = data.PhoneNumber, OrgId = data.OrgId });
                SnackbarService.ShowSuccess("Usuario agregado correctamente");
                StateHasChanged();
            }
        }

        protected async Task OpenEditModal(MockUser user)
        {
            var parameters = new DialogParameters { ["Email"] = user.Email, ["Name"] = user.Name, ["PhoneNumber"] = user.PhoneNumber, ["OrgId"] = user.OrgId };
            var dialog = await DialogService.ShowAsync<MedFarLab.Pwa.Pages.Admin.Components.UserDialog>("Editar Usuario", parameters);
            var result = await dialog.Result;

            if (!result.Canceled && result.Data != null)
            {
                dynamic data = result.Data;
                user.Email = data.Email;
                user.Name = data.Name;
                user.PhoneNumber = data.PhoneNumber;
                user.OrgId = data.OrgId;
                SnackbarService.ShowSuccess("Usuario actualizado correctamente");
                StateHasChanged();
            }
        }

        protected async Task DeleteUser(MockUser user)
        {
            bool? result = await DialogService.ShowMessageBox(
                "Eliminar Acceso", 
                $"¿Dar de baja a {user.Name} del sistema definitivamente?", 
                yesText: "Sí, borrar", cancelText: "Cancelar");

            if (result == true)
            {
                mockUsers.Remove(user);
                SnackbarService.ShowError("Acceso removido del registro.");
                StateHasChanged();
            }
        }
    }
}
