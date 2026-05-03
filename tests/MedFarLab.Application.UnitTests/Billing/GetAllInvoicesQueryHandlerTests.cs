using FluentAssertions;
using MedFarLab.Application.Features.Billing.Queries;
using MedFarLab.Application.Features.Billing.DTOs;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Domain.Interfaces.Http;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MedFarLab.Application.UnitTests.Billing
{
    public class GetAllInvoicesQueryHandlerTests
    {
        private readonly Mock<IExternalServiceClient> _mockApiClient;
        private readonly GetAllInvoicesQueryHandler _handler;

        public GetAllInvoicesQueryHandlerTests()
        {
            _mockApiClient = new Mock<IExternalServiceClient>();
            _handler = new GetAllInvoicesQueryHandler(_mockApiClient.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnMappedDtoList()
        {
            // Arrange
            var mockData = new List<InvoiceDto>
            {
                new InvoiceDto { InvoiceNumber = "F001-0001", Status = "Unknown" },
                new InvoiceDto { InvoiceNumber = "F001-0002", Status = "Pendiente" }
            };

            var expectedResponse = BaseResponse<List<InvoiceDto>>.Success(mockData);

            _mockApiClient.Setup(r => r.GetAsync<List<InvoiceDto>>(It.IsAny<string>()))
                          .ReturnsAsync(expectedResponse);

            var query = new GetAllInvoicesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].InvoiceNumber.Should().Be("F001-0001");
            result[0].Status.Should().Be("Unknown");
        }
    }
}

