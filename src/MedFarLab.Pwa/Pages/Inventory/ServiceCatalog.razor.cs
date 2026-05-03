using Microsoft.AspNetCore.Components;
using MediatR;
using MedFarLab.Application.Features.Inventory.Models;
using MedFarLab.Application.Features.Inventory.Queries.GetServiceCatalogQuery;

namespace MedFarLab.Pwa.Pages.Inventory
{
    public partial class ServiceCatalog : ComponentBase
    {
        [Inject] private IMediator Mediator { get; set; } = default!;
        [Inject] private NavigationManager NavManager { get; set; } = default!;
        [Inject] private MedFarLab.Pwa.State.AppState AppState { get; set; } = default!;

        protected List<ServiceItemVM> Services { get; set; } = new();
        protected bool IsLoading { get; set; } = true;
        protected string SearchString { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadCatalog();
        }

        private async Task LoadCatalog()
        {
            IsLoading = true;
            var response = await Mediator.Send(new GetServiceCatalogQuery(AppState.CurrentTenantRoute));

            if (response != null && response.IsSuccess && response.Data != null)
            {
                Services = response.Data;
            }
            IsLoading = false;
        }

        protected bool FilterFunc(ServiceItemVM item)
        {
            if (string.IsNullOrWhiteSpace(SearchString))
                return true;
            if (item.Name.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (item.Code.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (item.Category.Contains(SearchString, StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        protected void OpenNewServiceModal()
        {
            // Placeholder para modal de nuevo servicio
        }

        protected void GoBack()
        {
            NavManager.NavigateTo("/billing/new");
        }

        protected void OpenSampleConfig(ServiceItemVM item)
        {
            NavManager.NavigateTo($"/laboratory/config/samples/{item.Id}");
        }
    }
}
