using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Application.Features.Laboratory.Dtos.Request;
using MedfarLabs.Core.Application.Features.Laboratory.Dtos.Response;

namespace MedFarLab.Application.Features.Laboratory.Queries.GetLabExamTemplates
{
    public class GetLabExamTemplatesQuery : IRequest<BaseResponse<IEnumerable<LabExamTemplateResponseDTO>>>
    {
    }

    public class GetLabExamTemplatesQueryHandler : IRequestHandler<GetLabExamTemplatesQuery, BaseResponse<IEnumerable<LabExamTemplateResponseDTO>>>
    {
        private readonly IExternalServiceClient _client;

        public GetLabExamTemplatesQueryHandler(IExternalServiceClient client)
        {
            _client = client;
        }

        public async Task<BaseResponse<IEnumerable<LabExamTemplateResponseDTO>>> Handle(GetLabExamTemplatesQuery request, CancellationToken cancellationToken)
        {
            var command = new GetLabExamTemplatesRequestDTO();

            return await _client.PostAsync<GetLabExamTemplatesRequestDTO, IEnumerable<LabExamTemplateResponseDTO>>(
                $"api/Laboratory/{(int)MedfarLabs.Core.Domain.Const.AppAction.Laboratory.ViewTemplate}", command);
        }
    }
}
