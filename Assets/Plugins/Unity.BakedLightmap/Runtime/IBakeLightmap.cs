using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.Lightmaps
{
    public interface IBakeLightmap
    {
        void OnBeforeBakeLighting(string group);
        void OnAfterBakeLighting(string group);
    }

}