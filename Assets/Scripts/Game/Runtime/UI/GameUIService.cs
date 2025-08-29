using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Game.UI
{
    public interface IGameUIService
    {
        RectTransform Container { get; }
        void SetContainer(RectTransform root);
        void ClearContainer(RectTransform root = null);
        UniTask<RectTransform> WaitForContainerAsync(CancellationToken ct);
    }
    
    public sealed class GameUIService : IGameUIService
    {
        private readonly ReactiveProperty<RectTransform> _contaiener = new(null);
        
        public RectTransform Container => _contaiener.Value;
        
        public void SetContainer(RectTransform root)
        {
            if (root == null) 
                throw new System.ArgumentNullException(nameof(root), "Container RectTransform cannot be null.");
            _contaiener.Value = root;
        }

        public void ClearContainer(RectTransform root = null)
        {
            if (root == null || _contaiener.Value == root) 
                _contaiener.Value = null;
        }

        public async UniTask<RectTransform> WaitForContainerAsync(CancellationToken ct)
        {
            if (_contaiener.Value != null) return _contaiener.Value;
            return await _contaiener
                .Where(r => r != null)
                .First()
                .ToUniTask(cancellationToken: ct);
        }
    }
}