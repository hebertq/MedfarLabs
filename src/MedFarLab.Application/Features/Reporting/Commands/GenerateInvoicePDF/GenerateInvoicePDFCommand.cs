using MediatR;
using System.Net.Http.Json;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.Reporting.DTOs;

namespace MedFarLab.Application.Features.Reporting.Commands.GenerateInvoicePDF
{
    public class GenerateInvoicePDFCommand : IRequest<BaseResponse<ReportResponseDTO>>
    {
        public InvoiceReportRequestDTO RequestDto { get; set; }

        public GenerateInvoicePDFCommand(InvoiceReportRequestDTO requestDto)
        {
            RequestDto = requestDto;
        }
    }

    public class GenerateInvoicePDFCommandHandler : IRequestHandler<GenerateInvoicePDFCommand, BaseResponse<ReportResponseDTO>>
    {
        private readonly global::System.Net.Http.IHttpClientFactory _httpClientFactory;

        public GenerateInvoicePDFCommandHandler(global::System.Net.Http.IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse<ReportResponseDTO>> Handle(GenerateInvoicePDFCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ReportingApi");
                var response = await client.PostAsJsonAsync("api/Report/11001", request.RequestDto);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<BaseResponse<ReportResponseDTO>>() ?? new BaseResponse<ReportResponseDTO> { IsSuccess = false, Message = "Null response" };
                }
                return new BaseResponse<ReportResponseDTO> { IsSuccess = false, Message = await response.Content.ReadAsStringAsync() };
            }
            catch (Exception ex)
            {
                return new BaseResponse<ReportResponseDTO> { IsSuccess = false, Message = ex.Message };
            }
        }
    }
}
