using GhostAndGourmet;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GhostAndGourmet.Editor
{
    [InitializeOnLoad]
    public static class GhostAndGourmetProjectSetup
    {
        private const string SceneFolder = "Assets/Scenes";
        private const string ScenePath = "Assets/Scenes/GhostAndGourmetPrototype.unity";

        static GhostAndGourmetProjectSetup()
        {
            EditorApplication.delayCall += EnsurePrototypeScene;
        }

        private static void EnsurePrototypeScene()
        {
            EditorApplication.delayCall -= EnsurePrototypeScene;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(SceneFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            GameObject prototypeObject = new GameObject("GhostAndGourmetPrototype");
            GhostAndGourmetPrototype prototype = prototypeObject.AddComponent<GhostAndGourmetPrototype>();
            prototype.SeedDefaultsIfEmpty();
            EditorUtility.SetDirty(prototype);
            EditorSceneManager.MarkSceneDirty(scene);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
