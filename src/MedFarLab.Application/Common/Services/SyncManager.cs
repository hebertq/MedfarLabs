using MedFarLab.Application.Common.Interfaces;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MediatR;
using Microsoft.JSInterop;
using System.Text.Json;
using MedFarLab.Application.Common.Serialization;

namespace MedFarLab.Application.Common.Services
{
    public class SyncManager
    {
        private readonly IOfflineStorage _storage;
        private readonly ISender _mediator; // MediatR
        private readonly IJSRuntime _js;

        public SyncManager(IOfflineStorage storage, ISender mediator, IJSRuntime js)
        {
            _storage = storage;
            _mediator = mediator;
            _js = js;
        }

        public async Task ProcessPendingSync()
        {
            // 1. Verificar conexión real
            var isOnline = await _js.InvokeAsync<bool>("navigator.onLine");
            if (!isOnline) return;

            // 2. Obtener lo que tenemos en el "bolsillo" (LocalStorage)
            var pendingCommands = await _storage.GetPendingCommands();

            foreach (var item in pendingCommands)
            {
                try
                {
                    // Buscamos el tipo de comando por nombre (Ej: RegisterAppointmentCommand)
                    // Optimizado: Solo escaneamos el ensamblado de la aplicación, no todo el AppDomain
                    var type = typeof(PwaJsonContext).Assembly.GetTypes()
                        .FirstOrDefault(t => t.Name == item.CommandTypeName);

                    if (type == null) continue;

                    // Deserializamos el comando original usando Source Generators
                    var command = JsonSerializer.Deserialize(item.EncryptedData, type, PwaSerializationConfig.Options);

                    if (command != null)
                    {
                        // ¡RE-INTENTAMOS! Esto llamará a tu Handler original
                        var response = await _mediator.Send(command);

                        // Si el API ya lo recibió con éxito, lo borramos de la PWA
                        var responseType = typeof(BaseResponse<>);
                        if (response != null)
                        {
                            var respType = response.GetType();
                            if (respType.IsGenericType && respType.GetGenericTypeDefinition() == responseType)
                            {
                                var isSuccessProp = respType.GetProperty("IsSuccess");
                                if (isSuccessProp != null && isSuccessProp.GetValue(response) is bool isSuccess && isSuccess)
                                {
                                    await _storage.RemovePending(item.Id);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sincronizando {item.CommandTypeName}: {ex.Message}");
                }
            }
        }
    }
}
