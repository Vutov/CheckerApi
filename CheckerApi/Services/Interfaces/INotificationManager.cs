using CheckerApi.Data;

namespace CheckerApi.Services.Interfaces
{
    public interface INotificationManager
    {
        Result TriggerHook(params string[] messages);
    }
}
