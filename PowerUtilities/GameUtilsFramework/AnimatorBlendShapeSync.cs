using GameUtilsFramework;
using PowerUtilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AnimatorBlendShapeSync : MonoBehaviour
{
    [Serializable]
    public class CurveInfo
    {
        [Tooltip("Animation clip curve name, animator parameter name")]
        public string curveName;
        [Tooltip("Animator parameter value,[0-1]")]
        public float curRate;

        [Tooltip("drived blend shapes")]
        public List<string> blendShapeNameList = new List<string>();

        public float Weight => curRate * 100;
    }
    
    public Animator anim;
    public SkinnedMeshRenderer skinned;

    //[ListItemDraw("name:,name,value:,value,blendShapeName:,blendShapeName", "60,120,60,120,80,")]
    public List<CurveInfo> curveInfos = new();
    
    void Start()
    {
         if(! anim)
            anim = GetComponent<Animator>();
         if(!skinned)
            skinned = GetComponentInChildren<SkinnedMeshRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        foreach (var info in curveInfos)
        {
            info.curRate = anim.GetFloat(info.curveName);

            foreach(var name in info.blendShapeNameList)
            {
                skinned.SetBlendShapeWeight(name, info.Weight);

            }
        }
        
    }
}
