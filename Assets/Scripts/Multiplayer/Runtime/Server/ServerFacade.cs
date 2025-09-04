using UnityEngine;
using Zenject;

namespace Multiplayer.Server
{
    public class ServerFacade : MonoBehaviour
    {
        public GameObjectContext Context
        {
            get
            {
                return GetComponent<GameObjectContext>();
            }
        }
        public class Factory : PlaceholderFactory<ServerFacade>
        {
            
        }
    }
}