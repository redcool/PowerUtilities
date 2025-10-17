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
        [Header("--- Culling Info")]
        [Tooltip("Use MainCamera when empty")]
        public Camera cam;

        [Tooltip("Get Renderer from rootGo")]
        public GameObject rootGo;
        public List<CommomCullingInfo> cullingInfos = new List<CommomCullingInfo>();
        
        [EditorButton(onClickCall = "FindRenderers",tooltip ="setup cullingInfo from rootGo children")]
        public bool isFindRenderers;

        public float[] boundingDistances = new float[] { 5f, 10f, 20f, 50f };

        [EditorHeader("","--- Shared boundingSpheres")]
        [Tooltip("share other control's boundingSpheres")]
        public bool isUseOtherControlBoundingSpheres;
        public CommonCullingGroupControl otherControl;

        [Header("--- Color Gizmos")]
        public Color boundingSphereColor = Color.green;

        [Header("Reactions")]
        public UnityEvent<CullingGroupEvent> OnSphereStateChanged;

        [HideInInspector]
        public BoundingSphere[] cullingSpheres;

        CullingGroup group;

        private void OnEnable()
        {
            if (!IsUseSharedBoundingSpheres())
                TryInitGroup();
        }
        private void OnDisable()
        {
            DisposeGroup();
        }

        /// <summary>
        /// use shared,need delay get boundingSphere
        /// </summary>
        private void Start()
        {
            if (IsUseSharedBoundingSpheres())
            {
                TryInitGroup();
            }
        }
        public void Update()
        {
            if (group != null && group.targetCamera)
                group.SetDistanceReferencePoint(group.targetCamera.transform.position);
        }

        public void FindRenderers()
        {
            if (!rootGo)
                return;

            var renders = rootGo.GetComponentsInChildren<Renderer>();
            cullingInfos.Clear();

            foreach (var render in renders)
            {
                var pos = render.bounds.center;
                var boundSize = render.bounds.extents.magnitude;
                var cullingInfo = new CommomCullingInfo(pos, boundSize);
                cullingInfo.contentGameObjects.Add(render.gameObject);

                cullingInfos.Add(cullingInfo);
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

        private void SetBoundingSphere()
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

        void SetupCullingInfosVisible()
        {
            for (int i = 0; i < cullingInfos.Count; i++)
            {
                cullingInfos[i].IsVisible = (group.IsVisible(i));
            }
        }

        void OnCullingChanged(CullingGroupEvent e)
        {
            if (cullingInfos != null && e.index < cullingInfos.Count)
            {
                var info = cullingInfos[e.index];
                info.IsVisible = (e.isVisible);
                info.distanceBands = e.currentDistance;
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
