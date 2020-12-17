using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace UnityEngine.Lightmaps
{

    public delegate bool LightmapMatchRendererInfoDelegate(int lightmapSceneIndex, Renderer renderer, LightmapRendererInfo rendererInfo);

    /// <summary>
    /// 光照贴图资源
    /// </summary>
    [CreateAssetMenu]
    [ExecuteInEditMode]
    public class BakedLightmapAsset : ScriptableObject
    {

#if UNITY_EDITOR
        public SceneAsset baseSceneAsset;
        public bool isBakeAsync { get => true; }
#endif

        [SerializeField]
        public List<LightmapSceneInfo> lightmapScenes;
        public bool verbose = false;
        private Dictionary<int, int> lightmapOffsets = new Dictionary<int, int>();
        public LightmapMatchRendererInfoDelegate MatchRenderer;

        public List<UnityEngine.Rendering.SphericalHarmonicsL2[]> lightProbesRuntime = new List<UnityEngine.Rendering.SphericalHarmonicsL2[]>();


        public const string LightmapDirFormat = "Lightmap-{0}_comp_dir";
        public const string LightmapLightFormat = "Lightmap-{0}_comp_light";
        public const string LightmapShadowmaskFormat = "Lightmap-{0}_comp_shadowmask";
        public const string ReflectionProbeFormat = "ReflectionProbe-{0}";
        public static string LightmapDirFileFormat = LightmapDirFormat + ".png";
        public static string LightmapLightFileFormat = LightmapLightFormat + ".exr";
        public static string LightmapShadowmaskFileFormat = LightmapShadowmaskFormat + ".png";
        public static string ReflectionProbeFileFormat = ReflectionProbeFormat + ".exr";

        public void OnEnable()
        {
            if (lightmapScenes == null)
                lightmapScenes = new List<LightmapSceneInfo>();
        }

        public static string GetPath(Transform transform, Transform root = null)
        {
            Transform current = transform;
            string path = string.Empty;
            while (current)
            {
                if (string.IsNullOrEmpty(path))
                    path = current.name;
                else
                    path = current.name + "/" + path;
                current = current.parent;

                if (root == current)
                    break;
            }
            return path;
        }

        public void LoadLightmap(int lightmapSceneIndex)
        {
            lightmapOffsets.Clear();

            LightmapSceneInfo lightmapScene = lightmapScenes[lightmapSceneIndex];
            LightmapSettings.lightmapsMode = lightmapScene.lightmapsMode;
            //m_SwitchSceneCoroutine = StartCoroutine(SwitchSceneCoroutine(lightingScenesNames[previousLightingScenario], lightingScenesNames[currentLightingScenario]));
            LightmapSettings.lightmaps = new LightmapData[0];

            LightmapSettings.lightmaps = LoadLightmaps(lightmapSceneIndex);


            LightmapGroup.ActiveGroup = lightmapScene.group;
        }

        public void AddLightmap(int lightmapSceneIndex)
        {
            LightmapSettings.lightmaps = LoadLightmaps(lightmapSceneIndex);
        }


        public int GetLightmapOffset(int lightmapSceneIndex)
        {
            int lightmapOffset;
            if (!lightmapOffsets.TryGetValue(lightmapSceneIndex, out lightmapOffset))
            {

                //编辑器默认当前光照贴图
                if (!Application.isPlaying && lightmapSceneIndex == 0)
                    return 0;

                Debug.LogError($"not load lightmap <{lightmapSceneIndex}>");
                return -1;
            }
            return lightmapOffset;
        }

        LightmapData[] LoadLightmaps(int sceneIndex)
        {
            if (lightmapScenes[sceneIndex] == null || lightmapScenes[sceneIndex].lightmaps == null || lightmapScenes[sceneIndex].lightmaps.Length == 0)
            {
                Debug.LogWarning("No lightmaps stored in scenario " + sceneIndex);
                return null;
            }

            if (lightmapOffsets.ContainsKey(sceneIndex))
                throw new Exception($"aready load lightmap scene <{sceneIndex}>");

            var lightmapScene = lightmapScenes[sceneIndex];

            lightmapOffsets[sceneIndex] = LightmapSettings.lightmaps.Length;

            List<LightmapData> newLightmaps = new List<LightmapData>(LightmapSettings.lightmaps);

            for (int i = 0; i < lightmapScene.lightmaps.Length; i++)
            {
                var newLightmap = new LightmapData();
                newLightmap.lightmapColor = lightmapScene.lightmaps[i];
#if UNITY_EDITOR
                //Debug.Log("load light texture: " + AssetDatabase.GetAssetPath(lightmapScene.lightmaps[i]));
                //Debug.Log("load shadowMasks texture: " + AssetDatabase.GetAssetPath(lightmapScene.shadowMasks[i]));
#endif
                //  if (lightmapScene.lightmapsMode != LightmapsMode.NonDirectional)
                {
                    newLightmap.lightmapDir = lightmapScene.lightmapsDir[i];
                }
                //     if (lightmapScene.shadowMasks.Length > 0)
                {
                    newLightmap.shadowMask = lightmapScene.shadowMasks[i];
                }
                newLightmaps.Add(newLightmap);
            }

            return newLightmaps.ToArray();
        }

        public void ApplyAllRenderer(int lightmapSceneIndex)
        {
            //try
            //{
            LightmapSceneInfo lightmapScene = lightmapScenes[lightmapSceneIndex];
            int lightmapOffset = GetLightmapOffset(lightmapSceneIndex);

            List<LightmapRendererInfo> renderers = lightmapScene.renderers;

            for (int i = 0; i < renderers.Count; i++)
            {
                var rendererInfo = renderers[i];
                var renderer = rendererInfo.renderer;
                if (!renderer && !string.IsNullOrEmpty(rendererInfo.path))
                {
                    var go = GameObject.Find(rendererInfo.path);
                    renderer = go.GetComponent<Renderer>();
                }

                if (!renderer)
                    continue;
                _ApplyLightmap(renderer, lightmapOffset, rendererInfo);
            }
            //}
            //catch (Exception e)
            //{
            //    Debug.LogError("Error in ApplyRendererInfo:" + e.GetType().ToString());
            //}
        }



        private void _ApplyLightmap(Renderer renderer, int lightmapOffsetIndex, LightmapRendererInfo rendererInfo)
        {
            if (!renderer)
                return;
            renderer.lightmapIndex = lightmapOffsetIndex + rendererInfo.lightmapIndex;
            //   if (!rendererInfo.renderer.isPartOfStaticBatch)
            {
                renderer.lightmapScaleOffset = rendererInfo.lightmapScaleOffset;
            }
            if (renderer.isPartOfStaticBatch && verbose)
            {
                Debug.Log("Object " + rendererInfo.renderer.gameObject.name + " is part of static batch, skipping lightmap offset and scale.");
            }
            //不需要设置 static
            //renderer.gameObject.isStatic = true;
            //Debug.Log("load lightmap renderer " + GetRelativePath( renderer.transform)+"\n "+ rendererInfo.path);
        }

        public void ApplyLightmap(Renderer renderer, int lightmapSceneIndex, LightmapRendererInfo rendererInfo)
        {
            int lightmapOffset = GetLightmapOffset(lightmapSceneIndex);
            if (rendererInfo == null)
                return;

            _ApplyLightmap(renderer, lightmapOffset, rendererInfo);
        }

        public void ApplyLightmap(string path, Renderer renderer, int lightmapSceneIndex)
        {
            int lightmapOffset = GetLightmapOffset(lightmapSceneIndex);
            var lightmapScene = lightmapScenes[lightmapSceneIndex];
            LightmapRendererInfo rendererInfo = null;
            rendererInfo = lightmapScenes[lightmapSceneIndex].FindRenderer(path);

            if (rendererInfo == null)
                return;
            _ApplyLightmap(renderer, lightmapOffset, rendererInfo);
        }

        public void ApplyLightmapChildren(Transform root, int lightmapSceneIndex)
        {
            string rootPath;
            var r = root.root;
            if (r == root)
            {
                rootPath = root.name;
            }
            else
            {
                rootPath = r.name + "/" + GetRelativePath(root, root.root);
            }

            ApplyLightmapChildren(rootPath, root, lightmapSceneIndex);
        }

        public void ApplyLightmapChildren(string rootPath, Transform root, int lightmapSceneIndex)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>())
            {
                string path;

                if (renderer.transform == root)
                {
                    path = rootPath;
                }
                else
                {
                    path = GetRelativePath(renderer.transform, root.transform);
                    path = rootPath + "/" + path;
                }
                var rInfo = lightmapScenes[lightmapSceneIndex].FindRenderer(path);
                if (rInfo != null)
                {
                    ApplyLightmap(renderer, lightmapSceneIndex, rInfo);
                }
            }

        }
        public static string GetRelativePath(Transform transform, Transform relativeToParent = null)
        {
            var t = transform;
            string path = "";
            bool first = true;
            while (t && t != relativeToParent)
            {
                if (first)
                {
                    first = false;
                    path = t.name;
                }
                else
                {
                    path = t.name + "/" + path;
                }
                t = t.parent;
            }
            return path;
        }

    }
}