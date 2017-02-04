using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;


namespace ShanghaiWindy.Client {
    public class SceneAssetPrefab :MonoBehaviour {
        public string assetBundleName, assetBundleVariant, assetName;
        [System.NonSerialized]
        public bool LoadingDone = false, inLoading = false;

        [System.Serializable]
        public class MeshParameter {
            public Vector4 LightingMapTilingOffset, realTimeTilingOffset;
            public int LightingMapIndex = -1, realTimeIndex = -1;
            public bool RendererPathinChild = false;
            public string RendererPath = "";
            public ReflectionProbeUsage reflectionusage;
        }
        public MeshParameter[] meshParameters;


        public bool HasParticleSystem = false;
        public bool UseStandardShader = false;

        public void LoadAsset() {
            StartCoroutine(SubBuild_AssetBundleManager.LoadAsset(
                (myReturnValue) => {
                    if (myReturnValue == null) {
                        LoadingDone = true;
                        inLoading = false;
                        return;
                    }
                    GameObject Instance = Instantiate(myReturnValue) as GameObject;
                    Instance.transform.SetParent(transform);
                    Instance.transform.localScale = new Vector3(1, 1, 1);
                    Instance.transform.localEulerAngles = Vector3.zero;
                    Instance.transform.localPosition = Vector3.zero;
                    Instance.transform.SetParent(transform.parent);

                    Instance.isStatic = gameObject.isStatic;
                    Instance.tag = gameObject.tag;
                    Instance.layer = gameObject.layer;


                    Shader HighQuality = Shader.Find("Standard");
                    if (HasParticleSystem) {
                        ParticleSystemRenderer[] particles = Instance.GetComponentsInChildren<ParticleSystemRenderer>();
                        foreach (ParticleSystemRenderer particle in particles) {
                            for (int i = 0; i < particle.sharedMaterials.Length; i++) {
                                particle.sharedMaterials[i].shader  = Shader.Find(particle.sharedMaterials[i].shader.name);
                            }
                        }
                    }
                    if (meshParameters.Length > 0) {
                        foreach (MeshParameter meshParameter in meshParameters) {
                            MeshRenderer TargetRenderer = null;
                            if (meshParameter.RendererPathinChild) {
                                if (Instance.transform.FindChild(meshParameter.RendererPath))
                                    TargetRenderer = Instance.transform.FindChild(meshParameter.RendererPath).GetComponent<MeshRenderer>();
                            }
                            else {
                                if (Instance.transform)
                                    TargetRenderer = Instance.transform.GetComponent<MeshRenderer>();
                            }

                            if (TargetRenderer) {
                                TargetRenderer.lightmapIndex = meshParameter.LightingMapIndex;
                                TargetRenderer.lightmapScaleOffset = meshParameter.LightingMapTilingOffset;
                                TargetRenderer.realtimeLightmapIndex = meshParameter.realTimeIndex;
                                TargetRenderer.realtimeLightmapScaleOffset = meshParameter.realTimeTilingOffset;
                                TargetRenderer.reflectionProbeUsage = meshParameter.reflectionusage;
                                for (int i = 0; i < TargetRenderer.sharedMaterials.Length; i++) {
                                    if (TargetRenderer.sharedMaterials[i].shader.name == HighQuality.name) {
                                        TargetRenderer.sharedMaterials[i].shader = HighQuality;
                                    }

                                    else {
                                        TargetRenderer.sharedMaterials[i].shader = Shader.Find(TargetRenderer.sharedMaterials[i].shader.name);
                                    }
                                }
                            }
                        }

                    }
                    LoadingDone = true;
                    inLoading = false;
                }
                , assetName, assetBundleName, assetBundleVariant));
        }
    }
}
