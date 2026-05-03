using MedFarLab.Application.Common.Command;
using MedFarLab.Application.Common.Interfaces;
using Blazored.LocalStorage;

namespace MedFarLab.Application.Common.Services
{
    public class BrowserOfflineStorage : IOfflineStorage
    {
        private readonly ILocalStorageService _localStorage;
        private const string StorageKey = "medfarlab_offline_queue";

        public BrowserOfflineStorage(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task SavePending(PendingCommand command)
        {
            var queue = await GetQueue();
            queue.Add(command);
            await _localStorage.SetItemAsync(StorageKey, queue);
        }

        public async Task<IEnumerable<PendingCommand>> GetPendingCommands()
        {
            return await GetQueue();
        }

        public async Task RemovePending(Guid id)
        {
            var queue = await GetQueue();
            queue.RemoveAll(x => x.Id == id);
            await _localStorage.SetItemAsync(StorageKey, queue);
        }

        private async Task<List<PendingCommand>> GetQueue()
        {
            return await _localStorage.GetItemAsync<List<PendingCommand>>(StorageKey)
                   ?? [];
        }
    }
}
