namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// key options for override DrawShadowSettingSO
    /// </summary>
    public class DrawShadowOverrideSettingSO : DrawShadowSettingSO
    {
        [Header("Override Options")]
        [EditorGroup("Res Options", true)]
        [Tooltip("Change _BigShadowMap size, will recreate rt")]
        public bool isOverrideShadowMapRes;


    }
}