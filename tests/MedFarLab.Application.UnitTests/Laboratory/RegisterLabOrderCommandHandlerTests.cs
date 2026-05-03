using FluentAssertions;
using MedfarLabs.Core.Application.Features.Laboratory.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.Laboratory.Commands.RegisterLabOrder;
using Moq;
using Xunit;
using Bogus;

namespace MedFarLab.Application.UnitTests.Laboratory
{
    public class RegisterLabOrderCommandHandlerTests
    {
        private readonly Mock<IExternalServiceClient> _mockApiClient;
        private readonly RegisterLabOrderCommandHandler _handler;

        public RegisterLabOrderCommandHandlerTests()
        {
            _mockApiClient = new Mock<IExternalServiceClient>();
            _handler = new RegisterLabOrderCommandHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ShouldCallApiClient_AndReturnSuccessResponse()
        {
            // Arrange
            var fakerPayload = new Faker<LabOrderRequestDTO>()
                .CustomInstantiator(f => new LabOrderRequestDTO(
                    f.Random.Long(1, 100),
                    f.Random.Long(1, 100),
                    f.Random.Long(1, 100),
                    1,
                    null
                )).Generate();

            var command = new RegisterLabOrderCommand(fakerPayload);
            var expectedResponse = BaseResponse<object>.Success(2048L, "Orden de laboratorio registrada.");

            _mockApiClient
                .Setup(c => c.PostAsync<LabOrderRequestDTO, object>(
                    It.Is<string>(url => url.Contains("api/Laboratory")), 
                    It.IsAny<LabOrderRequestDTO>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(expectedResponse.Data);
            
            _mockApiClient.Verify(c => c.PostAsync<LabOrderRequestDTO, object>(
                It.Is<string>(url => url.Contains("api/Laboratory")), 
                It.IsAny<LabOrderRequestDTO>(),
                It.IsAny<string?>()), 
                Times.Once);
        }
    }
}
