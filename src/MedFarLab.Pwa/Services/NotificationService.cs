using System.Timers;

namespace MedFarLab.Pwa.Services;

public class ToastMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "info"; // "success", "error", "info"
}

public class NotificationService
{
    public event Action? OnChange;
    public List<ToastMessage> Messages { get; private set; } = new List<ToastMessage>();

    public void Show(string message, string type = "info")
    {
        var toast = new ToastMessage { Message = message, Type = type };
        Messages.Add(toast);
        NotifyStateChanged();

        DisposeToastAsync(toast);
    }

    public void Remove(ToastMessage toast)
    {
        if (Messages.Contains(toast))
        {
            Messages.Remove(toast);
            NotifyStateChanged();
        }
    }

    private async void DisposeToastAsync(ToastMessage toast)
    {
        await Task.Delay(4000); // 4 segundos de vida
        Remove(toast);
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
