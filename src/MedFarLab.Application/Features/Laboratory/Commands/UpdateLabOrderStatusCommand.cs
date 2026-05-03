using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace MedFarLab.Application.Features.Laboratory.Commands.UpdateLabOrderStatus
{
    public class UpdateLabOrderStatusCommand : IRequest<BaseResponse<bool>>
    {
        public long OrderId { get; set; }
        public string NewStatus { get; set; } = string.Empty;

        public UpdateLabOrderStatusCommand(long orderId, string newStatus)
        {
            OrderId = orderId;
            NewStatus = newStatus;
        }
    }

    public class UpdateLabOrderStatusCommandHandler : IRequestHandler<UpdateLabOrderStatusCommand, BaseResponse<bool>>
    {
        private readonly IExternalServiceClient _apiClient;

        public UpdateLabOrderStatusCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<bool>> Handle(UpdateLabOrderStatusCommand request, CancellationToken cancellationToken)
        {
            return await _apiClient.PostAsync<UpdateLabOrderStatusCommand, bool>($"api/Laboratory/{(int)MedfarLabs.Core.Domain.Const.AppAction.Laboratory.RegistrarOrden}", request);
        }
    }
}
