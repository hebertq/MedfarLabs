using Microsoft.AspNetCore.Components;
using MediatR;
using MedFarLab.Application.Features.Laboratory.Queries.GetLabDashboard;
using MedFarLab.Application.Features.Laboratory.Models;
using MudBlazor;

namespace MedFarLab.Pwa.Pages.Laboratory
{
    public partial class LabDashboard : ComponentBase
    {
        [Inject] private IMediator Mediator { get; set; } = default!;

        protected LabDashboardVM Model { get; set; } = new();

        protected bool ShowUploadModal { get; set; }
        protected bool IsSending { get; set; }
        protected long CurrentOrderId { get; set; }
        protected string UploadNotes { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            var query = new GetLabDashboardQuery { BranchId = 1, Date = DateTime.Today };
            var response = await Mediator.Send(query);

            if (response != null && response.IsSuccess && response.Data != null)
            {
                Model = response.Data;
            }
            else
            {
                Model = new LabDashboardVM();
            }
        }

        protected void OpenUploadModal(long orderId)
        {
            CurrentOrderId = orderId;
            UploadNotes = string.Empty;
            ShowUploadModal = true;
        }

        protected void CloseUploadModal()
        {
            ShowUploadModal = false;
        }

        protected async Task HandleUploadResults()
        {
            IsSending = true;
            StateHasChanged();

            await Task.Delay(1000); 

            IsSending = false;
            CloseUploadModal();
            StateHasChanged();
        }

        protected void ItemUpdated(MudItemDropInfo<LabPendingQueueVM> dropItem)
        {
            if (dropItem.Item != null)
            {
                dropItem.Item.Status = dropItem.DropzoneIdentifier;

                if (dropItem.Item.Status == "Completado")
                {
                    OpenUploadModal(dropItem.Item.OrderId);
                }
                
                // Fire and forget MediatR command to backend API proxy
                _ = Mediator.Send(new MedFarLab.Application.Features.Laboratory.Commands.UpdateLabOrderStatus.UpdateLabOrderStatusCommand(dropItem.Item.OrderId, dropItem.Item.Status));
            }
        }
    }
}
