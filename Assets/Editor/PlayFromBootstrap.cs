#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayFromBootstrap
{
    private const string BootScenePath = "Assets/Scenes/Bootstrap.unity";

    private const string MenuPath = "Tools/Play From Bootstrap";
    private const string PrefKeyEnabled = "PlayFromBootstrap.Enabled";

    static PlayFromBootstrap()
    {
        EditorApplication.delayCall += Init;
    }

    private static void Init()
    {
        bool enabled = EditorPrefs.GetBool(PrefKeyEnabled, true);
        Menu.SetChecked(MenuPath, enabled);
        Apply(enabled);
    }

    [MenuItem(MenuPath)]
    private static void Toggle()
    {
        bool enabled = !Menu.GetChecked(MenuPath);
        Menu.SetChecked(MenuPath, enabled);
        EditorPrefs.SetBool(PrefKeyEnabled, enabled);
        Apply(enabled);
    }

    private static void Apply(bool enabled)
    {
        if (!enabled)
        {
            EditorSceneManager.playModeStartScene = null;
            return;
        }

        var boot = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScenePath);
        if (boot == null)
        {
            EditorSceneManager.playModeStartScene = null;
            return;
        }

        EditorSceneManager.playModeStartScene = boot;
    }
}
#endif