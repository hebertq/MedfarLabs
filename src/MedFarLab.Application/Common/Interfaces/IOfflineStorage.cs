using MedFarLab.Application.Common.Command;

namespace MedFarLab.Application.Common.Interfaces
{
    public interface IOfflineStorage
    {
        /// <summary>
        /// Guarda un comando cifrado en la cola local del navegador.
        /// </summary>
        Task SavePending(PendingCommand command);

        /// <summary>
        /// Recupera todos los comandos pendientes de sincronización.
        /// </summary>
        Task<IEnumerable<PendingCommand>> GetPendingCommands();

        /// <summary>
        /// Elimina un comando una vez que el API confirmó su recepción.
        /// </summary>
        Task RemovePending(Guid id);
    }
}
