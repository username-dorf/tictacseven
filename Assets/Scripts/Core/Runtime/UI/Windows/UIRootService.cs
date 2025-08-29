using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Core.UI.Windows
{
    public interface IUIRootService
    {
        RectTransform Current { get; }
        void SetRoot(RectTransform root);
        void ClearRoot(RectTransform root = null);
        UniTask<RectTransform> WaitForRootAsync(CancellationToken ct);
    }

    public interface IUIBlurService
    {
        RectTransform Blur { get; }
        void SetBlur(RectTransform blur);
        void ClearBlur(RectTransform blur = null);
    }

    public sealed class UIRootService : IUIRootService, IUIBlurService
    {
        private readonly ReactiveProperty<RectTransform> _root = new(null);
        private readonly ReactiveProperty<RectTransform> _blur = new(null);
        
        public RectTransform Current => _root.Value;
        public RectTransform Blur => _blur.Value;

        public void SetBlur(RectTransform blur)
        {
            if (blur == null) 
                throw new System.ArgumentNullException(nameof(blur), "Blur RectTransform cannot be null.");
            _blur.Value = blur;
        }

        public void ClearBlur(RectTransform blur = null)
        {
            if (blur == null || _blur.Value == blur) 
                _blur.Value = null;
        }

        public void SetRoot(RectTransform root)
        {
            if (root == null) 
                throw new System.ArgumentNullException(nameof(root), "Root RectTransform cannot be null.");
            _root.Value = root;
        }

        public void ClearRoot(RectTransform root = null)
        {
            if (root == null || _root.Value == root) 
                _root.Value = null;
        }

        public async UniTask<RectTransform> WaitForRootAsync(CancellationToken ct)
        {
            if (_root.Value != null) return _root.Value;
            return await _root
                .Where(r => r != null)
                .First()
                .ToUniTask(cancellationToken: ct);
        }
    }
}