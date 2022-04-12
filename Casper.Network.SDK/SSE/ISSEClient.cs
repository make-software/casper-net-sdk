using System.Threading.Tasks;

namespace Casper.Network.SDK.SSE
{
    public interface ISSEClient
    {
        void AddEventCallback(EventType eventType, string name, EventCallback cb, 
                                int startFrom = int.MaxValue);

        bool RemoveEventCallback(EventType eventType, string name);

        void StartListening();

        Task StopListening();

        void Wait();
    }
}
