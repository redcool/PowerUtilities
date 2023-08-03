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
            if (Application.isPlaying)
            {
                if (!drawInfoSO)
                {
                    drawInfoSO = CreateNewProfile(gameObject.name);
                }
            }

            if (OnStarted != null)
            {
                OnStarted(this);
            }
        }

        DrawChildrenInstancedSO CreateNewProfile(string name)
        {
            var drawInfoSO = ScriptableObject.CreateInstance<DrawChildrenInstancedSO>();
            drawInfoSO.name = gameObject.name;

            drawInfoSO.SetupChildren(gameObject);
            drawInfoSO.DestroyOrHiddenChildren(false);
            return drawInfoSO;
        }

        void Update()
        {
            Profiler.BeginSample("Draw Children Instanced");
            if (drawInfoSO)
            {
                drawInfoSO.Update();
                drawInfoSO.DrawGroupList();
            }
            Profiler.EndSample();
        }

        void OnDestroy()
        {
            OnStarted = null;
        }


        public bool CheckDeviceSupport()
        {
            return SystemInfo.supportsInstancing;
        }


    }
}