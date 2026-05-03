using FluentAssertions;
using MedfarLabs.Core.Application.Features.Care.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.Care.Commands.RegisterConsultation;
using MedFarLab.Application.Features.Care.Models;
using Moq;
using Xunit;
using Bogus;

namespace MedFarLab.Application.UnitTests.Care
{
    public class RegisterConsultationCommandHandlerTests
    {
        private readonly Mock<IExternalServiceClient> _mockApiClient;
        private readonly RegisterConsultationCommandHandler _handler;

        public RegisterConsultationCommandHandlerTests()
        {
            _mockApiClient = new Mock<IExternalServiceClient>();
            _handler = new RegisterConsultationCommandHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ShouldCallApiClient_AndReturnSuccessResponse()
        {
            // Arrange
            var fakerPayload = new Faker<ConsultationRequestDTO>()
                .CustomInstantiator(f => new ConsultationRequestDTO(
                    ConsultationId: null,
                    MedicalRecordId: f.Random.Long(1, 100),
                    DoctorUserId: f.Random.Long(1, 100),
                    Subjective: f.Lorem.Sentence(),
                    Objective: f.Lorem.Sentence(),
                    Analysis: f.Lorem.Sentence(),
                    Plan: f.Lorem.Sentence(),
                    Vitals: null,
                    Diagnoses: new List<MedfarLabs.Core.Application.Features.Clinical.Dtos.Response.DiagnosisCodeDTO>(),
                    Prescriptions: new List<MedfarLabs.Core.Application.Features.Care.Dtos.Request.PrescriptionItemDTO>(),
                    LabOrders: new List<MedfarLabs.Core.Application.Features.Care.Dtos.Request.LabOrderDTO>()
                )).Generate();

            var command = new RegisterConsultationCommand(fakerPayload);

            var expectedResponse = BaseResponse<long>.Success(1024L, "Consulta registrada");

            _mockApiClient
                .Setup(c => c.PostAsync<ConsultationRequestDTO, long>(
                    It.Is<string>(url => url.Contains("api/Care")), 
                    It.IsAny<ConsultationRequestDTO>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(1024L);
            
            _mockApiClient.Verify(c => c.PostAsync<ConsultationRequestDTO, long>(
                It.Is<string>(url => url.Contains("api/Care")), 
                It.Is<ConsultationRequestDTO>(req => req.MedicalRecordId == fakerPayload.MedicalRecordId),
                It.IsAny<string?>()), 
                Times.Once);
        }
    }
}
