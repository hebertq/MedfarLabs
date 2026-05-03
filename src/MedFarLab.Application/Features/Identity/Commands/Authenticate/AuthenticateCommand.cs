using MediatR;
using MedFarLab.Application.Features.Identity.Models;
using MedfarLabs.Core.Domain.Interfaces.Http;

namespace MedFarLab.Application.Features.Identity.Commands.Authenticate
{
    public class AuthenticateCommand : IRequest<AuthProfileResponse?>
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public AuthenticateCommand(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }

    public class AuthenticateCommandHandler : IRequestHandler<AuthenticateCommand, AuthProfileResponse?>
    {
        private readonly IExternalServiceClient _apiClient;

        public AuthenticateCommandHandler(IExternalServiceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<AuthProfileResponse?> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var payload = new { username = request.Username, password = request.Password };
                var response = await _apiClient.PostAsync<object, AuthProfileResponse>("api/Auth/Login", payload);

                if (response != null && response.IsSuccess && response.Data != null)
                {
                    return response.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auth Error: {ex.Message}");
                throw;
            }
        }
    }
}
