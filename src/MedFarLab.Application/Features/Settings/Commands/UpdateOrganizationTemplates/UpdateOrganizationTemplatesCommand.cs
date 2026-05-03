using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using System.Threading;
using System.Threading.Tasks;
using MedfarLabs.Core.Domain.Interfaces.Repositories;
using System.Linq;
using MedfarLabs.Core.Domain.Enums;

namespace MedFarLab.Application.Features.Settings.Commands.UpdateOrganizationTemplates
{
    public class UpdateOrganizationTemplatesCommand : IRequest<BaseResponse<bool>>
    {
        public InvoiceFormatType DefaultInvoiceFormat { get; set; } = InvoiceFormatType.A4;
        public InvoiceTemplateStyle DefaultInvoiceTemplate { get; set; } = InvoiceTemplateStyle.Classic;
    }

    public class UpdateOrganizationTemplatesCommandHandler : IRequestHandler<UpdateOrganizationTemplatesCommand, BaseResponse<bool>>
    {
        private readonly IUnitOfWork _uow;

        public UpdateOrganizationTemplatesCommandHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<BaseResponse<bool>> Handle(UpdateOrganizationTemplatesCommand request, CancellationToken cancellationToken)
        {
            var orgs = await _uow.Organizations.GetAllAsync();
            var activeOrg = orgs.FirstOrDefault(x => x.IsActive);

            if (activeOrg == null)
            {
                return BaseResponse<bool>.Failure("No se encontró una organización activa.");
            }

            activeOrg.DefaultInvoiceFormat = request.DefaultInvoiceFormat;
            activeOrg.DefaultInvoiceTemplate = request.DefaultInvoiceTemplate;

            await _uow.Organizations.UpdateAsync(activeOrg);
            await _uow.SaveChangesAsync();

            return BaseResponse<bool>.Success(true, "Plantilla actualizada con éxito.");
        }
    }
}
