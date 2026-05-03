using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MediatR;
using MedFarLab.Application.Features.Patient.Models;

namespace MedFarLab.Application.Features.Patient.Queries.GetPatientRecord
{
    public class GetPatientRecordQuery : IRequest<BaseResponse<PatientRecordVM>>
    {
        public long PatientId { get; set; }
        public long OrganizationId { get; set; }
    }

    public class GetPatientRecordQueryHandler : IRequestHandler<GetPatientRecordQuery, BaseResponse<PatientRecordVM>>
    {
        private readonly MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient _apiClient;

        public GetPatientRecordQueryHandler(MedfarLabs.Core.Domain.Interfaces.Http.IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<BaseResponse<PatientRecordVM>> Handle(GetPatientRecordQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payload = new { patient_id = request.PatientId, organization_id = request.OrganizationId };
                var response = await _apiClient.PostAsync<object, MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.PatientRecordResponseDTO>(
                    "api/Clinical/4011", payload
                );

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    var dto = response.Data;
                    var vm = new PatientRecordVM
                    {
                        PatientId = dto.PatientId,
                        FullName = dto.FullName,
                        Identifier = dto.Identifier,
                        Age = dto.Age,
                        Gender = dto.Gender,
                        BloodType = dto.BloodType,
                        Allergies = dto.Allergies ?? new List<string>(),
                        BloodPressureSystolic = dto.BloodPressureSystolic,
                        BloodPressureDiastolic = dto.BloodPressureDiastolic,
                        VitalsLabels = dto.VitalsLabels
                    };

                    if (dto.Antecedents != null)
                    {
                        vm.Antecedents = dto.Antecedents.Select(a => new AntecedentVM
                        {
                            Id = a.Id,
                            TypeId = a.TypeId,
                            TypeName = a.TypeName,
                            Description = a.Description
                        }).ToList();
                    }

                    if (dto.Consultations != null)
                    {
                        vm.Consultations = dto.Consultations.Select(c => new ClinicalHistoryItemVM
                        {
                            ConsultationId = c.ConsultationId,
                            DoctorUserId = c.DoctorUserId,
                            StatusId = c.StatusId,
                            Date = c.Date,
                            Title = c.Title,
                            Summary = c.Summary,
                            DoctorName = c.DoctorName,
                            IsOwner = c.IsOwner
                        }).ToList();
                    }

                    if (dto.ActivePrescriptions != null)
                    {
                        vm.ActivePrescriptions = dto.ActivePrescriptions.Select(p => new ActivePrescriptionVM
                        {
                            MedicationName = p.MedicationName,
                            Dosage = p.Dosage,
                            Instructions = p.Instructions,
                            IconColor = p.IconColor
                        }).ToList();
                    }

                    if (dto.Consents != null)
                    {
                        vm.Consents = dto.Consents.Select(c => new PatientConsentVM
                        {
                            Id = c.Id,
                            TypeId = c.TypeId,
                            TypeName = c.TypeName,
                            DigitalFormUrl = c.DigitalFormUrl,
                            SignedAt = c.SignedAt
                        }).ToList();
                    }

                    return BaseResponse<PatientRecordVM>.Success(vm, "Expediente cargado con éxito.");
                }
                
                return BaseResponse<PatientRecordVM>.Failure(response?.Message ?? "Error obteniendo datos.");
            }
            catch (Exception ex)
            {
                return BaseResponse<PatientRecordVM>.Failure(ex.Message);
            }
        }
    }
}
