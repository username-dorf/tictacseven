using System;
using System.Collections.Generic;
using Core.User;
using Zenject;

namespace Game.User
{
    public class RoundsSettings
    {
        public const int ROUNDS_LENGTH = 3;
    }
    
    public class UserRoundModel : IDisposable
    {
        public UserModel UserModel { get; protected set; }
        public int Owner { get; protected set; }
        public List<bool> RoundResults { get; protected set; }

        protected UserRoundModel()
        {
            RoundResults = new List<bool>(RoundsSettings.ROUNDS_LENGTH);
        }
        
        [Inject]
        public UserRoundModel(UserModel userModel)
        {
            UserModel = userModel;
            Owner = 2;
            RoundResults = new List<bool>(RoundsSettings.ROUNDS_LENGTH);
        }

        public void SetRoundResult(bool isWinner)
        {
            if(RoundResults.Count+1 >= RoundsSettings.ROUNDS_LENGTH)
                throw new ArgumentOutOfRangeException($"Round index must be less than {RoundsSettings.ROUNDS_LENGTH}");
            
            RoundResults.Add(isWinner);
        }

        public void Drop()
        {
            RoundResults.Clear();
        }

        public void Dispose()
        {
            
        }
        public class Factory : PlaceholderFactory<UserRoundModel>
        {
            
        }
        public class Provider
        {
            private Factory _factory;
            private UserRoundModel _model;

            public UserRoundModel Model
            {
                get
                {
                    _model ??= _factory.Create();
                    return _model;
                }
            }
            
            public Provider(UserRoundModel.Factory factory)
            {
                _factory = factory;
            }

            public void Drop()
            {
                _model = null;
            }
        }
    }
    public class AIUserRoundModel : UserRoundModel
    {
        public PolicyDifficulty Difficulty { get; }

        [Inject]
        public AIUserRoundModel(UserModel.Factory userModelFactory)
        {
            UserModel = userModelFactory.Create();
            Owner = 1;
            Difficulty = PolicyDifficulty.Normal;
        }

        public class Factory : PlaceholderFactory<AIUserRoundModel>
        {
            
        }

        public class Provider
        {
            private Factory _factory;
            private AIUserRoundModel _model;

            public AIUserRoundModel Model
            {
                get
                {
                    _model ??= _factory.Create();
                    return _model;
                }
            }
            
            public Provider(AIUserRoundModel.Factory factory)
            {
                _factory = factory;
            }

            public void Drop()
            {
                _model = null;
            }
        }
    }
}