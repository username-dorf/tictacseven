using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.AppDebug.Components
{
    public class UIDebugView : MonoBehaviour
    {
        private Canvas _canvas;
        [field: SerializeField] public Transform Container { get; private set; }

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            _canvas.worldCamera = Camera.main;
        }
    }
}