using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.Laboratory.Models;

namespace MedFarLab.Application.Features.Laboratory.Commands.SaveServiceSampleConfigs
{
    public class SaveServiceSampleConfigsCommand : IRequest<BaseResponse<bool>>
    {
        public long OrganizationId { get; set; }
        public long ServiceId { get; set; }
        public List<ServiceSampleConfigDTO> Configs { get; set; }

        public SaveServiceSampleConfigsCommand(long organizationId, long serviceId, List<ServiceSampleConfigDTO> configs)
        {
            OrganizationId = organizationId;
            ServiceId = serviceId;
            Configs = configs;
        }
    }

    public class SaveServiceSampleConfigsCommandHandler : IRequestHandler<SaveServiceSampleConfigsCommand, BaseResponse<bool>>
    {
        private readonly IExternalServiceClient _apiClient;

        public SaveServiceSampleConfigsCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<bool>> Handle(SaveServiceSampleConfigsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var payload = new
                {
                    OrganizationId = request.OrganizationId,
                    ServiceId = request.ServiceId,
                    Configs = request.Configs
                };

                return await _apiClient.PostAsync<object, bool>("api/Laboratory/8006", payload) ?? new BaseResponse<bool> { IsSuccess = false, Message = "API Response was null" };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { IsSuccess = false, Message = ex.Message };
            }
        }
    }
}
