using UnityEngine;

namespace Core.UI
{

    public static class UIExtensions
    {
        /// <summary>
        /// Sets the layer for the given GameObject and all its children recursively.
        /// </summary>
        /// <param name="targetGameObject">The root GameObject to start from.</param>
        /// <param name="layerName">The name of the layer to set.</param>
        public static void SetLayerRecursively(this GameObject targetGameObject, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName); // Get the integer ID of the layer

            if (layer == -1) // Check if the layer name is valid
            {
                Debug.LogWarning($"Layer '{layerName}' does not exist. Please check your Project Settings > Tags and Layers.");
                return;
            }

            // Set the layer for the current GameObject
            targetGameObject.layer = layer;

            // Recursively set the layer for all children
            foreach (Transform child in targetGameObject.transform)
            {
                SetLayerRecursively(child.gameObject, layerName);
            }
        }

        /// <summary>
        /// Sets the layer for the given GameObject and all its children recursively, using a layer index.
        /// </summary>
        /// <param name="targetGameObject">The root GameObject to start from.</param>
        /// <param name="layerIndex">The integer index of the layer to set.</param>
        public static void SetLayerRecursively(this GameObject targetGameObject, int layerIndex)
        {
            // Set the layer for the current GameObject
            targetGameObject.layer = layerIndex;

            // Recursively set the layer for all children
            foreach (Transform child in targetGameObject.transform)
            {
                SetLayerRecursively(child.gameObject, layerIndex);
            }
        }
    }

}