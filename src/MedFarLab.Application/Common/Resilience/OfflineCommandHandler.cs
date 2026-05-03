using MedFarLab.Application.Common.Command;
using MedFarLab.Application.Common.Interfaces;
using MedfarLabs.Core.Domain.Common.Responses.Generic;
using MedfarLabs.Core.Infrastructure.Common.Interfaces;
using MediatR;
using System.Text.Json;
using MedFarLab.Application.Common.Serialization;

namespace MedFarLab.Application.Common.Resilience
{
    public class OfflineCommandHandler<TRequest>
    where TRequest : IRequest<BaseResponse<object>>
    {
        private readonly IOfflineStorage _storage;
        private readonly IEncryptionService _encryption;

        public OfflineCommandHandler(IOfflineStorage storage, IEncryptionService encryption)
        {
            _storage = storage;
            _encryption = encryption;
        }

        public async Task<BaseResponse<object>> ProcessOffline(TRequest request)
        {
            // 1. Serializamos el comando que MediatR no pudo enviar
            var json = JsonSerializer.Serialize(request, typeof(TRequest), PwaSerializationConfig.Options);

            // 2. Ciframos con tu AesEncryptionService (Nivel Médico)
            var encrypted = _encryption.Encrypt(json);
            var encryptedBase64 = Convert.ToBase64String(encrypted);

            // 3. Guardamos metadatos para que el SyncManager sepa qué Handler disparar luego
            await _storage.SavePending(new PendingCommand(
                Guid.NewGuid(),
                typeof(TRequest).Name, // Guardamos el tipo de comando
                encryptedBase64,
                DateTime.UtcNow
            ));

            return BaseResponse<object>.Success(
                null,
                "MODO OFFLINE: Los datos médicos se han cifrado y guardado localmente. Se sincronizarán al detectar conexión."
            );
        }
    }
}
