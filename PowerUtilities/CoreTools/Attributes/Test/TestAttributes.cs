#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    public class TestAttributes : MonoBehaviour
    {
        [Flags]
        public enum PropEnum {
            //[InspectorName("class/a")]
            a,
            //[InspectorName("class/b")]
            b,
            c};


        [EditorGroup("Group1",true)]
        public bool Group1On;

        [EditorGroup("Group1")]
        //[Searchable]
        [EnumFlags]
        public PropEnum propEnum;
        

        [EditorGroup("Group1")]
        [DisplayName("Ò»¸ö»¬¿é", "slider helps","Assets/Gizmos/img1.png")]
        [Range(0, 1)]
        public float propA;

        [EditorGroup("Group2",true)]
        public bool Group2On;

        [EditorGroup("Group2")]
        public float a;

        [EditorGroup("Group2")]
        [TexturePreview]
        public Texture tex;

        [LayerIndex]
        public int layerId;

        [HelpBox]
        public string helpStr = "help text";

        [SortingLayerIndex] 
        public int sortingLayerId;


        //[ApplicationExit]
        //public static void Test()
        //{
        //    Debug.Log("test attr");
        //}

    }
}
#endif