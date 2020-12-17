using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.Lightmaps;
using System.Text.RegularExpressions;
using UnityEditor.GUIExtensions;

namespace UnityEditor.Lightmaps
{

    [CustomEditor(typeof(BakedLightmapAsset))]
    class BakedLightmapAssetInspector : Editor
    {
        private IEnumerator runner;
        SceneAsset currentSceneAsset;

        BakedLightmapAsset Asset
        {
            get => target as BakedLightmapAsset;
        }



        private void OnEnable()
        {

        }

        void StartCoroutine(IEnumerator routine)
        {
            EditorApplication.CallbackFunction next = null;
            next = () =>
            {
                if (routine.MoveNext())
                {
                    EditorApplication.delayCall += next;
                }
            };

            EditorApplication.delayCall += next;
        }


        public override void OnInspectorGUI()
        {
            BakedLightmapAsset asset = Asset;

            var lightmapScenes = asset.lightmapScenes;
            if (lightmapScenes == null)
            {
                lightmapScenes = new List<LightmapSceneInfo>();
                asset.lightmapScenes = lightmapScenes;
            }

            currentSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorSceneManager.GetActiveScene().path);


            using (var checker = new EditorGUI.ChangeCheckScope())
            {

                //using (new GUILayout.HorizontalScope())
                //{
                //    EditorGUILayout.PrefixLabel("Base Scene");
                //    Asset.baseSceneAsset = (SceneAsset)EditorGUILayout.ObjectField(Asset.baseSceneAsset, typeof(SceneAsset), false);
                //}
                //    EditorGUILayout.PropertyField(lightingSceneAssetsProperty);

                for (int i = 0; i < lightmapScenes.Count; i++)
                {
                    LightmapSceneInfo lightmapScene;
                    lightmapScene = asset.lightmapScenes[i];
                    GUILightmapScene(lightmapScene, i);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add Group"))
                    {
                        var ls = new LightmapSceneInfo();
                        ls.id = Guid.NewGuid().ToString("N");
                        ls.lightmaps = new Texture2D[0];
                        ls.renderers = new List<LightmapRendererInfo>();

                        lightmapScenes.Add(ls);
                    }
                    GUILayout.FlexibleSpace();
                }

                if (checker.changed)
                {
                    EditorUtility.SetDirty(asset);

                }
            }

            //GUILayout.Label("Current Lightmap");
            //if (LightmapSettings.lightmaps.Length > 0)
            //{
            //    var lightmap = LightmapSettings.lightmaps[0];
            //    //GUILayout.Label("lightmapColor");
            //    ////GUILayout.Box(lightmap.lightmapColor,GUILayout.MaxWidth(128),GUILayout.MaxHeight(128));
            //    //GUILayout.Label("shadowMask");
            //    //GUILayout.Box(lightmap.shadowMask, GUILayout.MaxWidth(128), GUILayout.MaxHeight(128));
            //    EditorGUILayout.ObjectField("lightmapColor", lightmap.lightmapColor, typeof(Texture), false);
            //    EditorGUILayout.ObjectField("lightmapDir", lightmap.lightmapDir, typeof(Texture), false);
            //    EditorGUILayout.ObjectField("shadowMask", lightmap.shadowMask, typeof(Texture), false);
            //}
            return;
            var renderers = new List<LightmapRendererInfo>();
            if (GUILayout.Button("Save All"))
            {
                string path = AssetDatabase.GetAssetPath(target);
                string lightmapDir = Path.GetDirectoryName(Path.GetDirectoryName(path));
                string scenePath = lightmapDir + ".unity";
                string outputPath = Path.Combine(lightmapDir, "LightmapSaved");
                scenePath = scenePath.Replace('\\', '/');

                if (!string.Equals(scenePath, SceneManager.GetActiveScene().path, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    Debug.LogError("open scene " + scenePath);
                    return;
                }

                foreach (var file in Directory.GetFiles(outputPath))
                {
                    string filename = Path.GetFileNameWithoutExtension(file);

                    if (filename.StartsWith("Lightmap", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (filename.EndsWith("_comp_dir"))
                        {
                            File.Delete(file);
                        }
                        if (filename.EndsWith("_comp_light"))
                        {
                            File.Delete(file);
                        }

                        if (filename.EndsWith("_comp_shadowmask"))
                        {
                            File.Delete(file);
                        }
                    }
                    if (filename.StartsWith("ReflectionProbe-", StringComparison.InvariantCultureIgnoreCase))
                    {
                        File.Delete(file);
                    }
                }

                renderers.Clear();
                HashSet<int> lightmapIndexs = new HashSet<int>();
                foreach (var renderer in GameObject.FindObjectsOfType<Renderer>())
                {
                    if (renderer.lightmapIndex == -1)
                    {
                        continue;
                    }

                    LightmapRendererInfo info = new LightmapRendererInfo();
                    info.renderer = renderer;
                    info.lightmapIndex = renderer.lightmapIndex;
                    info.lightmapScaleOffset = renderer.lightmapScaleOffset;

                    renderers.Add(info);
                    if (!lightmapIndexs.Contains(renderer.lightmapIndex))
                    {
                        lightmapIndexs.Add(renderer.lightmapIndex);
                    }
                }

                //foreach (var lightmapIndex in lightmapIndexs)
                //{
                //    CopyLightmap(scenePath, lightmapIndex);
                //}

                EditorUtility.SetDirty(asset);
                AssetDatabase.Refresh();
            }

            //foreach (var item in asset.renderers)
            //{
            //    if (GUILayout.Button(item.path, "label"))
            //    {
            //        var roots = SceneManager.GetActiveScene().GetRootGameObjects().Select(o => o.transform).ToArray();
            //        Transform t = item.Find(roots);
            //        if (t)
            //            EditorGUIUtility.PingObject(t);
            //    }
            //}
        }

        void GUILightmapScene(LightmapSceneInfo lightmapScene, int index)
        {
            var asset = Asset;
            var sceneAsset = lightmapScene.GetSceneAsset(asset);
            var lightmapScenes = asset.lightmapScenes;

            bool isActive = false;
            if (LightmapSettings.lightmaps != null && LightmapSettings.lightmaps.Length > 0 && lightmapScene.lightmaps.Length > 0)
            {
                var lightmap = LightmapSettings.lightmaps[0];
                if (lightmap.lightmapColor == lightmapScene.lightmaps[0])
                {
                    isActive = true;
                }
            }


            using (new EditorGUILayoutx.Scopes.FoldoutHeaderGroupScope(true, new GUIContent(string.IsNullOrEmpty(lightmapScene.group) ? "(default)" : lightmapScene.group), menuAction: (r) =>
            {
                GenericMenu menu = new GenericMenu();
                if (index > 0)
                {
                    menu.AddItem(new GUIContent("Move Up"), false, () =>
                    {
                        var tmp = lightmapScenes[index - 1];
                        lightmapScenes[index - 1] = lightmapScenes[index];
                        lightmapScenes[index] = tmp;
                        EditorUtility.SetDirty(Asset);
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Move Up"));
                }

                if (index < Asset.lightmapScenes.Count - 1)
                {
                    menu.AddItem(new GUIContent("Move Down"), false, () =>
                    {
                        var tmp = lightmapScenes[index + 1];
                        lightmapScenes[index + 1] = lightmapScenes[index];
                        lightmapScenes[index] = tmp;
                        EditorUtility.SetDirty(Asset);
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Move Down"));
                }

                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    lightmapScenes.RemoveAt(index);
                    GUI.changed = true;
                    EditorUtility.SetDirty(Asset);
                });


                menu.AddItem(new GUIContent("Save"), false, () =>
                {
                    SaveLightmapInfos(index);
                });
                menu.AddItem(new GUIContent("Bake Sync"), false, () =>
                {
                    StartCoroutine(BakeScene(index, false));
                });
                menu.ShowAsContext();
            }))
            {

            }

            using (new GUILayout.HorizontalScope())
            {
                string sceneName = null;
                if (sceneAsset)
                {
                    sceneName = sceneAsset.name;
                }
                else
                {
                    sceneName = "(null)";
                }


                using (new EditorGUI.DisabledGroupScope(currentSceneAsset != lightmapScene.GetSceneAsset(asset)))
                {
                    if (isActive)
                    {
                        GUI.color = Color.yellow;
                    }

                    if (GUILayout.Button("Active", GUILayout.ExpandWidth(true)))
                    {
                        asset.LoadLightmap(index);
                    }
                    GUI.color = Color.white;
                }

                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                {
                    if (GUILayout.Button("Bake", GUILayout.ExpandWidth(true)))
                    {
                        StartCoroutine(BakeScene(index, true));
                    }

                }

            }

            lightmapScene.sceneAsset = (SceneAsset)EditorGUILayout.ObjectField(lightmapScene.sceneAsset, typeof(SceneAsset), false);


            lightmapScene.group = EditorGUILayout.TextField("Group", lightmapScene.group);

            //lightmapScene.copy = EditorGUILayout.Toggle("Copy", lightmapScene.copy);


            int width;
            width = 60;
            for (int j = 0; j < lightmapScene.lightmaps.Length; j++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    //"lightmapColor",
                    EditorGUILayout.ObjectField(lightmapScene.lightmaps[j], typeof(Texture), false, GUILayout.Width(width), GUILayout.Height(width));
                    //"lightmapDir", 
                    EditorGUILayout.ObjectField(lightmapScene.lightmapsDir[j], typeof(Texture), false, GUILayout.Width(width), GUILayout.Height(width));

                    //"shadowMask", 
                    EditorGUILayout.ObjectField(lightmapScene.shadowMasks[j], typeof(Texture), false, GUILayout.Width(width), GUILayout.Height(width));
                }
            }




            GUIRendererInfoList(lightmapScene);
        }


        void GUIRendererInfoList(LightmapSceneInfo lightmapScene)
        {
            using (var renderersHeader = new EditorGUILayoutx.Scopes.FoldoutHeaderGroupScope(false, new GUIContent($"Renderer ({lightmapScene.renderers.Count})")))
            {
                if (renderersHeader.Visiable)
                {
                    int index = 0;

                    lightmapScene.includeRenderer = EditorGUILayout.TextField("Include", lightmapScene.includeRenderer);
                    lightmapScene.excludeRenderer = EditorGUILayout.TextField("Exclude", lightmapScene.excludeRenderer);

                    GUILayout.Label("List");
                    using (new EditorGUILayoutx.Scopes.IndentLevelVerticalScope())
                    {
                        GUIStyle lableStyle = "label";
                        foreach (var rendererInfo in lightmapScene.renderers.OrderBy(o => o.path))
                        {
                            if (GUILayout.Button(new GUIContent(rendererInfo.path, rendererInfo.path), lableStyle))
                            {
                                var go = GameObject.Find(rendererInfo.path);
                                if (go)
                                {
                                    EditorGUIUtility.PingObject(go);
                                }
                            }
                        }
                    }

                }
            }
        }

        class RendererInfoListState
        {
            public bool isShow;
        }

        IEnumerator BakeScene(int index, bool async)
        {
            var lightmapScene = Asset.lightmapScenes[index];
            var sceneAsset = lightmapScene.GetSceneAsset(Asset);
            var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            string baseScenePath = null;
            if (Asset.baseSceneAsset)
            {
                baseScenePath = AssetDatabase.GetAssetPath(Asset.baseSceneAsset);
                if (baseScenePath == scenePath)
                    baseScenePath = null;
            }
            return BakeScene(index, scenePath, baseScenePath, async);
        }

        IEnumerator BakeScene(int index, string scenePath, string baseScenePath, bool async)
        {
            //Debug.Log("scene: " + scenePath);
            //Debug.Log("base scene: " + baseScenePath);
            Debug.Log($"bake lightmap start");

            OpenSceneMode openMode = OpenSceneMode.Single;
            Scene lightingScene;
            LightmapsMode originLightmapMode = LightmapsMode.NonDirectional;
            if (openMode == OpenSceneMode.Additive)
            {
                originLightmapMode = LightmapSettings.lightmapsMode;
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                lightingScene = SceneManager.GetSceneByPath(scenePath);
                EditorSceneManager.SetActiveScene(lightingScene);
            }
            else
            {
                if (EditorSceneManager.GetActiveScene().path == scenePath)
                {
                    lightingScene = EditorSceneManager.GetActiveScene();
                }
                else
                {
                    lightingScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
            }


            if (!string.IsNullOrEmpty(baseScenePath))
            {
                var baseScene = EditorSceneManager.OpenScene(baseScenePath, OpenSceneMode.Additive);
            }
            yield return null;
            //LightmapSettings.lightmaps = null;
            Lightmapping.lightingDataAsset = null;

            SearchLightsNeededRealtime();
            var lightmapSceneInfo = Asset.lightmapScenes[index];
            LightmapGroup.ActiveGroup = lightmapSceneInfo.group;
            foreach (var o in Resources.FindObjectsOfTypeAll<LightmapGroup>())
            {
                o.OnBeforeBakeLighting(lightmapSceneInfo.group);
            }
            DateTime startTime = DateTime.Now;

            if (async)
            {
                Lightmapping.BakeAsync();
                while (Lightmapping.isRunning) { yield return null; }
            }
            else
            {
                Lightmapping.Bake();
            }
            //EditorSceneManager.SaveScene(EditorSceneManager.GetSceneByPath("Assets/Scenes/" + ScenarioName + ".unity"));

            foreach (var o in Resources.FindObjectsOfTypeAll<LightmapGroup>())
            {
                o.OnAfterBakeLighting(lightmapSceneInfo.group);
            }
            Debug.Log($"bake complete ({(DateTime.Now - startTime).TotalSeconds:0.#}s)");

            //AssetDatabase.Refresh();
            SaveLightmapInfos(index);


            //EditorSceneManager.SaveScene(lightingScene);



            if (openMode == OpenSceneMode.Additive)
            {
                EditorSceneManager.CloseScene(lightingScene, true);
                LightmapSettings.lightmapsMode = originLightmapMode;
            }
            yield break;
        }
        bool latestBuildHasReltimeLights;
        void SearchLightsNeededRealtime()
        {
            var lights = FindObjectsOfType<Light>();
            var reflectionProbes = FindObjectsOfType<ReflectionProbe>();
            latestBuildHasReltimeLights = false;

            foreach (Light light in lights)
            {
                if (light.lightmapBakeType == LightmapBakeType.Mixed || light.lightmapBakeType == LightmapBakeType.Realtime)
                    latestBuildHasReltimeLights = true;
            }
            if (reflectionProbes.Length > 0)
                latestBuildHasReltimeLights = true;
        }

        public void SaveLightmapInfos(int index)
        {
            Debug.Log("Storing data for lighting scenario " + index);
            if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
            {
                Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
                return;
            }
            var lightmapScene = Asset.lightmapScenes[index];
            var sceneAsset = lightmapScene.GetSceneAsset(Asset);
            if (!sceneAsset)
                return;
            var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            bool isSelf = false;
            Scene lightingScene;
            if (SceneManager.GetActiveScene().path == scenePath)
            {
                isSelf = true;
                lightingScene = SceneManager.GetActiveScene();
            }
            else
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                lightingScene = SceneManager.GetSceneByPath(scenePath);

                EditorSceneManager.SetActiveScene(lightingScene);
            }

            lightmapScene.renderers.Clear();
            var newRendererInfos = lightmapScene.renderers;
            var newLightmapsTextures = new List<Texture2D>();
            var newLightmapsTexturesDir = new List<Texture2D>();
            LightmapsMode newLightmapsMode = LightmapsMode.NonDirectional;
            var newSphericalHarmonicsList = new List<SphericalHarmonics>();
            var newLightmapsShadowMasks = new List<Texture2D>();

            newLightmapsMode = LightmapSettings.lightmapsMode;

            string baseDir = Path.Combine(Path.GetDirectoryName(scenePath), Path.GetFileNameWithoutExtension(scenePath));

            if (!string.IsNullOrEmpty(lightmapScene.group))
            {
                baseDir += "-" + lightmapScene.group;
            }

            Action<UnityEngine.Object> saveFile = (o) =>
            {
                if (!o)
                    return;
                string assetPath = AssetDatabase.GetAssetPath(o);
                string newPath = Path.Combine(baseDir, Path.GetFileName(assetPath));
                newPath = newPath.Replace('\\', '/');
                if (assetPath == newPath)
                    return;
                if (File.Exists(newPath))
                {
                    AssetDatabase.DeleteAsset(newPath);
                }

                if (!Directory.Exists(Path.GetDirectoryName(newPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                AssetDatabase.MoveAsset(assetPath, newPath);
                //Debug.Log(assetPath + " > " + newPath);
            };

            for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
            {
                var lightmap = LightmapSettings.lightmaps[i];

                newLightmapsTextures.Add(lightmap.lightmapColor);
                newLightmapsTexturesDir.Add(lightmap.lightmapDir);
                newLightmapsShadowMasks.Add(lightmap.shadowMask);

                if (lightmap.lightmapColor)
                {
                    saveFile(lightmap.lightmapColor);
                }
                if (lightmap.lightmapDir)
                {
                    saveFile(lightmap.lightmapDir);
                }
                if (lightmap.shadowMask)
                {
                    saveFile(lightmap.shadowMask);
                }
            }

            GenerateLightmapInfo(lightmapScene, lightingScene, newRendererInfos, newLightmapsTextures, newLightmapsTexturesDir, newLightmapsShadowMasks, newLightmapsMode);

            if (Asset.baseSceneAsset)
            {
                string baseScenePath = AssetDatabase.GetAssetPath(Asset.baseSceneAsset);
                Scene baseScene = SceneManager.GetSceneByPath(baseScenePath);
                GenerateLightmapInfo(lightmapScene, baseScene, newRendererInfos, newLightmapsTextures, newLightmapsTexturesDir, newLightmapsShadowMasks, newLightmapsMode);
            }

            lightmapScene.sceneName = SceneManager.GetActiveScene().name;
            lightmapScene.lightmapsMode = newLightmapsMode;

            lightmapScene.lightmaps = newLightmapsTextures.ToArray();

            if (newLightmapsMode != LightmapsMode.NonDirectional)
            {
                lightmapScene.lightmapsDir = newLightmapsTexturesDir.ToArray();
            }

            //Mixed or realtime support
            lightmapScene.hasRealtimeLights = latestBuildHasReltimeLights;

            lightmapScene.shadowMasks = newLightmapsShadowMasks.ToArray();



            var scene_LightProbes = new UnityEngine.Rendering.SphericalHarmonicsL2[LightmapSettings.lightProbes.bakedProbes.Length];
            scene_LightProbes = LightmapSettings.lightProbes.bakedProbes;

            for (int i = 0; i < scene_LightProbes.Length; i++)
            {
                var SHCoeff = new SphericalHarmonics();

                // j is coefficient
                for (int j = 0; j < 3; j++)
                {
                    //k is channel ( r g b )
                    for (int k = 0; k < 9; k++)
                    {
                        SHCoeff.coefficients[j * 9 + k] = scene_LightProbes[i][j, k];
                    }
                }

                newSphericalHarmonicsList.Add(SHCoeff);
            }

            lightmapScene.lightProbes = newSphericalHarmonicsList.ToArray();

            EditorUtility.SetDirty(Asset);
            EditorSceneManager.MarkSceneDirty(lightingScene);
            AssetDatabase.SaveAssets();
            //if (!isSelf)
            //{
            //    EditorSceneManager.CloseScene(lightingScene, true);
            //}
            Debug.Log("save bake lightmap");
            AssetDatabase.Refresh();
        }



        static void GenerateLightmapInfo(LightmapSceneInfo lightmapScene, Scene scene, List<LightmapRendererInfo> newRendererInfos, List<Texture2D> newLightmapsLight, List<Texture2D> newLightmapsDir, List<Texture2D> newLightmapsShadow, LightmapsMode newLightmapsMode)
        {
            var renderers = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<MeshRenderer>()).ToArray();
            Debug.Log("stored info for " + renderers.Length + " meshrenderers");

            Regex include = null, exclude = null;
            if (!string.IsNullOrEmpty(lightmapScene.includeRenderer))
                include = new Regex(lightmapScene.includeRenderer);
            if (!string.IsNullOrEmpty(lightmapScene.excludeRenderer))
                exclude = new Regex(lightmapScene.excludeRenderer);

            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer.lightmapIndex == -1)
                    continue;
                string path;
                path = GetTransformPath(renderer.transform);

                if ((include != null && !include.IsMatch(path)) || (exclude != null && exclude.IsMatch(path)))
                    continue;

                LightmapRendererInfo info = new LightmapRendererInfo();
                info.renderer = renderer;
                info.lightmapScaleOffset = renderer.lightmapScaleOffset;

                if (renderer.lightmapIndex == 65534)
                    continue;

                Texture2D lightmaplight = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                info.lightmapIndex = newLightmapsLight.IndexOf(lightmaplight);
                if (info.lightmapIndex == -1)
                {
                    info.lightmapIndex = newLightmapsLight.Count;
                    //AddLightmap(renderer.lightmapIndex);
                }

                if (newLightmapsMode != LightmapsMode.NonDirectional)
                {
                    Texture2D lightmapdir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                    info.lightmapIndex = newLightmapsDir.IndexOf(lightmapdir);
                    if (info.lightmapIndex == -1)
                    {
                        info.lightmapIndex = newLightmapsDir.Count;
                        //AddLightmap(renderer.lightmapIndex);
                    }
                }
                if (LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask != null)
                {
                    Texture2D lightmapShadow = LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask;
                    info.lightmapIndex = newLightmapsShadow.IndexOf(lightmapShadow);
                    if (info.lightmapIndex == -1)
                    {
                        info.lightmapIndex = newLightmapsShadow.Count;
                        //AddLightmap(renderer.lightmapIndex);
                    }
                }
                info.path = path;
                newRendererInfos.Add(info);
            }

        }

        static string GetTransformPath(Transform t)
        {
            string path = t.name;
            t = t.parent;
            while (t)
            {
                path = t.name + "/" + path;
                t = t.parent;
            }
            return path;
        }

        IEnumerable<string> EnumerateLightmapFileNames(int lightIndex)
        {
            yield return string.Format(BakedLightmapAsset.LightmapDirFileFormat, lightIndex);
            yield return string.Format(BakedLightmapAsset.LightmapLightFileFormat, lightIndex);
            yield return string.Format(BakedLightmapAsset.LightmapShadowmaskFileFormat, lightIndex);
            yield return string.Format(BakedLightmapAsset.ReflectionProbeFileFormat, lightIndex);
        }

        //void CopyLightmap(string scenePath, int lightIndex)
        //{
        //    string sceneName = Path.GetFileNameWithoutExtension(scenePath);
        //    string lightmapDir = Path.Combine(Path.GetDirectoryName(scenePath), sceneName);
        //    string output = Path.Combine(lightmapDir, "LightmapSaved");

        //    string path;
        //    foreach (var filename in EnumerateLightmapFileNames(lightIndex))
        //    {
        //        path = Path.Combine(lightmapDir, filename);
        //        if (!File.Exists(path))
        //        {
        //            //throw new Exception("not exists ligthmap file: " + path);
        //            continue;
        //        }
        //        string outputPath = Path.Combine(output, filename);
        //        File.Copy(path, outputPath);

        //        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(outputPath);
        //        AssetDatabase.ImportAsset(outputPath);
        //        var importer = (TextureImporter)TextureImporter.GetAtPath(outputPath);

        //        if (filename.StartsWith("ReflectionProbe"))
        //        {
        //            importer.textureShape = TextureImporterShape.TextureCube;
        //            //importer = false;
        //            //       TextureImporterCubemapConvolution
        //            //LevelLightmapData.
        //        }
        //        else if (filename.EndsWith("comp_light.exr", StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            importer.textureType = TextureImporterType.Lightmap;
        //        }
        //        importer.SaveAndReimport();
        //    }
        //}




        //[MenuItem("Assets/Create/Lightmap Saved")]
        //public static void Create()
        //{
        //    string scenePath = SceneManager.GetActiveScene().path;
        //    if (string.IsNullOrEmpty(scenePath))
        //    {
        //        Debug.LogError("scene path null");
        //        return;
        //    }
        //    string path = Path.Combine(Path.GetDirectoryName(scenePath), Path.GetFileNameWithoutExtension(scenePath), "LightmapSaved/LightmapSaved.asset");

        //    if (!File.Exists(path))
        //    {
        //        var asset = ScriptableObject.CreateInstance<LightmapDatabase>();
        //        AssetDatabase.CreateAsset(asset, path);
        //        AssetDatabase.Refresh();
        //    }
        //}
    }
}