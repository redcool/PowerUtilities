namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using UnityEngine.Profiling;



    /// <summary>
    /// 解决,gpu instancing对光照图的处理
    /// 
    /// 调用Graphics.DrawMeshInstanced来绘制物体.
    /// 烘焙+gpu instancing,使用此代码来绘制.
    /// 
    /// groups:[
    ///     mesh:[
    ///         transformsGroup:[
    ///             transforms:[]
    ///         ]
    ///     ]
    /// ]
    /// 
    /// </summary>
    [ExecuteInEditMode]
    public class DrawChildrenInstanced : MonoBehaviour
    {
        public DrawChildrenInstancedSO drawInfoSO;

        public static event Action<DrawChildrenInstanced> OnStarted;

        public void Start()
        {
            if (!CheckDeviceSupport()) //设备不支持instance 或者 等级为 0
            {
                enabled = false;
                return;
            }

            //not run in editor
            if(Application.isPlaying)
            {
                if (!drawInfoSO)
                {
                    CreateNewProfile(out drawInfoSO);
                }
            }

            if (OnStarted != null)
            {
                OnStarted(this);
            }
        }

        void CreateNewProfile(out DrawChildrenInstancedSO drawInfoSO)
        {
            drawInfoSO = ScriptableObject.CreateInstance<DrawChildrenInstancedSO>();
            drawInfoSO.name = gameObject.name;

            drawInfoSO.enableLightmap = LightmapSettings.lightmaps.Length != 0;

            drawInfoSO.SetupChildren(gameObject, GetLevelId());
            drawInfoSO.DestroyOrHiddenChildren(drawInfoSO.destroyGameObjectWhenCannotUse);
        }

        void Update()
        {
            Profiler.BeginSample("Draw Children Instanced");
            if (drawInfoSO)
            {
                drawInfoSO.UpdateGroupListMaterial(LightmapSettings.lightmaps.Length != 0);
                drawInfoSO.DrawGroupList();
            }
            Profiler.EndSample();
        }

        void OnDestroy()
        {
            OnStarted = null;
        }

        /// <summary>
        /// low => high = [0,1,2,3] 
        /// </summary>
        /// <returns></returns>
        public int GetLevelId()
        {
            var levelId = 2;
            return levelId;
        }

        public bool CheckDeviceSupport()
        {
            return SystemInfo.supportsInstancing;
        }
    }
}