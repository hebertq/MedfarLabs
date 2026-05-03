using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Laboratory.Commands.ReceiveSample
{
    public class ReceiveSampleCommand : IRequest<BaseResponse<bool>>
    {
        public long Id { get; set; }

        public ReceiveSampleCommand(long id)
        {
            Id = id;
        }
    }

    public class ReceiveSampleCommandHandler : IRequestHandler<ReceiveSampleCommand, BaseResponse<bool>>
    {
        private readonly IExternalServiceClient _apiClient;

        public ReceiveSampleCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<bool>> Handle(ReceiveSampleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return await _apiClient.PostAsync<object, bool>("api/Laboratory/8003", new { id = request.Id }) ?? new BaseResponse<bool> { IsSuccess = false, Message = "API Response was null" };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { IsSuccess = false, Message = ex.Message };
            }
        }
    }
}
