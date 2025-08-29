using UnityEngine;

namespace Core.UI.Components
{
    public abstract class UIProvider<T> : MonoBehaviour where T: IUIView
    {
        [field:SerializeField] public T UI { get; protected set; }
    }
}