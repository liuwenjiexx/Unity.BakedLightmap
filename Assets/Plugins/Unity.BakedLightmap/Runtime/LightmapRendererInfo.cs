using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.Lightmaps
{
    [Serializable]
    public class LightmapRendererInfo
    {
        public Renderer renderer;
        public string path;
        public int lightmapIndex;
        public Vector4 lightmapScaleOffset;


        //public Transform Find(Transform[] roots)
        //{
        //    string[] parts = path.Split(new char[] { '/' }, 2);
        //    string rootName = parts[0];

        //    foreach (var root in roots)
        //    {
        //        if (root.name == rootName)
        //        {
        //            if (parts.Length == 1)
        //                return root;
        //            return root.Find(parts[1]);
        //        }
        //    }
        //    return null;
        //}


    }
}