using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace ShanghaiWindy.Client {
    public class SceneLoader:MonoBehaviour {
        /// <summary>
        /// string Current Loading name 
        /// int current count of ScenePrefabs
        /// </summary>
        public static event System.Action<string, int> OnLoadingNewScenePrefab;

        public static event System.Action<int> OnStartLoadingScenePrefabs;


        #region Load Scene and assets. 
        public static IEnumerator RequestScene(string SceneName, System.Action<bool> SceneLoadFinish = null) {
            AudioListener.volume = 0;

            List<string> ReleaseAssets = new List<string>();
            foreach (string Key in SubBuild_AssetBundleManager.LoadedAssets.Keys) {
                ReleaseAssets.Add(Key);
            }
            foreach (string key in ReleaseAssets) {
                if (SubBuild_AssetBundleManager.LoadedAssets[key] != null) {
                    SubBuild_AssetBundleManager.LoadedAssets[key].Unload(true);
                }
                SubBuild_AssetBundleManager.LoadedAssets.Remove(key);
            }
            SubBuild_AssetBundleManager.LoadedAssets = new Dictionary<string, AssetBundle>();

            Resources.UnloadUnusedAssets();



            AsyncOperation LoadSceneOperation = SceneManager.LoadSceneAsync(SceneName + "_Cooked");

            yield return LoadSceneOperation;
            #region Preventing losing shader
            if (RenderSettings.skybox != null) {
                RenderSettings.skybox.shader = Shader.Find(RenderSettings.skybox.shader.name);
            }
            #endregion

            SceneAssetPrefab[] Sceneassets = GameObject.FindObjectsOfType<SceneAssetPrefab>();

            OnStartLoadingScenePrefabs(Sceneassets.Length);

            for (int i = 0; i < Sceneassets.Length; i++) {
                OnLoadingNewScenePrefab(Sceneassets[i].name, i);

                Sceneassets[i].inLoading = true;
                Sceneassets[i].LoadAsset();
                while (Sceneassets[i].inLoading) {
                    yield return new WaitForFixedUpdate();
                }
            }



            Resources.UnloadUnusedAssets();

            yield return new WaitForEndOfFrame();

            AudioListener.volume = 1;

            SceneLoadFinish(true);
        }
        #endregion
    }
}
