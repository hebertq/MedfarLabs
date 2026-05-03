using MediatR;
using MedfarLabs.Core.Domain.Const;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Application.Features.Billing.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;

namespace MedFarLab.Application.Features.Billing.Commands
{
    public record UpdateSaasPlanCommand : UpdateSaasPlanRequestDTO, IRequest<BaseResponse<bool>>
    {
    }

    public class UpdateSaasPlanCommandHandler : IRequestHandler<UpdateSaasPlanCommand, BaseResponse<bool>>
    {
        private readonly IExternalServiceClient _apiClient;

        public UpdateSaasPlanCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<bool>> Handle(UpdateSaasPlanCommand request, CancellationToken cancellationToken)
        {
            var response = await _apiClient.PostAsync<UpdateSaasPlanCommand, bool>($"api/Billing/{AppAction.Billling.UpdateSaasPlan}", request);
            return response ?? new BaseResponse<bool> { IsSuccess = false, Message = "API Request failed" };
        }
    }
}
