using Microsoft.AspNetCore.Components;
using MediatR;
using MedFarLab.Application.Features.Pharmacy.Queries.GetPharmacyDashboard;
using MedFarLab.Application.Features.Pharmacy.Models;

namespace MedFarLab.Pwa.Pages.Pharmacy
{
    public partial class PharmacyDashboard : ComponentBase
    {
        [Inject] private IMediator Mediator { get; set; } = default!;

        protected PharmacyDashboardVM Model { get; set; } = new();

        protected bool ShowRestockModal { get; set; }
        protected bool IsSending { get; set; }
        protected bool IsLoading { get; set; } = true;
        protected long CurrentItemId { get; set; }
        protected int RestockQuantity { get; set; }

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            var query = new GetPharmacyDashboardQuery { BranchId = 1 };
            var response = await Mediator.Send(query);

            if (response != null && response.IsSuccess && response.Data != null)
            {
                Model = response.Data;
            }
            else
            {
                Model = new PharmacyDashboardVM();
            }
            IsLoading = false;
        }

        protected void OpenRestockModal(long itemId)
        {
            CurrentItemId = itemId;
            RestockQuantity = 0;
            ShowRestockModal = true;
        }

        protected void CloseRestockModal()
        {
            ShowRestockModal = false;
        }

        protected async Task HandleRestockSubmit()
        {
            IsSending = true;
            StateHasChanged();

            // Execute MediatR command (Proxies HTTP Client behind scenes)
            var response = await Mediator.Send(new MedFarLab.Application.Features.Pharmacy.Commands.RestockMedication.RestockMedicationCommand(CurrentItemId, RestockQuantity));

            if (response != null && response.IsSuccess)
            {
                // Reload dashboard data
                var query = new GetPharmacyDashboardQuery { BranchId = 1 };
                var dashResponse = await Mediator.Send(query);
                if (dashResponse != null && dashResponse.IsSuccess && dashResponse.Data != null)
                {
                    Model = dashResponse.Data;
                }
            } 

            IsSending = false;
            CloseRestockModal();
            StateHasChanged();
        }
    }
}
