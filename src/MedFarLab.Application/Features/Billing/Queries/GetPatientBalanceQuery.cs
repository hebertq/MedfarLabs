using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace MedFarLab.Application.Features.Billing.Queries
{
    public class GetPatientBalanceQuery : IRequest<decimal>
    {
        public long PatientId { get; set; }
    }

    public class GetPatientBalanceQueryHandler : IRequestHandler<GetPatientBalanceQuery, decimal>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetPatientBalanceQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<decimal> Handle(GetPatientBalanceQuery request, CancellationToken cancellationToken)
        {
            var dto = new GetPatientBalanceRequestDTO { PatientId = request.PatientId };
            var payload = JsonSerializer.Serialize(dto);
            
            var response = await _apiClient.GetAsync<decimal>("api/Billing/$(AppAction.Billling.GetPatientBalance)?payload=$(payload)");
            
            if (response != null && response.IsSuccess)
            {
                return response.Data;
            }
            
            return 0;
        }
    }
}
