using CheckerApi.Models;

namespace CheckerApi.Services.Interfaces
{
    public interface INotificationManager
    {
        Result TriggerHook(params string[] messages);
    }
}
