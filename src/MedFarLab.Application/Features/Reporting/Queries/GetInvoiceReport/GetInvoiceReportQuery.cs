using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedFarLab.Application.Features.Reporting.DTOs;

namespace MedFarLab.Application.Features.Reporting.Queries.GetInvoiceReport
{
    public record GetInvoiceReportQuery(InvoiceReportRequestDTO Payload) : IRequest<BaseResponse<ReportResponseDTO>>;
}
