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
                cullingInfoEditor = CreateEditor(inst.cullingProfile);
            }

            cullingInfoEditor.OnInspectorGUI();

            if (GUILayout.Button("Bake DrawChildrenInstancedGroup"))
            {
                inst.BakeDrawChildrenInstancedGroup();
                //inst.SetBoundingSpheres();
                //inst.InitSceneProfileVisibles();

                EditorUtility.SetDirty(inst.cullingProfile);
            }

            if(GUILayout.Button("set bounds"))
            {
                inst.SetBoundingSpheres();
            }

            if(GUILayout.Button("Init All Visible"))
            {
                inst.InitSceneProfileVisibles();
                EditorUtility.SetDirty(inst.cullingProfile);
            }
        }

    }

#endif

    /// <summary>
    /// CullingGroup 
    /// </summary>
    [ExecuteInEditMode]
    public class CullingGroupControl : MonoBehaviour
    {
        public CullingGroupSO cullingProfile;

        public Camera targetCam;
        CullingGroup group;

        public bool isInitAllVisiblesWhenStart;

        [Header("Debug")]
        public bool isShowDebug;
        public bool isShowGizmos;

        public static event Action<CullingGroupEvent> OnVisibleChanged;
        public static event Action OnInitSceneProfileVisibles;

        static CullingGroupControl instance;

        public static CullingGroupControl GetInstance() => SingletonTools.GetInstance(ref instance);

        private void Awake()
        {
            //destroy instances 
            if (instance)
            {
                Debug.Log("CullingGroupControl exists one.");
#if UNITY_EDITOR
                DestroyImmediate(this);
#else
                Destroy(this);
#endif
                return;
            }

            if (!targetCam)
                targetCam  = Camera.main;
            
            TryInitCullingGroup();
        }
        private void Start()
        {
            SetBoundingSpheres();

            if (isInitAllVisiblesWhenStart)
            {
                InitSceneProfileVisibles();
            }
        }

        public void TryInitCullingGroup()
        {
            if(group == null)
                group = new CullingGroup();

            group.targetCamera = targetCam;

            group.onStateChanged -= OnStateChanged;
            group.onStateChanged += OnStateChanged;
        }

        public void SetBoundingSpheres()
        {
            group.SetBoundingSpheres(SceneProfile.cullingInfos.Select(c => new BoundingSphere(c.pos, c.size)).ToArray());
        }
        public void TryDisposeCullingGroup()
        {
            if (group != null)
            {
                group.onStateChanged -= OnStateChanged;
                group.Dispose();
                group = null;
            }
        }

        private void OnEnable()
        {
            TryInitCullingGroup();

#if UNITY_EDITOR
            // spheres will lose when compiled in editor
            SetBoundingSpheres();
#endif
        }

        private void OnDisable()
        {
            TryDisposeCullingGroup();
        }

        private void OnDrawGizmos()
        {
            if (!isShowGizmos)
                return;

            cullingProfile.cullingInfos.ForEach(info =>
            {
                Gizmos.DrawWireSphere(info.pos,info.size);
            });
        }

        private void OnStateChanged(CullingGroupEvent e)
        {
            if (isShowDebug)
                Debug.Log($"{e.index},visible:{e.isVisible}");


            SceneProfile.cullingInfos[e.index].isVisible = e.isVisible;

            if (OnVisibleChanged != null)
                OnVisibleChanged(e);
        }

        public static CullingGroupSO GetProfle(string scenePath = "")
        {
            if (string.IsNullOrEmpty(scenePath))
                scenePath = SceneManager.GetActiveScene().path;


#if UNITY_EDITOR
            if (GetInstance().cullingProfile == null)
            {
                var path = $"{AssetDatabaseTools.CreateGetSceneFolder()}/CullingProfile.asset";
                GetInstance().cullingProfile = ScriptableObjectTools.CreateGetInstance<CullingGroupSO>(path);
            }
#endif
            return GetInstance().cullingProfile;
        }

        public static CullingGroupSO SceneProfile => GetProfle();

        public void BakeDrawChildrenInstancedGroup(bool isClearAll = true)
        {
            // clear all
            if (isClearAll)
                SceneProfile.cullingInfos.Clear();

            // fill
            var drawInfos = FindObjectsOfType<DrawChildrenInstanced>();
            drawInfos.ForEach((drawInfo ,infoId)=>
            {
                drawInfo.drawInfoSO.drawChildrenId = infoId;
                cullingProfile.SetupCullingGroupSO(infoId,drawInfo.drawInfoSO.groupList);
            });

        }

        public void InitSceneProfileVisibles()
        {
            SceneProfile.cullingInfos.ForEach((info, id) =>
            {
                info.isVisible = group.IsVisible(id);
            });

            if (OnInitSceneProfileVisibles != null)
                OnInitSceneProfileVisibles();
        }
    }
}
