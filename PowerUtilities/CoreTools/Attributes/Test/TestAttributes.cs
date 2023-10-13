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


        [EditorGroup("Group1")]
        [EditorGroupItem("Group1")]
        public bool Group1On;

        [EditorGroupItem("Group1")]
        //[EnumFlags]
        //[Searchable]
        public PropEnum propEnum;

        [EditorGroupItem("Group1")]
        [DisplayName("Ò»¸ö»¬¿é", "slider helps","Assets/Gizmos/img1.png")]
        [Range(0, 1)]
        public float propA;

        [EditorGroup("Group2")]
        [EditorGroupItem("Group2")]
        public bool Group2On;

        [EditorGroupItem("Group2")]
        public float a;

        [TexturePreview]
        public Texture tex;
    }
}
#endif