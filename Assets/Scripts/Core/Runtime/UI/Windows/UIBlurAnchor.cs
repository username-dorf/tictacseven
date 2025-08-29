using UnityEngine;
using Zenject;

namespace Core.UI.Windows
{
    [DisallowMultipleComponent]
    public sealed class UIBlurAnchor : MonoBehaviour
    {
        [SerializeField] private RectTransform blur;
        [Inject] private IUIBlurService _svc;

        private void Awake()
        {
            if (!blur) 
                blur = (RectTransform)transform;
            _svc.SetBlur(blur);
        }

        private void OnDestroy()
        {
            if(_svc is not null && blur!=null)
                _svc.ClearBlur(blur);
        }
    }
}