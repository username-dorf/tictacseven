using System;
using System.Collections.Generic;
using System.Linq;
using Multiplayer.Client;
using Multiplayer.Connection;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Multiplayer.Server
{
    public interface IServerActiveClientProvider
    {
        ReactiveProperty<string> ActiveClientId { get; }
        void ChangeActiveClientId();
    }
    public class ServerActiveClientProvider: IServerActiveClientProvider, IInitializable, IDisposable
    {
        private readonly IServerClientsProvider _clientsProvider;
        private CompositeDisposable _disposable;
        public ReactiveProperty<string> ActiveClientId { get; }
        
        public ServerActiveClientProvider(IServerClientsProvider clientsProvider)
        {
            _clientsProvider = clientsProvider;
            ActiveClientId = new ReactiveProperty<string>();
            _disposable = new CompositeDisposable();
        }
        
        public void Initialize()
        {
            _clientsProvider.Clients
                .ObserveCountChanged()
                .Where(x=>x>= ConnectionConfig.MAX_CLIENTS)
                .First()
                .Subscribe(_=>ActivateRandomClient(_clientsProvider.Clients))
                .AddTo(_disposable);
        }

        private void ActivateRandomClient(ReactiveDictionary<int,ClientConnection> clients)
        {
            var randomIndex = Random.Range(0, clients.Count);
            ActiveClientId.Value = clients.ElementAt(randomIndex).Value.Preferences.id;
        }

        public void ChangeActiveClientId()
        {
            var pool = _clientsProvider.Clients;
            var count = Mathf.Min(ConnectionConfig.MAX_CLIENTS, pool.Count);
            
            var current = ActiveClientId.Value;

            int currentIdx = -1;
            for (int i = 0; i < count; i++)
            {
                if (pool.ElementAt(i).Value.Preferences.id == current)
                {
                    currentIdx = i;
                    break;
                }
            }

            int nextIdx = (currentIdx == -1) ? 0 : (currentIdx + 1) % count;

            if (pool.ElementAt(nextIdx).Value.Preferences.id == current && count > 1)
                nextIdx = (nextIdx + 1) % count;

            ActiveClientId.Value = pool.ElementAt(nextIdx).Value.Preferences.id;
            
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            ActiveClientId?.Dispose();
        }
    }
}