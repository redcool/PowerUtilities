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
        bool isCullingInfoFolded;
        Editor cullingInfoEditor;
        public override bool NeedDrawDefaultUI() => true;

        public override void DrawInspectorUI(CullingGroupControl inst)
        {
            if (!inst.cullingProfile)
            {
                var path = $"{AssetDatabaseTools.CreateGetSceneFolder()}/CullingProfile.asset";
                inst.cullingProfile = ScriptableObjectTools.CreateGetInstance<CullingGroupSO>(path);
            }

            if(isCullingInfoFolded = EditorGUILayout.Foldout(isCullingInfoFolded,"culling infos", true))
            {
                if(cullingInfoEditor ==null || cullingInfoEditor.target != inst.cullingProfile)
                {
                    cullingInfoEditor = Editor.CreateEditor(inst.cullingProfile);
                }

                cullingInfoEditor.OnInspectorGUI();
            }
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

        private void Awake()
        {
            if (!targetCam)
                targetCam  = Camera.main;

            group = new CullingGroup();
            group.targetCamera = targetCam;
        }

        private void OnEnable()
        {
            group.SetBoundingSpheres(cullingProfile.cullingInfos.Select(c => c.boundingSphere).ToArray());
            group.onStateChanged = OnChanged;
        }

        private void OnDisable()
        {
            group.onStateChanged =null;
            group.Dispose();
        }

        private void OnChanged(CullingGroupEvent e)
        {
            Debug.Log($"{e.index},visible:{e.isVisible}");
        }

    }
}
