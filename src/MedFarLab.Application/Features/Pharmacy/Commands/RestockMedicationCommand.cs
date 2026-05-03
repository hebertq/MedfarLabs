using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace MedFarLab.Application.Features.Pharmacy.Commands.RestockMedication
{
    public class RestockMedicationCommand : IRequest<BaseResponse<bool>>
    {
        public long MedicationId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;

        public RestockMedicationCommand(long medicationId, int quantity, string reason = "Reabastecimiento General")
        {
            MedicationId = medicationId;
            Quantity = quantity;
            Reason = reason;
        }
    }

    public class RestockMedicationCommandHandler : IRequestHandler<RestockMedicationCommand, BaseResponse<bool>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RestockMedicationCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<bool>> Handle(RestockMedicationCommand request, CancellationToken cancellationToken)
        {
            // The Dashboard relies on this QueryHandler to execute the HTTP action 
            return await _apiClient.PostAsync<RestockMedicationCommand, bool>($"api/Pharmacy/{(int)MedfarLabs.Core.Domain.Const.AppAction.Inventory.AjustarStock}", request);
        }
    }
}
