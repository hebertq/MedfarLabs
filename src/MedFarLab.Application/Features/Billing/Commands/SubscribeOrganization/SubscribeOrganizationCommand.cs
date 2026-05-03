using MediatR;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Billing.Commands.SubscribeOrganization
{
    public class SubscribeOrganizationPayload
    {
        public long OrganizationId { get; set; }
        public int PlanId { get; set; }
        public decimal MonthlyPrice { get; set; }
    }

    public record SubscribeOrganizationCommand(SubscribeOrganizationPayload Payload) : IRequest<BaseResponse<long>>;

    public class SubscribeOrganizationCommandHandler : IRequestHandler<SubscribeOrganizationCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _apiClient;

        public SubscribeOrganizationCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(SubscribeOrganizationCommand request, CancellationToken cancellationToken)
        {
            var reqDto = new SubscriptionRequestDTO
            {
                OrganizationId = request.Payload.OrganizationId,
                PlanId = request.Payload.PlanId
            };
            return await _apiClient.PostAsync<SubscriptionRequestDTO, long>($"api/Billing/{(int)MedfarLabs.Core.Domain.Const.AppAction.Billling.SuscribirOrganizacion}", reqDto);
        }
    }
}
