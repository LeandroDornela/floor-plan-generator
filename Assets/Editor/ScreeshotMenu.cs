using UnityEditor;
using UnityEngine;
using BuildingGenerator;

public class ScreenshotMenu : EditorWindow
{
    [MenuItem("Tools/Screenshot")]
    public static void ShowWindow()
    {
        GetWindow<ScreenshotMenu>("Screenshot");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Capture screen"))
        {
            Utils.Screenshot("");
        }
    }
}
