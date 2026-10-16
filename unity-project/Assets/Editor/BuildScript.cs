using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildScript
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    // Usage: Unity -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWebGL -logFile -
    public static void BuildWebGL()
    {
        Directory.CreateDirectory("Assets/Scenes");
        if (!File.Exists(ScenePath))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        PlayerSettings.productName = "Tetris";
        PlayerSettings.companyName = "UnityTetris";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = false;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        PlayerSettings.defaultWebScreenWidth = 960;
        PlayerSettings.defaultWebScreenHeight = 600;
        PlayerSettings.runInBackground = true;

        var opts = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Build/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };
        var report = BuildPipeline.BuildPlayer(opts);
        Debug.Log("BUILD RESULT: " + report.summary.result + " size=" + report.summary.totalSize);
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
