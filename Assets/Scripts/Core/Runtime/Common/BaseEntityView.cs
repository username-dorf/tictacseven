using Core.UI.Components;
using UnityEngine;

namespace Core.Common
{
    public abstract class BaseEntityView : UIWorldBounceable, IMaterialApplicableView
    {
        [field: SerializeField] public Renderer Renderer { get; protected set; }
        
        public void ChangeMaterial(Material material)
        {
            Renderer.material = material;
        }
    }
}