namespace PowerUtilities.RenderFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine.Rendering;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;
    using System.Reflection;

#if UNITY_EDITOR
    using UnityEditor;
    using PowerUtilities.UIElements;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SRPFeature), true)]
    public class SRPFeatureEditor : PowerEditor<SRPFeature>
    {
        public override bool NeedDrawDefaultUI() => true;
        public override void DrawInspectorUI(SRPFeature inst)
        {
            //DrawDefaultInspector();    
        }
    }

    public partial class SRPFeature
    {
        public BaseNodeInfo nodeInfo = new BaseNodeInfo();

    }
#endif


    /// <summary>
    /// SRPPass's parameters
    /// 
    /// </summary>
    public abstract partial class SRPFeature : ScriptableObject
    {

        public enum CameraCompareType
        {
            Tag=0,Name
        }

        public const string SRP_FEATURE_MENU = "PowerUtilities/SrpRenderFeatures";
        public const string SRP_FEATURE_PASSES_MENU = SRP_FEATURE_MENU + "/Passes";

        [EditorBorder(17)]
        [Header("Pass Options")]
        [Tooltip("Skip when false")]
        public bool enabled = true;

        [Tooltip("Interrupt others pass when this pass done")]
        public bool interrupt;

        [Tooltip("Only work in editor's scene camera")]
        public bool isSceneCameraOnly;

        [Header("Pass Options / Filters")]
        [Tooltip("Compared properties ")]
        public CameraCompareType gameCameraCompareType = CameraCompareType.Tag;

        [Tooltip("Which camera can run this pass ? only work for Game Camera")]
        public string gameCameraTag = "MainCamera";

        [Header("Pass Options / URP Event")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public int renderPassEventOffset = 0;

        /// <summary>
        /// output this pass's log,show in inspector
        /// </summary>
        [Header("Pass Options / Pass Log")]
        [Multiline]
        public string log;

        /// <summary>
        /// is details foldout ? show in inspector
        /// </summary>
        [HideInInspector]
        public bool isFoldout;

        
        // pass instance cache
        ScriptableRenderPass passInstance;
        TooltipAttribute featureTooltipAttr;
        /// <summary>
        /// get a new pass instance
        /// </summary>
        /// <returns></returns>
        public abstract ScriptableRenderPass GetPass();

        /// <summary>
        /// get cached pass instance
        /// </summary>
        public ScriptableRenderPass PassInstance
        {
            get
            {
                if (passInstance == null)
                    passInstance = GetPass();
                return passInstance;
            }
        }

        public string Tooltip
        {
            get { 
                if(featureTooltipAttr == null)
                    featureTooltipAttr = GetType().GetCustomAttribute<TooltipAttribute>();
                return featureTooltipAttr?.tooltip;
            }
        }
        
        public void DestroyPassInstance()
        {
            passInstance = null;
        }
    }

}
