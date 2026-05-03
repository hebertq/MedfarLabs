using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Application.Features.Laboratory.Dtos.Request;
using MedfarLabs.Core.Application.Features.Laboratory.Dtos.Response;

namespace MedFarLab.Application.Features.Laboratory.Queries.GetLabExamTemplateItems
{
    public class GetLabExamTemplateItemsQuery : IRequest<BaseResponse<IEnumerable<LabExamTemplateItemResponseDTO>>>
    {
        public long TemplateId { get; set; }
        public GetLabExamTemplateItemsQuery(long templateId) { TemplateId = templateId; }
    }

    public class GetLabExamTemplateItemsQueryHandler : IRequestHandler<GetLabExamTemplateItemsQuery, BaseResponse<IEnumerable<LabExamTemplateItemResponseDTO>>>
    {
        private readonly IExternalServiceClient _client;

        public GetLabExamTemplateItemsQueryHandler(IExternalServiceClient client)
        {
            _client = client;
        }

        public async Task<BaseResponse<IEnumerable<LabExamTemplateItemResponseDTO>>> Handle(GetLabExamTemplateItemsQuery request, CancellationToken cancellationToken)
        {
            var command = new GetLabExamTemplateItemsRequestDTO { TemplateId = request.TemplateId };

            return await _client.PostAsync<GetLabExamTemplateItemsRequestDTO, IEnumerable<LabExamTemplateItemResponseDTO>>(
                $"api/Laboratory/{(int)MedfarLabs.Core.Domain.Const.AppAction.Laboratory.ViewTemplateItems}", command);
        }
    }
}
