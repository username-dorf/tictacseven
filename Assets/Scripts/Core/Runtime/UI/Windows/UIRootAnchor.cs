using UnityEngine;
using Zenject;

namespace Core.UI.Windows
{
    [DisallowMultipleComponent]
    public sealed class UIRootAnchor : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [Inject] private IUIRootService _svc;

        private void Awake()
        {
            if (!root) 
                root = (RectTransform)transform;
            _svc.SetRoot(root);
        }

        private void OnDestroy()
        {
            if(_svc is not null && root!=null)
                _svc.ClearRoot(root);
        }
    }
}