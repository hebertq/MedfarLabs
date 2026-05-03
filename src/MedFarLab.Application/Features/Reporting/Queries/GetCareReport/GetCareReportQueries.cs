using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces;
using MedfarLabs.Core.Domain.Models.Reporting;
using MedFarLab.Application.Features.Reporting.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace MedFarLab.Application.Features.Reporting.Queries.GetCareReport
{
    // DTO Returns Base64 byte array in ReportResponseDTO like Invoice
    
    // PRESCRIPTION
    public record GetPrescriptionReportQuery(PrescriptionReportModel Payload) : IRequest<BaseResponse<ReportResponseDTO>>;

    public class GetPrescriptionReportQueryHandler : IRequestHandler<GetPrescriptionReportQuery, BaseResponse<ReportResponseDTO>>
    {
        private readonly IReportGenerator _reportGenerator;

        public GetPrescriptionReportQueryHandler(IReportGenerator reportGenerator)
        {
            _reportGenerator = reportGenerator;
        }

        public async Task<BaseResponse<ReportResponseDTO>> Handle(GetPrescriptionReportQuery request, CancellationToken cancellationToken)
        {
            var format = request.Payload.Format == "Ticket" ? "PrescriptionTicket" : "PrescriptionA4";
            byte[] pdfBytes = await _reportGenerator.GenerateReportAsync(format, request.Payload);

            var reportResponse = new ReportResponseDTO
            {
                Base64Data = global::System.Convert.ToBase64String(pdfBytes),
                FileName = $"Receta_{request.Payload.PatientName.Replace(" ", "_")}_{(global::System.DateTime.Now):yyyyMMdd}.pdf",
                MimeType = "application/pdf"
            };

            return BaseResponse<ReportResponseDTO>.Success(reportResponse);
        }
    }

    // LAB ORDER
    public record GetLabOrderReportQuery(LabOrderReportModel Payload) : IRequest<BaseResponse<ReportResponseDTO>>;

    public class GetLabOrderReportQueryHandler : IRequestHandler<GetLabOrderReportQuery, BaseResponse<ReportResponseDTO>>
    {
        private readonly IReportGenerator _reportGenerator;

        public GetLabOrderReportQueryHandler(IReportGenerator reportGenerator)
        {
            _reportGenerator = reportGenerator;
        }

        public async Task<BaseResponse<ReportResponseDTO>> Handle(GetLabOrderReportQuery request, CancellationToken cancellationToken)
        {
            var format = request.Payload.Format == "Ticket" ? "LabOrderTicket" : "LabOrderA4";
            byte[] pdfBytes = await _reportGenerator.GenerateReportAsync(format, request.Payload);

            var reportResponse = new ReportResponseDTO
            {
                Base64Data = global::System.Convert.ToBase64String(pdfBytes),
                FileName = $"OrdenLaboratorio_{request.Payload.PatientName.Replace(" ", "_")}_{(global::System.DateTime.Now):yyyyMMdd}.pdf",
                MimeType = "application/pdf"
            };

            return BaseResponse<ReportResponseDTO>.Success(reportResponse);
        }
    }
}
