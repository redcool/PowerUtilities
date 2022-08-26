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
        public enum PropEnum { a,b,c};


        [EditorGroup("Group1", true)]
        public bool Group1On;

        [EditorGroup("Group1")]
        [EnumFlags]
        public PropEnum propEnum;

        [EditorGroup("Group1")]
        [DisplayName("Ò»¸ö»¬¿é", "slider helps")]
        [Range(0, 1)]
        public float propA;
    }
}
#endif