using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

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

        static CullingGroupControl instance;

        private void Awake()
        {
            if (!targetCam)
                targetCam  = Camera.main;

            group = new CullingGroup();
            group.targetCamera = targetCam;
            instance = this;
        }

        private void OnEnable()
        {
            group.SetBoundingSpheres(cullingProfile.cullingInfos.Select(c => new BoundingSphere(c.pos,c.size)).ToArray());
            group.onStateChanged = OnChanged;
        }

        private void OnDisable()
        {
            group.onStateChanged =null;
            group.Dispose();
        }

        private void OnDrawGizmos()
        {
            GetProfle().cullingInfos.ForEach(info =>
            {
                Gizmos.DrawWireSphere(info.pos,info.size);
            });
        }

        private void OnChanged(CullingGroupEvent e)
        {
            Debug.Log($"{e.index},visible:{e.isVisible}");
        }

        public static CullingGroupSO GetProfle(string scenePath = "")
        {
            if (string.IsNullOrEmpty(scenePath))
                scenePath = SceneManager.GetActiveScene().path;

            if (Application.isPlaying)
            {
                return instance.cullingProfile;
            }
#if UNITY_EDITOR
            var path = $"{AssetDatabaseTools.CreateGetSceneFolder()}/CullingProfile.asset";
            var profile = ScriptableObjectTools.CreateGetInstance<CullingGroupSO>(path);
            return profile;
#endif
        }

        public static CullingGroupSO SceneProfile
        {
            get { return GetProfle(); }
        }

    }
}
