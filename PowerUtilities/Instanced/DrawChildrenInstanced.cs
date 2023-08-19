namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using UnityEngine.Profiling;
    using System.Text;
    using UnityEngine.Rendering;

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

        private void AddEvents()
        {
            CullingGroupControl.OnVisibleChanged -= CullingGroupControl_OnVisibleChanged;
            CullingGroupControl.OnVisibleChanged += CullingGroupControl_OnVisibleChanged;

            CullingGroupControl.OnInitSceneProfileVisibles -= CullingGroupControl_OnInitAllVisibles;
            CullingGroupControl.OnInitSceneProfileVisibles += CullingGroupControl_OnInitAllVisibles;

        }
        private void RemoveEvents()
        {
            CullingGroupControl.OnVisibleChanged -= CullingGroupControl_OnVisibleChanged;
            CullingGroupControl.OnInitSceneProfileVisibles -= CullingGroupControl_OnInitAllVisibles;
        }

        public void Awake()
        {
            CheckSupports();

            //AddEvents,must first register, CullingGroupControl onEnable will call
            if (enabled)
                AddEvents();
        }

        private void OnEnable()
        {
            AddEvents();
        }
        private void OnDisable()
        {
            RemoveEvents();
        }

        private void CheckSupports()
        {
            if (!GraphicsDeviceTools.IsDeviceSupportInstancing()) //设备不支持instance 或者 等级为 0
            {
                enabled = false;

                // show original objects
                drawInfoSO.SetupRenderers(gameObject, true);
                drawInfoSO.SetRendersActive(true);
            }
        }

        public void Start()
        {
            //not run in editor
            if (Application.isPlaying)
            {
                if (!drawInfoSO)
                {
                    drawInfoSO = CreateNewProfile(gameObject.name);
                }
            }
        }

        private void CullingGroupControl_OnInitAllVisibles()
        {
            CullingGroupControl.SceneProfile.cullingInfos.ForEach((cullingInfo ,infoId)=>
            {
                if (drawInfoSO.drawChildrenId != cullingInfo.drawChildrenId)
                    return;

                drawInfoSO.groupList[cullingInfo.groupId]
                    .originalTransformsGroupList[cullingInfo.transformGroupId]
                    .transformVisibleList[cullingInfo.transformId] = cullingInfo.isVisible;
            });
            //Debug.Log(drawInfoSO.ToString());
        }

        private void CullingGroupControl_OnVisibleChanged(CullingGroupEvent e)
        {
            if (CullingGroupControl.SceneProfile.cullingInfos[e.index] is InstancedGroupCullingInfo cullingInfo)
            {
                //Debug.Log(e.index+":"+e.isVisible+":"+cullingInfo.ToString());

                if (drawInfoSO.drawChildrenId != cullingInfo.drawChildrenId)
                    return;

                drawInfoSO.groupList[cullingInfo.groupId]
                    .originalTransformsGroupList[cullingInfo.transformGroupId]
                    .transformVisibleList[cullingInfo.transformId] = cullingInfo.isVisible;
            }
            //Debug.Log(drawInfoSO.ToString());
        }

        DrawChildrenInstancedSO CreateNewProfile(string name)
        {
            var drawInfoSO = ScriptableObject.CreateInstance<DrawChildrenInstancedSO>();
            drawInfoSO.name = gameObject.name;

            drawInfoSO.SetupChildren(gameObject);
            drawInfoSO.DestroyOrHiddenChildren(false);
            return drawInfoSO;
        }

        void LateUpdate()
        {
            Profiler.BeginSample("Draw Children Instanced");
            if (drawInfoSO)
            {
                drawInfoSO.Update();
                drawInfoSO.DrawGroupList();
            }
            Profiler.EndSample();
        }

    }
}