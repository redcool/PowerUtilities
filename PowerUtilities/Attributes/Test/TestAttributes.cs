#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    public class TestAttributes : MonoBehaviour
    {
        [DisplayName("Ò»¸ö»¬¿é","slider helps")]
        [Range(0,1)]
        public float propA;


        [Flags]
        public enum PropEnum { a,b,c};
        [EnumFlags]
        public PropEnum propEnum;

        //[EditorGroup("group1",true)]
        //public bool group1;

        //[EditorGroup("group1")]
        //public int num2;
    }
}
#endif