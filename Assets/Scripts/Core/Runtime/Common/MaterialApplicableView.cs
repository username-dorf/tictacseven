using UnityEngine;

namespace Core.Common
{
    public class MaterialApplicableView : MonoBehaviour
    {
        [field: SerializeField] public Renderer Renderer { get; protected set; }

        public void ChangeMaterial(Material material)
        {
            Renderer.material = material;
        }
    }
}