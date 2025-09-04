using System;
using Game.User;
using Multiplayer.Client;
using UniRx;

namespace Multiplayer.Server
{
    public interface IServerClientsRegister
    {
        int RegisterClient(ClientConnection client);
    }

    public interface IServerClientsProvider
    {
        ReactiveDictionary<int, ClientConnection> Clients { get; }
        public ReactiveDictionary<string,UserEntitiesModel> ClientEntitiesModels { get; }
        int GetClientOwnerValue(string clientId);
        void Drop();
    }
    public class ServerClientsProvider: IServerClientsProvider, IServerClientsRegister, IDisposable
    {
        public ReactiveDictionary<int,ClientConnection> Clients { get; private set; }
        public ReactiveDictionary<string,UserEntitiesModel> ClientEntitiesModels { get; private set; }
        
        public int GetClientOwnerValue(string clientId)
        {
            foreach (var (key, value) in Clients)
            {
                if (value.Preferences.id == clientId)
                    return key;
            }

            return 0;
        }

        public ServerClientsProvider()
        {
            Clients = new ReactiveDictionary<int,ClientConnection>();
            ClientEntitiesModels = new ReactiveDictionary<string, UserEntitiesModel>();
        }        
        public int RegisterClient(ClientConnection client)
        {
            var count = Clients.Count;
            var owner = count + 1;
            Clients.Add(owner, client);
            ClientEntitiesModels.Add(client.Preferences.id, new UserEntitiesModel(owner));
            return owner;
        }

        public void Drop()
        {
            ClientEntitiesModels?.Clear();
            foreach (var (key, value) in Clients)
            {
                ClientEntitiesModels!.Add(value.Preferences.id, new UserEntitiesModel(key));
            }
        }

        public void Dispose()
        {
            Clients?.Dispose();
            ClientEntitiesModels?.Dispose();
        }
    }
}