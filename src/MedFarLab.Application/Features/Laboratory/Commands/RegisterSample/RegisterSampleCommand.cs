using MediatR;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Laboratory.Commands.RegisterSample
{
    public class RegisterSampleCommand : IRequest<BaseResponse<object>>
    {
        public long PatientId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string SampleType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public RegisterSampleCommand(long patientId, string barcode, string sampleType, string notes)
        {
            PatientId = patientId;
            Barcode = barcode;
            SampleType = sampleType;
            Notes = notes;
        }
    }

    public class RegisterSampleCommandHandler : IRequestHandler<RegisterSampleCommand, BaseResponse<object>>
    {
        private readonly IExternalServiceClient _apiClient;

        public RegisterSampleCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<object>> Handle(RegisterSampleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var payload = new
                {
                    patientId = request.PatientId,
                    barcode = request.Barcode,
                    sampleType = request.SampleType,
                    notes = request.Notes
                };
                return await _apiClient.PostAsync<object, object>("api/Laboratory/8010", payload) ?? new BaseResponse<object> { IsSuccess = false, Message = "API Response was null" };
            }
            catch (Exception ex)
            {
                return new BaseResponse<object> { IsSuccess = false, Message = ex.Message };
            }
        }
    }
}
