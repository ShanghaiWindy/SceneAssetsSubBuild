using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace ShanghaiWindy {
    class SceneBuilder : EditorWindow {
        public static BuildTarget runtimePlatform = BuildTarget.StandaloneWindows;

        [MenuItem("Tools/ShanghaiWindy/Build/SceneBuilder")]
        static void Init() {
            Rect wr = new Rect(0, 0, 250, 500);
            SceneBuilder window = (SceneBuilder)EditorWindow.GetWindowWithRect(typeof(SceneBuilder), wr, false, "SceneBuilder");
            window.Show();
            if (PlayerPrefs.HasKey("TargetPlatform")) {
                runtimePlatform = (BuildTarget)PlayerPrefs.GetInt("TargetPlatform");
            }
        }
        void OnGUI() {
            EditorGUILayout.HelpBox("The tool will build all assets serialized into Assetbundles separately. ", MessageType.Info);

            EditorGUILayout.HelpBox("Select target platform you want to build assets! ", MessageType.Info);
            runtimePlatform = (BuildTarget)EditorGUILayout.EnumPopup("Target Platform", runtimePlatform);
            PlayerPrefs.SetInt("TargetPlatform", (int)runtimePlatform);

            if (GUILayout.Button("Build Sub-Assets")) {
                if (!EditorUtility.DisplayDialog("[Important]Comfirm", "Compile Asset on Target Platform:" + runtimePlatform.ToString(), "Yes", "No")) {
                    return;
                }
               
                DirectoryInfo folder = new DirectoryInfo("Assets/res/Cooked/.packages/");
                if (!folder.Exists) {
                    folder.Create();
                }

                AssetBundleManifest assetbundleMainifest = BuildPipeline.BuildAssetBundles("Assets/res/Cooked/.packages", BuildAssetBundleOptions.ChunkBasedCompression, runtimePlatform);

                Hashtable Info = new Hashtable();
                foreach (FileInfo file in folder.GetFiles("*.extramesh")) {
                    List<string> Details = new List<string>();
                    Details.Add(GetMD5HashFromFile(file.FullName));
                    Info.Add(file.Name, Details);
                }

                StreamWriter streamWriter = new StreamWriter(folder + "md5.checker");
                streamWriter.Write(MiniJSON.Json.Serialize(Info));
                streamWriter.Close();
                if (!EditorUtility.DisplayDialog("Copy", "Copy cooked assets to Streamassets Folder", "Yes", "No")) {
                    return;
                }
                DirectoryInfo dir = new DirectoryInfo("Assets/res/Cooked/.packages/");
                string path = "Assets/res/Cooked/.packages/";
                string Seach_Path = Application.streamingAssetsPath + "/TWRPackages";

                DirectoryInfo TargetDir = new DirectoryInfo(Application.streamingAssetsPath + "/TWRPackages");
                if (!TargetDir.Exists) {
                    TargetDir.Create();
                }

                FileInfo[] files = dir.GetFiles();
                for (int i = 0; i < files.Length; i++) {
                    string source = path + "\\" + files[i].Name;
                    string target = Seach_Path + "\\" + files[i].Name;
                    if (!File.Exists(target) || (GetMD5HashFromFile(target) != GetMD5HashFromFile(source))) {
                        File.Copy(source, target, true);
                    }
                }
            }
        }
        public static string GetMD5HashFromFile(string fileName) {
            try {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++) {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (System.Exception ex) {
                Debug.Log(ex.Source);
                return null;
            }
        }
    }
}
