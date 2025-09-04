using System.Collections.Generic;
using Core.User;
using Game.States;
using UnityEngine;
using Zenject;

namespace Game.User
{
    public interface IActiveUserProvider
    {
        int GetActiveUserId();
        void ChangeNextUser();
    }
    public class ActiveUserProvider: IActiveUserProvider
    {
        private int _activeUserIndex = 0;
        private int[] _ownerValues;

        public ActiveUserProvider(
            [Inject(Id = GameSubstatesFacade.ROUND_MODELS_ALIAS)] List<UserRoundModel> users,
            DiContainer diContainer)
        {
            _ownerValues = new int[users.Count];
            for (int i = 0; i < users.Count; i++)
            {
                _ownerValues[i] = users[i].Owner;
            }
            _activeUserIndex = Random.Range(0, _ownerValues.Length);
        }
        
        public int GetActiveUserId()
        {
            return _ownerValues[_activeUserIndex];
        }

        public void ChangeNextUser()
        {
            _activeUserIndex++;
            if (_activeUserIndex >= _ownerValues.Length)
            {
                _activeUserIndex = 0;
            }
        }
    }
}