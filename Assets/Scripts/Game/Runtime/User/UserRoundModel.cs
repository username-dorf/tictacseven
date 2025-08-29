using System;
using System.Collections.Generic;
using Core.Data;
using Core.User;
using UniRx;
using Zenject;

namespace Game.User
{
    public class RoundsSettings
    {
        public const int ROUNDS_LENGTH = 3;
    }
    
    public class UserRoundModel : IUserRoundModel, IDisposable
    {
        public UserModel UserModel { get; protected set; }
        public int Owner { get; protected set; }
        public ReactiveProperty<bool> AwaitingTurn { get; protected set; }
        public ReactiveCollection<bool> RoundResults { get; protected set; }

        protected UserRoundModel()
        {
            RoundResults = new ReactiveCollection<bool>();
        }
        
        [Inject]
        public UserRoundModel(IUserPreferencesProvider preferencesProvider)
        {
            UserModel = preferencesProvider.Current.User;
            Owner = 2;
            RoundResults = new ReactiveCollection<bool>();
            AwaitingTurn = new ReactiveProperty<bool>(false);
        }

        public void SetRoundResult(bool isWinner)
        {
            if(RoundResults.Count>= RoundsSettings.ROUNDS_LENGTH)
                throw new ArgumentOutOfRangeException($"Round index must be less than {RoundsSettings.ROUNDS_LENGTH}");
            
            RoundResults.Add(isWinner);
        }
        public void SetAwaitingTurn(bool awaiting)
        {
            AwaitingTurn?.SetValueAndForceNotify(awaiting);
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
        public class Provider : IDisposable
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

            public void Dispose()
            {
                _model?.Dispose();
            }
        }
    }
    public class AIUserRoundModel : UserRoundModel, IAIUserRoundModel
    {
        public PolicyDifficulty Difficulty { get; }
        public string ProfileAssetId { get; }

        [Inject]
        public AIUserRoundModel(UserModel.Factory userModelFactory,ProfileSpriteSetsProvider profileSpriteSetsProvider)
        {
            UserModel = userModelFactory.Create();
            Owner = 1;
            Difficulty = PolicyDifficulty.Normal;
            ProfileAssetId = profileSpriteSetsProvider.GetRandomSet();
            AwaitingTurn = new ReactiveProperty<bool>(false);

        }

        public class Factory : PlaceholderFactory<AIUserRoundModel>
        {
            
        }

        public class Provider: IDisposable
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

            public void Dispose()
            {
                _model?.Dispose();
            }
        }
    }
}