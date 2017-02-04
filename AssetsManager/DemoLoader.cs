using UnityEngine;

namespace ShanghaiWindy.Client {
    class DemoLoader : MonoBehaviour {
        void Awake() {
            SceneLoader.OnLoadingNewScenePrefab += OnLoadingNewScenePrefab;
            SceneLoader.OnStartLoadingScenePrefabs += OnStartLoadingScenePrefabs;
        }
        void OnDestroy() {
            SceneLoader.OnLoadingNewScenePrefab -= OnLoadingNewScenePrefab;
            SceneLoader.OnStartLoadingScenePrefabs -= OnStartLoadingScenePrefabs;
        }

        void OnStartLoadingScenePrefabs(int AssetsCount) {
            Debug.Log("You have to load" + AssetsCount + "Assets to open the scene");
        }
        void OnLoadingNewScenePrefab(string currentLoadingAssetName,int currentLoadingAssetOrder) {
            Debug.Log("Loading:" + currentLoadingAssetName + "Order:" + currentLoadingAssetOrder);
        }

        void Start() {
            DontDestroyOnLoad(this.gameObject); //Coroutine need this gameobject to keep active! 
            StartCoroutine(SceneLoader.RequestScene("DemoSimple", (onLoaded) => {
                //On loading is finish,the following scripts will run.
                Debug.Log("Scene is Loaded!");
            }));
        }
    }
}
