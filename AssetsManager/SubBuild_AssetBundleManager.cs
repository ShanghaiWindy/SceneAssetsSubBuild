using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace ShanghaiWindy.Client {
    public class SubBuild_AssetBundleManager : MonoBehaviour {
        public static List<string> AssetBundleInLoading = new List<string>();
        public static Dictionary<string, AssetBundle> LoadedAssets = new Dictionary<string, AssetBundle>();
        public static string Path() {
            string path = "";
            switch (Application.platform) {
                case RuntimePlatform.Android:
                path = "jar:file://" + Application.dataPath + "!/assets/TWRPackages/";
                break;
                case RuntimePlatform.WindowsPlayer:
                path = "file://" + Application.streamingAssetsPath + "/TWRPackages/";
                break;
                case RuntimePlatform.WSAPlayerARM:
                path = "file://" + Application.streamingAssetsPath + "/TWRPackages/";
                break;
                case RuntimePlatform.WSAPlayerX86:
                path = "file://" + Application.streamingAssetsPath + "/TWRPackages/";
                break;
                case RuntimePlatform.WSAPlayerX64:
                path = "file://" + Application.streamingAssetsPath + "/TWRPackages/";
                break;
                case RuntimePlatform.WindowsEditor:
                path = "file://" + Application.streamingAssetsPath + "/TWRPackages/";
                break;
                case RuntimePlatform.IPhonePlayer:
                path = "file://" + Application.streamingAssetsPath + "/TWRPackages/";
                break;
                case RuntimePlatform.OSXPlayer:
                    path = Application.dataPath + "/StreamingAssets/TWRPackages/";
                    break;
                case RuntimePlatform.OSXEditor:
                    path = "file://" + Application.streamingAssetsPath + "/TWRPackages/";
                break;

            }

            return path;
        }
        #region Load Assets
        public static IEnumerator LoadAsset(System.Action<GameObject> ReturnObject, string AssetName, string assetBundleName, string assetBundleVariant) {
            AssetBundleRequest assetBundleRequest;
            assetBundleName = assetBundleName.ToLower();
            assetBundleVariant = assetBundleVariant.ToLower();

            while (AssetBundleInLoading.Contains(AssetName)) {
                yield return new WaitForEndOfFrame();
            }

            if (LoadedAssets.ContainsKey(AssetName)) {
                if (LoadedAssets[AssetName] == null)
                    ReturnObject(null);
                else {
                    assetBundleRequest = LoadedAssets[AssetName].LoadAssetAsync(AssetName);
                    yield return assetBundleRequest;
                    ReturnObject((GameObject)assetBundleRequest.asset);
                }
                yield break;
            }
            else {
                AssetBundleInLoading.Add(AssetName);
                WWW www = new WWW(Path() + assetBundleName + "." + assetBundleVariant);
                yield return www;
                if (www.error != null) {
                    ReturnObject(null);
                    yield break;
                }

                LoadedAssets.Add(AssetName, www.assetBundle);

                assetBundleRequest = LoadedAssets[AssetName].LoadAssetAsync(AssetName);
                yield return assetBundleRequest;
                ReturnObject((GameObject)assetBundleRequest.asset);

                www.Dispose();
                AssetBundleInLoading.Remove(AssetName);

                yield break;
            }
        }
        #endregion

    }
}