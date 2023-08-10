using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
#if UNITY_EDITOR
    [CustomEditor(typeof(CullingGroupControl))]
    public class CullingGroupControlEditor : PowerEditor<CullingGroupControl>
    {
        Editor cullingInfoEditor;
        public override bool NeedDrawDefaultUI() => true;

        public override void DrawInspectorUI(CullingGroupControl inst)
        {
            if (!inst.cullingProfile)
            {
                var path = $"{AssetDatabaseTools.CreateGetSceneFolder()}/CullingProfile.asset";
                inst.cullingProfile = ScriptableObjectTools.CreateGetInstance<CullingGroupSO>(path);
            }

            if (cullingInfoEditor == null || cullingInfoEditor.target != inst.cullingProfile)
            {
                cullingInfoEditor = Editor.CreateEditor(inst.cullingProfile);
            }

            cullingInfoEditor.OnInspectorGUI();

            if (GUILayout.Button("Bake DrawChildrenInstancedGroup"))
                BakeDrawChildrenInstancedGroup(inst);
        }

        private void BakeDrawChildrenInstancedGroup(CullingGroupControl inst)
        {
            // clear all
            CullingGroupControl.SceneProfile.cullingInfos.Clear();

            // fill
            var drawInfos = Object.FindObjectsOfType<DrawChildrenInstanced>();
            drawInfos.ForEach(drawInfo =>
            {
                inst.cullingProfile.SetupCullingGroupSO(drawInfo.drawInfoSO.groupList);
            });
        }
    }

#endif

    /// <summary>
    /// CullingGroup 
    /// </summary>
    public class CullingGroupControl : MonoBehaviour
    {
        public CullingGroupSO cullingProfile;

        public Camera targetCam;
        CullingGroup group;

        [Header("Debug")]
        public bool isShowDebug;

        static CullingGroupControl instance;

        public static event Action<CullingGroupEvent> OnVisibleChanged;

        private void Awake()
        {
            if (!targetCam)
                targetCam  = Camera.main;

            InitCullingGroup();

            // keep first instance
            if (!instance)
                instance = this;
        }

        private void InitCullingGroup()
        {
            group = new CullingGroup();
            group.targetCamera = targetCam;
        }

        private void OnEnable()
        {
            if(group == null)
            {
                InitCullingGroup();
            }
            group.SetBoundingSpheres(cullingProfile.cullingInfos.Select(c => new BoundingSphere(c.pos,c.size)).ToArray());
            group.onStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if(group != null)
            {
                group.onStateChanged -= OnStateChanged;
                group.Dispose();
            }

            OnVisibleChanged = null;
        }

        private void OnDrawGizmosSelected()
        {
            GetProfle().cullingInfos.ForEach(info =>
            {
                Gizmos.DrawWireSphere(info.pos,info.size);
            });
        }

        private void OnStateChanged(CullingGroupEvent e)
        {
            if (isShowDebug)
            {
                Debug.Log($"{e.index},visible:{e.isVisible}");

            }

            SceneProfile.cullingInfos[e.index].isVisible = e.isVisible;

            if (OnVisibleChanged != null)
                OnVisibleChanged(e);
        }

        public static CullingGroupSO GetProfle(string scenePath = "")
        {
            if (string.IsNullOrEmpty(scenePath))
                scenePath = SceneManager.GetActiveScene().path;

#if UNITY_EDITOR
            var path = $"{AssetDatabaseTools.CreateGetSceneFolder()}/CullingProfile.asset";
            var profile = ScriptableObjectTools.CreateGetInstance<CullingGroupSO>(path);
            return profile;
#endif
            return instance.cullingProfile;
        }

        public static CullingGroupSO SceneProfile
        {
            get { return GetProfle(); }
        }


    }
}
