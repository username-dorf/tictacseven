using UnityEngine;

namespace Core.Common
{
    public interface IMaterialApplicableView
    {
        Renderer Renderer { get;}

        void ChangeMaterial(Material material);
    }
}