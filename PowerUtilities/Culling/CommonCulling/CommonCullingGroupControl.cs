using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Events;

namespace PowerUtilities
{

#if UNITY_EDITOR

    [CustomEditor(typeof(CommonCullingGroupControl))]
    public class CommonCullingGroupControlEditor : PowerEditor<CommonCullingGroupControl>
    {

        public override bool NeedDrawDefaultUI() => true;

        public override void OnInspectorGUIChanged(CommonCullingGroupControl inst)
        {
            inst.DisposeGroup();
            inst.TryInitGroup();
        }

    }
#endif

    /// <summary>
    /// cullingGroup normal use
    /// </summary>
    [ExecuteInEditMode]
    public class CommonCullingGroupControl : MonoBehaviour
    {
        public enum ReactionType
        {
            None,GameObject,Renderer
        }

        [Header("--- Culling Info")]
        [Tooltip("Use MainCamera when empty")]
        public Camera cam;

        [Tooltip("Get Renderer from rootGo")]
        public GameObject rootGo;
        public List<CommomCullingInfo> cullingInfos = new List<CommomCullingInfo>();
        
        [EditorButton(onClickCall = "SetupCullingInfos", tooltip ="setup cullingInfo from rootGo children")]
        public bool isSetupCullingInfos;

        [Tooltip("distance lods")]
        public float[] boundingDistances = new float[] { 100,float.MaxValue};

        [EditorHeader("","--- Shared boundingSpheres")]
        [Tooltip("share other control's boundingSpheres")]
        public bool isUseOtherControlBoundingSpheres;
        public CommonCullingGroupControl otherControl;

        [Header("--- Color Gizmos")]
        public Color boundingSphereColor = Color.green;

        [Header("Reactions")]
        [Tooltip("visible state changed callback")]
        public UnityEvent<CullingGroupEvent> OnSphereStateChanged;

        [Tooltip("active type when visible changed")]
        public ReactionType reactionType;

        [HideInInspector]
        public BoundingSphere[] cullingSpheres;

        public CullingGroup group;

        /// <summary>
        /// culling state changed callbake
        /// </summary>
        public event Action<CommomCullingInfo> OnStateChanged;

        private void OnEnable()
        {
            if (!cam)
                cam = Camera.main;

            TryInitGroup();
        }
        private void OnDisable()
        {
            DisposeGroup();
        }

        public void Update()
        {
            UpdateTargetCam();
        }

        public void UpdateTargetCam()
        {
            if (group != null && cam)
            {
                group.targetCamera = cam;
                group.SetDistanceReferencePoint(group.targetCamera.transform.position);
            }
        }

        public void SetupCullingInfos()
        {
            if (!rootGo)
                return;

            var renders = rootGo.GetComponentsInChildren<Renderer>();
            SetupCullingInfos(renders,true,true,0);
        }
        public void ClearCullingInfos()
        {
            cullingInfos.Clear();
        }

        /// <summary>
        /// Setup cullingInfos from renders
        /// </summary>
        /// <param name="renders"></param>
        /// <param name="isClearLastInfos">Clear list,first group set true</param>
        /// <param name="isSetBoundingSpheres">set cullingGroup boundingSphere and set cullingInfo item's isVisible,last group set true</param>
        public void SetupCullingInfos(IList<Renderer> renders, bool isClearLastInfos, bool isSetBoundingSpheres,int batchGroupId)
        {
            if (isClearLastInfos)
                cullingInfos.Clear();

            for(int i = 0; i < renders.Count; i++)
            {
                var render = renders[i];
                var pos = render.bounds.center;
                var boundSize = render.bounds.extents.magnitude;
                var cullingInfo = new CommomCullingInfo(pos, boundSize);
                cullingInfo.contentGameObjects.Add(render.gameObject);
                // brg info
                cullingInfo.batchGroupId = batchGroupId;
                cullingInfo.visibleId = i;

                cullingInfos.Add(cullingInfo);
            }

            if (isSetBoundingSpheres)
            {
                SetBoundingSphere();
                SetupCullingInfosVisible();
            }
        }

        public bool IsUseSharedBoundingSpheres() => isUseOtherControlBoundingSpheres && otherControl != null;

        public void DisposeGroup()
        {
            if (group != null)
            {
                group.onStateChanged = null;
                group.Dispose();
                group = null;
            }
        }

        public void SetBoundingSphere()
        {
            if (IsUseSharedBoundingSpheres())
            {
                cullingSpheres = otherControl.cullingSpheres;
            }
            else
            {
                cullingSpheres = cullingInfos.Select(item => new BoundingSphere(item.pos, item.size)).ToArray();
            }
            group.SetBoundingSpheres(cullingSpheres);
            group.SetBoundingDistances(boundingDistances);
        }

        public void TryInitGroup()
        {
            if (group != null)
                return;
            group = new CullingGroup();

            group.onStateChanged = OnCullingChanged;
            group.targetCamera = cam ?? Camera.main;

            SetBoundingSphere();
            SetupCullingInfosVisible();
        }
        /// <summary>
        /// setup visible query cullingGroup
        /// </summary>
        /// <param name="startId"></param>
        public void SetupCullingInfosVisible(int startId = 0)
        {
            for (int i = startId; i < cullingInfos.Count; i++)
            {
                var info = cullingInfos[i];
                
                SetupCullingInfo(info,group.IsVisible(i),group.GetDistance(i));
            }
        }

        private void SetupCullingInfo(CommomCullingInfo info,bool isVisible,int distanceBands)
        {
            info.isVisible = isVisible;
            info.distanceBands = distanceBands;
            info.SetActive(reactionType);

            OnStateChanged?.Invoke(info);
        }

        /// <summary>
        /// cullingGroup callback setup visible 
        /// </summary>
        /// <param name="e"></param>
        void OnCullingChanged(CullingGroupEvent e)
        {
            if (!enabled)
                return;

            if (cullingInfos != null && e.index < cullingInfos.Count)
            {
                var info = cullingInfos[e.index];
                SetupCullingInfo(info, e.isVisible, e.currentDistance);
            }

            OnSphereStateChanged?.Invoke(e);
        }

        private void OnDrawGizmosSelected()
        {
            var lastColor = Gizmos.color;
            Gizmos.color = boundingSphereColor;
            foreach (var item in cullingInfos)
            {
                Gizmos.DrawWireSphere(item.pos, item.size);
            }
            Gizmos.color = lastColor;
        }
    }
}
