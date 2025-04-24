namespace PowerUtilities.RenderFeatures
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine.Rendering;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;
    using System.Reflection;
    using PowerUtilities.UIElements;
    using UnityEngine.SceneManagement;


#if UNITY_EDITOR
    [CanEditMultipleObjects]
    //[CustomEditor(typeof(SRPFeature),true)]
    public class SRPFeatureEditor : PowerEditor<SRPFeature>
    {
        public override bool NeedDrawDefaultUI() => true;

        public override void DrawInspectorUI(SRPFeature inst)
        {
            //inst.isBaseOptionsFoldout = EditorGUILayout.Foldout(inst.isBaseOptionsFoldout, "Base Pass Options", true);
            //if (inst.isBaseOptionsFoldout)
            //    DrawDefaultInspector();
        }
    }


    /// <summary>
    /// Graph, node
    /// </summary>
    public partial class SRPFeature
    {
        /// <summary>
        /// sfc graph's position
        /// </summary>
        [HideInInspector]
        public BaseNodeInfo nodeInfo = new BaseNodeInfo();

        ///// <summary>
        ///// editor 
        ///// </summary>
        //Editor editorInstance;
        //public Editor GetEditor()
        //{
        //    EditorTools.CreateEditor(this, ref editorInstance);
        //    return editorInstance;
        //}
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
            Tag = 0, Name
        }

        public enum CameraTypeCompareFunc
        {
            LessEquals, Equals
        }

        public const string SRP_FEATURE_MENU = "PowerUtilities/SrpRenderFeatures";
        public const string SRP_FEATURE_PASSES_MENU = SRP_FEATURE_MENU + "/Passes";
        const string PASS_OPTIONS_GROUP = "PassOptions";


        [EditorBorder(19,groupName = PASS_OPTIONS_GROUP)]
        //[Header("Pass Options")]
        [EditorGroup(PASS_OPTIONS_GROUP, true,tooltip ="Show SRPPass base options")]
        [Tooltip("Skip when false")]
        public bool enabled = true;
        bool lastEnabled = true;

        [EditorGroup(PASS_OPTIONS_GROUP)]
        [Tooltip("Interrupt others pass when this pass done")]
        public bool interrupt;

        [EditorHeader(PASS_OPTIONS_GROUP, "Pass Options / CameraType")]
        [EditorGroup("PassOptions")]
        [Tooltip("Which camera can run")]
        public CameraType cameraType = CameraType.Reflection;

        [EditorGroup(PASS_OPTIONS_GROUP)]
        [Tooltip("Camera compare function")]
        public CameraTypeCompareFunc cameraTypeCompareFunc;

        [EditorGroup(PASS_OPTIONS_GROUP)]
        [Tooltip("Only work in editor")]
        public bool isEditorOnly;

        [EditorGroup(PASS_OPTIONS_GROUP)]
        [Tooltip("prefab stage show all objects?")]
        public bool isShowAllInPrefabStage = true;

        [EditorHeader(PASS_OPTIONS_GROUP, "Pass Options / Filters")]
        [EditorGroup(PASS_OPTIONS_GROUP)]
        [Tooltip("Compared properties ")]
        public CameraCompareType gameCameraCompareType = CameraCompareType.Tag;

        [EditorGroup(PASS_OPTIONS_GROUP)]
        [Tooltip("Which camera can run this pass ? only work for Game Camera, empty for all camera")]
#if UNITY_EDITOR
        [StringListSearchable(type = typeof(TagManager), staticMemberName = nameof(TagManager.GetTags))]
#endif
        public string gameCameraTag = "MainCamera";

        [EditorHeader(PASS_OPTIONS_GROUP, "Pass Options / URP Event")]
        [EditorGroup(PASS_OPTIONS_GROUP)]
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingSkybox;

        [EditorGroup(PASS_OPTIONS_GROUP)]
        public int renderPassEventOffset = 0;

        /// <summary>
        /// output this pass's log,show in inspector
        /// </summary>
        //[Multiline]
        [EditorGroup(PASS_OPTIONS_GROUP)]
        [EditorHeader(PASS_OPTIONS_GROUP, "Pass Options / Pass Log")]
        public string log;

        /// <summary>
        /// is details foldout ? show in inspector
        /// </summary>
        [HideInInspector]
        public bool isFoldout;

        /// <summary>
        /// SRPFeature's options
        /// </summary>
        [HideInInspector]
        public bool isBaseOptionsFoldout;

        // pass instance cache
        ScriptableRenderPass passInstance;
        TooltipAttribute featureTooltipAttr;

        Scene lastScene;
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
                {
                    passInstance = GetPass();
                    // call OnEnable when create new instance
                    OnEnable();
                }
                return passInstance;
            }
        }

        public string Tooltip
        {
            get
            {
                if (featureTooltipAttr == null)
                    featureTooltipAttr = GetType().GetCustomAttribute<TooltipAttribute>();
                return featureTooltipAttr?.tooltip;
            }
        }

        public void UpdateActiveState()
        {
            var isDisabled = (lastEnabled && !enabled);
            var isEnabled = (!lastEnabled && enabled);
            if (!CompareTools.CompareAndSet(ref lastEnabled, ref enabled))
                return;

            if (isDisabled)
                OnDisable();
            if (isEnabled)
                OnEnable();
        }

        public void UpdateSceneState()
        {
            var scene = SceneManager.GetActiveScene();

            if (CompareTools.CompareAndSet(ref lastScene, ref scene))
            {
                if (passInstance is SRPPass srpPass)
                    srpPass.OnSceneChanged();
            }
        }
        /// <summary>
        /// SRPFeature called
        /// Unity called
        /// </summary>
        public virtual void OnEnable()
        {
            if (passInstance is SRPPass srpPass)
            {
                srpPass.OnEnable();
            }
        }
        /// <summary>
        /// SRPFeature called
        /// Unity called
        /// </summary>
        public virtual void OnDisable()
        {
            if (passInstance is SRPPass srpPass)
            {
                srpPass.OnDisable();
            }
        }
         
        /// <summary>
        /// Destory current instance, next time will get new istanced
        /// </summary>
        public virtual void OnDestroy()
        {
            if (passInstance is SRPPass srpPass)
            {
                srpPass.OnDestroy();
            }
            passInstance = null;
        }

        public bool IsCameraValid(Camera c)
            => cameraTypeCompareFunc == CameraTypeCompareFunc.Equals ? c.cameraType == cameraType : c.cameraType <= cameraType;

        public static IEnumerable<Type> GetSRPFeatureTypes()
        => ReflectionTools.GetTypesDerivedFrom<SRPFeature>();

    }

}
