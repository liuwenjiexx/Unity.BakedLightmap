using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Lightmaps;

public class TestSingleScene : MonoBehaviour
{

    public BakedLightmapAsset bakedLightmap;    
    public GameObject prefab;

    // Start is called before the first frame update
    void Start()
    {
        bakedLightmap.LoadLightmap(0);
    }

    // Update is called once per frame
    void Update()
    {

    }
    private int selectedSceneIndex;

    private void OnGUI()
    {
        if (GUILayout.SelectionGrid(selectedSceneIndex, new string[] { "Light", "Night" }, 10, GUILayout.ExpandWidth(true)).IfChanged(ref selectedSceneIndex))
        {
            bakedLightmap.LoadLightmap(selectedSceneIndex);
        }

        if (GUILayout.Button("Instantiate"))
        {
            LoadCube();
        }

    }

    public void LoadCube()
    {
        var go = Instantiate(prefab);
        go.transform.position += new Vector3(Random.value * 5, 0, Random.value * 5);
        bakedLightmap.ApplyLightmapChildren( "static/" + prefab.name, go.transform, selectedSceneIndex);
        
    }

    [ContextMenu("GetRelativePath")]
    public void GetRelativePath()
    {
        Debug.Log( UnityEngine.Lightmaps.BakedLightmapAsset.GetRelativePath(transform,transform.root));
    }

}

public static class Extensions
{
    public static bool IfChanged<T>(this T newValue, ref T value)
    {
        if (object.Equals(value, newValue))
            return false;
        value = newValue;
        return true;
    }
}