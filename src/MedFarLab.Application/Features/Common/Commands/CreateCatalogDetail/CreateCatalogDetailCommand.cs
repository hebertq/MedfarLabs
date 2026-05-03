using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Common.Commands.CreateCatalogDetail
{
    public class CreateCatalogDetailCommand : IRequest<BaseResponse<bool>>
    {
        public int CatalogId { get; set; }
        public string Name { get; set; } = string.Empty;

        public CreateCatalogDetailCommand(int catalogId, string name)
        {
            CatalogId = catalogId;
            Name = name;
        }
    }

    public class CreateCatalogDetailCommandHandler : IRequestHandler<CreateCatalogDetailCommand, BaseResponse<bool>>
    {
        private readonly IExternalServiceClient _apiClient;

        public CreateCatalogDetailCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<bool>> Handle(CreateCatalogDetailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var payload = new
                {
                    CatalogId = request.CatalogId,
                    Name = request.Name
                };

                return await _apiClient.PostAsync<object, bool>("api/Common/6003", payload) ?? new BaseResponse<bool> { IsSuccess = false, Message = "API Response was null" };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool> { IsSuccess = false, Message = ex.Message };
            }
        }
    }
}
