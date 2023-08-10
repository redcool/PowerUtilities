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


        [EditorGroup("Group1", true)]
        public bool Group1On;

        [EditorGroup("Group1")]
        //[EnumFlags]
        [Searchable]
        public PropEnum propEnum;

        [EditorGroup("Group1")]
        [DisplayName("Ò»¸ö»¬¿é", "slider helps","Assets/Gizmos/img1.png")]
        [Range(0, 1)]
        public float propA;

        [TexturePreview]
        public Texture tex;
    }
}
#endif