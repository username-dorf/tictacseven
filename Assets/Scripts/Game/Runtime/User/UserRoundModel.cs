using System;
using System.Collections.Generic;
using System.Linq;
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
        public ReactiveProperty<string> ProfileAssetId { get; protected set; }
        public MaterialId MaterialId { get; protected set; }


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
            ProfileAssetId = new ReactiveProperty<string>(preferencesProvider.Current.ProfileAssetId.Value);
            MaterialId = preferencesProvider.Current.TileMaterialId.Value;
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

        public void SetOwner(int owner)
        {
            Owner = owner;
        }
        
        public void Drop()
        {
            RoundResults.Clear();
        }

        public void Dispose()
        {
            AwaitingTurn?.Dispose();
            RoundResults?.Dispose();
            ProfileAssetId?.Dispose();
            UserModel?.Dispose();
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
               
            }
        }
    }
    public class AIUserRoundModel : UserRoundModel, IAIUserRoundModel
    {
        public PolicyDifficulty Difficulty { get; }

        [Inject]
        public AIUserRoundModel(UserModel.Factory userModelFactory,ProfileSpriteSetsProvider profileSpriteSetsProvider)
        {
            UserModel = userModelFactory.Create();
            Owner = 1;
            Difficulty = PolicyDifficulty.Normal;
            ProfileAssetId = new ReactiveProperty<string>(profileSpriteSetsProvider.GetRandomSet());
            AwaitingTurn = new ReactiveProperty<bool>(false);
            MaterialId = Core.Data.MaterialId.Opponent;
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
    
    public static class UserRoundModelExtension 
    {
        public static void UpdateAllModels(this List<UserRoundModel> models, List<int> winnerOwners)
        {
            foreach (var model in models)
            {
                model.SetRoundResult(winnerOwners.Contains(model.Owner));
            }
        }
        public static void UpdateAllModels(this List<UserRoundModel> models, List<string> winnerIds)
        {
            foreach (var model in models)
            {
                model.SetRoundResult(winnerIds.Contains(model.UserModel.Id));
            }
        }
    
    }
}