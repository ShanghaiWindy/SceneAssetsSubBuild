﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using ShanghaiWindy.Client;

namespace ShanghaiWindy {
    class SceneAssetsSerialize : EditorWindow {
        [MenuItem("Tools/ShanghaiWindy/Build/SceneAssetsSerialize")]
        static void Init() {
            Rect wr = new Rect(0, 0, 250, 500);
            SceneAssetsSerialize window = (SceneAssetsSerialize)EditorWindow.GetWindowWithRect(typeof(SceneAssetsSerialize), wr, false, "SceneAssetsSerialize");
            window.Show();
        }
        void OnGUI() {
            EditorGUILayout.HelpBox("The tool will serialize scene  generating a new cooked scene and label scene assets  automatically.Then, you can use SceneBuilder tool to build these assets!", MessageType.Info);

            if (GUILayout.Button("Serialize Scene")) {
                #region Save As New Scene
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/" + EditorSceneManager.GetActiveScene().name + "_Cooked.unity", true);
                EditorSceneManager.OpenScene("Assets/" + EditorSceneManager.GetActiveScene().name + "_Cooked.unity");
                #endregion
                GameObject[] SceneObjects = GameObject.FindObjectsOfType<GameObject>();
                List<GameObject> ScenePrefabsGameObjects = new List<GameObject>();

                foreach (GameObject sceneObject in SceneObjects) {
                    GameObject PrefabRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(sceneObject);

                    if (PrefabUtility.GetPrefabType(sceneObject) != PrefabType.None) {
                        if (PrefabRoot.tag == "NotAllowBuild")
                            continue;

                        if (ScenePrefabsGameObjects.Contains(PrefabRoot)) {
                            Debug.Log(PrefabRoot.name + "Built");
                            continue;
                        }

                        #region Genrerate new ReplaceObject with SceneAssetPrefab.cs
                        GameObject ReplaceObject = new GameObject(PrefabRoot.name + "_CookedResource");
                        ReplaceObject.transform.SetParent(PrefabRoot.transform);
                        ReplaceObject.transform.localScale = new Vector3(1, 1, 1);
                        ReplaceObject.transform.localPosition = Vector3.zero;
                        ReplaceObject.transform.localEulerAngles = Vector3.zero;
                        ReplaceObject.transform.SetParent(PrefabRoot.transform.parent);
                        ReplaceObject.isStatic = PrefabRoot.isStatic;
                        ReplaceObject.layer = PrefabRoot.layer;
                        ReplaceObject.transform.tag = PrefabRoot.transform.tag;

                        #region SetUp SceneAssetPrefab
                        SceneAssetPrefab sceneAssetPrefab = ReplaceObject.AddComponent<SceneAssetPrefab>();
                        List<SceneAssetPrefab.MeshParameter> MeshParameters = new List<SceneAssetPrefab.MeshParameter>();
                        #region Save LightMap data
                        if (PrefabRoot.GetComponentsInChildren<ParticleSystem>().Length > 0) {
                            sceneAssetPrefab.HasParticleSystem = true;
                        }

                        if (PrefabRoot.GetComponentsInChildren<MeshRenderer>().Length > 0) {
                            foreach (MeshRenderer meshRender in PrefabRoot.GetComponentsInChildren<MeshRenderer>()) {
                                SceneAssetPrefab.MeshParameter MeshParameter = new SceneAssetPrefab.MeshParameter();
                                if (meshRender.transform == PrefabRoot.transform) {
                                    MeshParameter.RendererPathinChild = false;
                                }
                                else {
                                    MeshParameter.RendererPathinChild = true;
                                    Transform Current = meshRender.transform;

                                    string ChildPath = "";
                                    List<string> Relation = new List<string>();
                                    while (Current != PrefabRoot.transform) {
                                        Relation.Add(Current.name);
                                        Current = Current.parent;
                                    }
                                    for (int i = Relation.Count - 1; i >= 0; i--) {
                                        ChildPath += Relation[i];
                                    }
                                    MeshParameter.RendererPath = ChildPath;
                                }
                                MeshParameter.LightingMapIndex = meshRender.lightmapIndex;
                                MeshParameter.LightingMapTilingOffset = meshRender.lightmapScaleOffset;
                                MeshParameter.realTimeIndex = meshRender.realtimeLightmapIndex;
                                MeshParameter.realTimeTilingOffset = meshRender.realtimeLightmapScaleOffset;
                                MeshParameter.reflectionusage = meshRender.reflectionProbeUsage;
                                MeshParameters.Add(MeshParameter);
                            }
                        }
                        sceneAssetPrefab.assetName = PrefabUtility.GetPrefabParent(PrefabRoot).name;
                        sceneAssetPrefab.meshParameters = MeshParameters.ToArray();
                        sceneAssetPrefab.assetBundleName = AssetNameCorretor(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(PrefabRoot))));
                        sceneAssetPrefab.assetBundleVariant = "sceneobject";
                        #endregion
                        #endregion



                        #endregion

                        if (!ScenePrefabsGameObjects.Contains(PrefabRoot)) {
                            ScenePrefabsGameObjects.Add(PrefabRoot);

                        }
                    }
                }
                List<Object> Reimported = new List<Object>();

                foreach (GameObject scenePrefab in ScenePrefabsGameObjects) {
                    if (scenePrefab != null && !Reimported.Contains(PrefabUtility.GetPrefabParent(scenePrefab))) {
                        Reimported.Add(PrefabUtility.GetPrefabParent(scenePrefab));

                        Debug.Log(scenePrefab.name + "Asset Reimported!");
                        AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(scenePrefab)));
                        string AssetPathToGUID = AssetNameCorretor(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(scenePrefab))));

                        assetImporter.assetBundleName = AssetPathToGUID;
                        assetImporter.assetBundleVariant = "sceneobject";


                    }
                    DestroyImmediate(scenePrefab);
                }
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
                string AllScenes = "\n Current Active Scenes:";
                bool AddFlag = true;

                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++) {
                    AllScenes +=  "\n -"+EditorBuildSettings.scenes[i].path;
                    if (EditorBuildSettings.scenes[i].path == EditorSceneManager.GetActiveScene().path) {
                        AddFlag = false;
                    }
                }
                if (AddFlag) {
                    if (EditorUtility.DisplayDialog("Add to BuildSetting", "Do you want to add this cooked scene to BuildSetting?"+ AllScenes, "Yes", "No,Thanks")) {
                        EditorBuildSettingsScene[] original = EditorBuildSettings.scenes;
                        EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[original.Length + 1];
                        System.Array.Copy(original, newSettings, original.Length);
                        newSettings[newSettings.Length - 1] = new EditorBuildSettingsScene(EditorSceneManager.GetActiveScene().path, true);
                        EditorBuildSettings.scenes = newSettings;
                    }
                }




                //EditorSceneManager.SaveOpenScenes ();

            }
        }
        public static string AssetNameCorretor(string str) {
            return str.Replace(" ", "").ToLower();

        }
    }
}
