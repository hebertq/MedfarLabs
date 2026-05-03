using MediatR;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Application.Features.Laboratory.Dtos.Request;

namespace MedFarLab.Application.Features.Laboratory.Commands.CloneLabTemplate
{
    public class CloneLabTemplateCommand : IRequest<BaseResponse<long>>
    {
        public long OrganizationId { get; set; }
        public long TemplateId { get; set; }

        public CloneLabTemplateCommand(long orgId, long templateId)
        {
            OrganizationId = orgId;
            TemplateId = templateId;
        }
    }

    public class CloneLabTemplateCommandHandler : IRequestHandler<CloneLabTemplateCommand, BaseResponse<long>>
    {
        private readonly IExternalServiceClient _client;

        public CloneLabTemplateCommandHandler(IExternalServiceClient client)
        {
            _client = client;
        }

        public async Task<BaseResponse<long>> Handle(CloneLabTemplateCommand request, CancellationToken cancellationToken)
        {
            var command = new CloneTemplateRequestDTO { OrganizationId = request.OrganizationId, TemplateId = request.TemplateId };

            return await _client.PostAsync<CloneTemplateRequestDTO, long>(
                $"api/Laboratory/{(int)MedfarLabs.Core.Domain.Const.AppAction.Laboratory.CloneTemplate}", command);
        }
    }
}
