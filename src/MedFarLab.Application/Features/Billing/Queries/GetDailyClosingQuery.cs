using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Domain.Interfaces.Repositories.Billing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace MedFarLab.Application.Features.Billing.Queries
{
    public class GetDailyClosingQuery : IRequest<IEnumerable<DailyClosingRow>>
    {
        public long BranchId { get; set; }
        public global::System.DateTime Date { get; set; }
    }

    public class GetDailyClosingQueryHandler : IRequestHandler<GetDailyClosingQuery, IEnumerable<DailyClosingRow>>
    {
        private readonly IExternalServiceClient _apiClient;

        public GetDailyClosingQueryHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<DailyClosingRow>> Handle(GetDailyClosingQuery request, CancellationToken cancellationToken)
        {
            var dto = new GetDailyClosingRequestDTO { BranchId = request.BranchId, Date = request.Date };
            var payload = JsonSerializer.Serialize(dto);
            
            var response = await _apiClient.GetAsync<IEnumerable<DailyClosingRow>>($"api/Billing/{AppAction.Billling.GetDailyClosing}?payload={payload}");
            
            if (response != null && response.IsSuccess && response.Data != null)
            {
                return response.Data;
            }
            
            return new List<DailyClosingRow>();
        }
    }
}
