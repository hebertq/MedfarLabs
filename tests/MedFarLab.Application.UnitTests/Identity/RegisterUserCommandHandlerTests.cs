using FluentAssertions;
using MedfarLabs.Core.Application.Features.Identity.Dtos.Request;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using MedFarLab.Application.Features.Identity.Commands.RegisterUser;
using Moq;
using Xunit;
using Bogus;

namespace MedFarLab.Application.UnitTests.Identity
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly Mock<IExternalServiceClient> _mockApiClient;
        private readonly RegisterUserCommandHandler _handler;

        public RegisterUserCommandHandlerTests()
        {
            _mockApiClient = new Mock<IExternalServiceClient>();
            _handler = new RegisterUserCommandHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ShouldCallApiClient_AndReturnSuccessResponse()
        {
            // Arrange
            var fakerRequest = new Faker<UsuarioRequestDTO>()
                .CustomInstantiator(f => new UsuarioRequestDTO() {
                    PersonId = f.Random.Long(1, 100),
                    Username = f.Internet.UserName(),
                    Password = f.Internet.Password(),
                    IsActive = true
                }).Generate();

            var command = new RegisterUserCommand(fakerRequest);

            var expectedResponse = BaseResponse<object>.Success(99L, "Usuario creado exitosamente");

            _mockApiClient
                .Setup(c => c.PostAsync<UsuarioRequestDTO, object>(
                    It.Is<string>(url => url.Contains("api/Identity")), 
                    It.IsAny<UsuarioRequestDTO>(),
                    It.IsAny<string?>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(expectedResponse.Data);
            
            _mockApiClient.Verify(c => c.PostAsync<UsuarioRequestDTO, object>(
                It.IsAny<string>(), 
                It.IsAny<UsuarioRequestDTO>(),
                It.IsAny<string?>()), 
                Times.Once);
        }
    }
}
