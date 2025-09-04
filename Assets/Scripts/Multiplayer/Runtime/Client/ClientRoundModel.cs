using Core.Data;
using Core.User;
using Game.User;
using Multiplayer.Connection;
using UniRx;

namespace Multiplayer.Client
{
    public class ClientRoundModel : UserRoundModel
    {
        public ClientRoundModel(UserPreferencesDto preferences, ConnectionType connectionType = ConnectionType.Host)
        {
            UserModel = new UserModel(preferences.id, preferences.nickname);
            Owner = connectionType == ConnectionType.Host ? 1 : 2;
            RoundResults = new ReactiveCollection<bool>();
            AwaitingTurn = new ReactiveProperty<bool>();
            ProfileAssetId = new ReactiveProperty<string>(preferences.profileAssetId);
            MaterialId = (MaterialId) preferences.tileMaterialId;
        }
    }
}