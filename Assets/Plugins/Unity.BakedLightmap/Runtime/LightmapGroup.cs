using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace UnityEngine.Lightmaps
{

    public class LightmapGroup : MonoBehaviour, IBakeLightmap
    {
        public string group;

        private static string activeGroup;

        public static string ActiveGroup
        {
            get => activeGroup;
            set
            {
                value = value ?? string.Empty;
                Active(value);
            }
        }

        private static void Active(string group)
        {
            activeGroup = group;
            foreach (var g in Resources.FindObjectsOfTypeAll<LightmapGroup>())
            {
                g.OnLightmapGroupChanged();
            }
        }

        public void OnLightmapGroupChanged()
        {
            if (group == activeGroup)
            {
                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }
            }
            else
            {
                if (gameObject.activeSelf)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        public void OnBeforeBakeLighting(string group)
        {

        }

        public void OnAfterBakeLighting(string group)
        {
        }


    }

}