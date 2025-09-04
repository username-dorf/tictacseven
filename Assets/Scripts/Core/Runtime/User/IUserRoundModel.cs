using System.Collections.Generic;
using Core.Data;
using UniRx;

namespace Core.User
{
    public interface IProfileAssetId
    {
        ReactiveProperty<string> ProfileAssetId { get; }
    }
    public interface IUserRoundModel : IProfileAssetId
    {
        UserModel UserModel { get;}
        int Owner { get;}
        ReactiveCollection<bool> RoundResults { get;}
        ReactiveProperty<bool> AwaitingTurn { get; }
    }

    public interface IAIUserRoundModel : IUserRoundModel
    {
        PolicyDifficulty Difficulty { get; }
        
    }
}