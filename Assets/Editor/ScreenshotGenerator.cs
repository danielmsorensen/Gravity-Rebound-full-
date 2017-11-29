using UnityEngine;
using UnityEditor;
using System.Collections;

public class ScreenshotGenerator : EditorWindow {

    string outputPath;

    [MenuItem("Tools/ScreenshotGenerator")]
    public static void ShowWindow() {
        GetWindow(typeof(ScreenshotGenerator));
    }

    void OnGUI() {
        GUILayout.Label("Screenshot Generator");

        GUILayout.Space(20f);

        EditorGUILayout.LabelField("Output path: " + outputPath);
        if (GUILayout.Button("Browse")) {
            outputPath = EditorUtility.SaveFilePanel("Select Screenshot Output Path", outputPath, "Screenshot.png", "png");
        }

        GUILayout.Space(20f);

        if(GUILayout.Button("Take Screenshot")) {
            if (!Application.isPlaying) EditorUtility.DisplayDialog("Unable to take Screenshot", "The game must be playing to take a Screenshot", "OK");
            else TakeScreenshot(outputPath);
        }
    }

    public void TakeScreenshot(string outputPath) {
        Application.CaptureScreenshot(outputPath);

        EditorUtility.DisplayDialog("Taken Screenshot Successfully", "Saved a Screenshot to " + outputPath, "OK");
    }
}
