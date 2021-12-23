#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class MaterialBatch {

    static Dictionary<Material, List<Renderer>> dict = new Dictionary<Material, List<Renderer>>();

    //[MenuItem(AnalysisUtils.ANALYSIS_UTILS+"/Material Batch")]
    static void Init()
    {
        var gos = Selection.gameObjects;
        foreach (var go in gos)
        {
            var r = go.GetComponent<Renderer>();
            if (!r)
                continue;

            var m = r.sharedMaterial;

            if (!dict.ContainsKey(m))
            {
                dict.Add(m, new List<Renderer>());
            }
            dict[m].Add(r);
        }


    }

    
}
#endif