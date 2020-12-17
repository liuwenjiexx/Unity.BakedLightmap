using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Lightmaps;


public class TestSplitScene : MonoBehaviour
{
    public BakedLightmapAsset bakedLightmap;

    // Start is called before the first frame update
    void Start()
    {
        bakedLightmap.LoadLightmap(0);
        bakedLightmap.AddLightmap(1);



        var prefab = Resources.Load<GameObject>("Prefabs/Cube");
        var go = GameObject.Instantiate(prefab);
        go.transform.parent = GameObject.Find("static/CubeParent").transform;
        go.name = prefab.name;
        go.transform.localPosition = Vector3.zero;
        bakedLightmap.ApplyLightmapChildren(go.transform, 1);


        prefab = Resources.Load<GameObject>("Prefabs/Capsule");
        go = GameObject.Instantiate(prefab);
        go.transform.parent = GameObject.Find("static/CapsuleParent").transform;
        go.name = prefab.name;
        go.transform.localPosition = Vector3.zero;
        bakedLightmap.ApplyLightmapChildren(go.transform, 1);

        prefab = Resources.Load<GameObject>("Prefabs/Sphere");
        go = GameObject.Instantiate(prefab);
        go.transform.parent = GameObject.Find("static/SphereParent").transform;
        go.name = prefab.name;
        go.transform.localPosition = Vector3.zero;
        bakedLightmap.ApplyLightmapChildren(go.transform, 1);

    }

    // Update is called once per frame
    void Update()
    {

    }



}
