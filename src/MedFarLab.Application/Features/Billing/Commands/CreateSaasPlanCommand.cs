using MediatR;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;

namespace MedFarLab.Application.Features.Billing.Commands
{
    public record CreateSaasPlanCommand : CreateSaasPlanRequestDTO, IRequest<BaseResponse<long>>
    {
    }

    public class CreateSaasPlanCommandHandler : IRequestHandler<CreateSaasPlanCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _apiClient;

        public CreateSaasPlanCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<long>> Handle(CreateSaasPlanCommand request, CancellationToken cancellationToken)
        {
            var response = await _apiClient.PostAsync<CreateSaasPlanCommand, long>($"api/Billing/{AppAction.Billling.CreateSaasPlan}", request);
            return response ?? new BaseResponse<long> { IsSuccess = false, Message = "API Request failed" };
        }
    }
}
