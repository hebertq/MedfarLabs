using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Laboratory.Commands.RejectSample
{
    public class RejectSampleCommand : IRequest<BaseResponse<bool>>
    {
        public long Id { get; set; }
        public string RejectionReason { get; set; } = string.Empty;

        public RejectSampleCommand(long id, string rejectionReason)
        {
            Id = id;
            RejectionReason = rejectionReason;
        }
    }

    public class RejectSampleCommandHandler : IRequestHandler<RejectSampleCommand, BaseResponse<bool>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RejectSampleCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<bool>> Handle(RejectSampleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return await _apiClient.PostAsync<object, bool>("api/Laboratory/8004", new { id = request.Id, rejectionReason = request.RejectionReason }) ?? new BaseResponse<bool> { IsSuccess = false, Message = "API Response was null" };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { IsSuccess = false, Message = ex.Message };
            }
        }
    }
}
