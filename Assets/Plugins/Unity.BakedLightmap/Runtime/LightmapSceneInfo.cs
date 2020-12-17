using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.Lightmaps
{
    /// <summary>
    /// 光照场景的信息
    /// </summary>
    [Serializable]
    public class LightmapSceneInfo : ISerializationCallbackReceiver
    {
        public string id;
        public string group;
        public string sceneName;
        public List<LightmapRendererInfo> renderers = new List<LightmapRendererInfo>();
        public Texture2D[] lightmaps = new Texture2D[0];
        public Texture2D[] lightmapsDir;
        public Texture2D[] shadowMasks;
        public LightmapsMode lightmapsMode;
        public SphericalHarmonics[] lightProbes;
        public bool hasRealtimeLights;
        public bool copy;
        public string includeRenderer;
        public string excludeRenderer;
        private Dictionary<string, LightmapRendererInfo> cachedRenderers;

        public LightmapRendererInfo FindRenderer(string path)
        {
            LightmapRendererInfo rendererInfo;
            if (cachedRenderers.TryGetValue(path, out rendererInfo))
                return rendererInfo;
            return null;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (cachedRenderers == null)
                cachedRenderers = new Dictionary<string, LightmapRendererInfo>();
            cachedRenderers.Clear();
            foreach (var item in renderers)
            {
                cachedRenderers[item.path] = item;
            }
        }


#if UNITY_EDITOR
        public SceneAsset sceneAsset;

        public SceneAsset GetSceneAsset(BakedLightmapAsset lightmapInfo)
        {
            //var scene= lightmapInfo.gameObject.scene;
            //if (string.IsNullOrEmpty(scene.path))
            //    throw new Exception("scene path empty");
            //var sceneAsset= AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
            return sceneAsset;
        }

#endif

    }

    [System.Serializable]
    public class SphericalHarmonics
    {
        public float[] coefficients = new float[27];
    }
}