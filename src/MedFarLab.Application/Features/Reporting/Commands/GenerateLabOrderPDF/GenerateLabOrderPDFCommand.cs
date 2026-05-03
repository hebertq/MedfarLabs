using MediatR;
using System.Net.Http.Json;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedFarLab.Application.Features.Reporting.DTOs;
using MedfarLabs.Core.Domain.Models.Reporting;

namespace MedFarLab.Application.Features.Reporting.Commands.GenerateLabOrderPDF
{
    public class GenerateLabOrderPDFCommand : IRequest<BaseResponse<ReportResponseDTO>>
    {
        public LabOrderReportModel RequestDto { get; set; }

        public GenerateLabOrderPDFCommand(LabOrderReportModel requestDto)
        {
            RequestDto = requestDto;
        }
    }

    public class GenerateLabOrderPDFCommandHandler : IRequestHandler<GenerateLabOrderPDFCommand, BaseResponse<ReportResponseDTO>>
    {
        private readonly global::System.Net.Http.IHttpClientFactory _httpClientFactory;

        public GenerateLabOrderPDFCommandHandler(global::System.Net.Http.IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<BaseResponse<ReportResponseDTO>> Handle(GenerateLabOrderPDFCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ReportingApi");
                var response = await client.PostAsJsonAsync("api/Report/11004", request.RequestDto);
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
