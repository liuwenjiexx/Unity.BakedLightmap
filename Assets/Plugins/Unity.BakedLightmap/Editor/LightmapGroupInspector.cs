using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Lightmaps;

namespace UnityEditor.Lightmaps
{
    [CustomEditor(typeof(LightmapGroup))]
    public class LightmapGroupInspector : Editor
    {

        public LightmapGroup Asset
        {
            get => target as LightmapGroup;
        }

        public override void OnInspectorGUI()
        {

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Current Group", LightmapGroup.ActiveGroup ?? "");
                GUILayout.FlexibleSpace();
                if (Asset.group == LightmapGroup.ActiveGroup)
                {
                    GUI.color = Color.yellow;
                }
                if (GUILayout.Button("Active"))
                {
                    LightmapGroup.ActiveGroup= Asset.group;
                }
                GUI.color = Color.white;
                GUILayout.FlexibleSpace();
            }

            base.OnInspectorGUI();

        }
    }
}