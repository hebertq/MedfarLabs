using FluentAssertions;
using MedfarLabs.Core.Application.Features.Clinical.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.Clinical.Commands.RegisterPatient;
using MedFarLab.Application.Features.Clinical.Models;
using Moq;
using Xunit;
using Bogus;

namespace MedFarLab.Application.UnitTests.Clinical
{
    public class RegisterPatientCommandHandlerTests
    {
        private readonly Mock<IExternalServiceClient> _mockApiClient;
        private readonly RegisterPatientCommandHandler _handler;

        public RegisterPatientCommandHandlerTests()
        {
            _mockApiClient = new Mock<IExternalServiceClient>();
            _handler = new RegisterPatientCommandHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ShouldCallApiClient_AndReturnSuccessResponse()
        {
            // Arrange
            var fakerPayload = new Faker<PatientVM>()
                .RuleFor(c => c.PersonId, f => f.Random.Long(1, 100))
                .RuleFor(c => c.InternalCode, f => f.Random.String2(5, "ABCDEFGHIJKLMNOPQRSTUVWXYZ"))
                .RuleFor(c => c.AuditNotes, f => f.Lorem.Sentence())
                .Generate();

            var command = new RegisterPatientCommand(fakerPayload);

            var expectedResponse = BaseResponse<long>.Success(777L, "Paciente registrado correctamente");

            _mockApiClient
                .Setup(c => c.PostAsync<PatientRequestDTO, long>(
                    It.Is<string>(url => url.Contains("api/Clinical")), 
                    It.IsAny<PatientRequestDTO>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(777L);
            
            _mockApiClient.Verify(c => c.PostAsync<PatientRequestDTO, long>(
                It.Is<string>(url => url.Contains("api/Clinical")), 
                It.Is<PatientRequestDTO>(req => req.PersonId == fakerPayload.PersonId),
                It.IsAny<string?>()), 
                Times.Once);
        }
    }
}
