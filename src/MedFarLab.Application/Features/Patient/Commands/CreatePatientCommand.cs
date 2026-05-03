using MediatR;
using MedFarLab.Application.Features.Patient.DTOs;
using MedfarLabs.Core.Domain.Interfaces.Repositories.Clinical;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MedFarLab.Application.Features.Patient.Commands
{
    public class CreatePatientCommand : IRequest<long>
    {
        public PatientDto Patient { get; set; } = new();
    }

    public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, long>
    {
        private readonly IPatientRepository _repository;

        public CreatePatientCommandHandler(IPatientRepository repository)
        {
            _repository = repository;
        }

        public async Task<long> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Patient;

            var patientEntity = new MedfarLabs.Core.Domain.Entities.Clinical.Patient
            {
                InternalCode = string.IsNullOrWhiteSpace(dto.RecordId) ? $"REC-{new Random().Next(10000, 99999)}" : dto.RecordId,
                PersonId = 1,
                OrganizationId = 1,
                IsActive = true
            };

            var savedEntityId = await _repository.AddAsync(patientEntity);
            return savedEntityId;
        }
    }
}

