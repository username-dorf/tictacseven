using FishNet;
using FishNet.Connection;

namespace Multiplayer.Connection
{
    public interface IHostConnectionProvider
    {
        bool TryGetConnection(out NetworkConnection conn);
    }
    public class HostConnectionProvider : IHostConnectionProvider
    {
        public bool TryGetConnection(out NetworkConnection conn)
        {
            conn = null;
            var cm = InstanceFinder.ClientManager;
            var sm = InstanceFinder.ServerManager;
            if (cm.Connection == null) 
                return false;

            var id = cm.Connection.ClientId;
            return sm.Clients.TryGetValue(id, out conn);
        } 
    }
}