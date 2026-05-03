using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Response;
using System.Threading;
using System.Threading.Tasks;

namespace MedFarLab.Application.Features.Billing.Commands
{
    public class CloseBillingPeriodCommand : IRequest<CloseBillingPeriodResponseDTO>
    {
        public int? BranchId { get; set; }
        public global::System.DateTime? EndDate { get; set; }
    }

    public class CloseBillingPeriodCommandHandler : IRequestHandler<CloseBillingPeriodCommand, CloseBillingPeriodResponseDTO>
    {
        private readonly IExternalServiceClient _apiClient;

        public CloseBillingPeriodCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<CloseBillingPeriodResponseDTO> Handle(CloseBillingPeriodCommand request, CancellationToken cancellationToken)
        {
            var dto = new CloseBillingPeriodRequestDTO(); 
            var response = await _apiClient.PostAsync<CloseBillingPeriodRequestDTO, CloseBillingPeriodResponseDTO>($"api/Billing/{AppAction.Billling.CerrarPeriodo}", dto);
            
            return response?.Data;
        }
    }
}
