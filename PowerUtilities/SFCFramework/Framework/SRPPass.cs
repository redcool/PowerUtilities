using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// SRPPass's subClass's constructor for initial
    /// </summary>
    public abstract class SRPPass : ScriptableRenderPass
    {
        /// <summary>
        /// is pass run first, can do init once
        /// </summary>
        public bool isFirstPass;
        /// <summary>
        /// SRPFeature's name
        /// </summary>
        public string featureName;
        /// <summary>
        /// called when unity recompile
        /// </summary>
        public virtual void OnEnable() { }
        /// <summary>
        /// called when unity recompile
        /// </summary>
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }
        public virtual void OnSceneChanged() { }
    }

    /// <summary>
    /// urp renderPass controlled by SRPFeatureControl
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SRPPass<T> : SRPPass
        where T : SRPFeature
    {
        public T Feature { get; private set; }

        /// <summary>
        /// current camera, can access from OnExecute
        /// </summary>
        public Camera camera;
        /// <summary>
        /// current ScriptableRenderContext,can access from OnExecute
        /// </summary>
        protected ScriptableRenderContext context;

        /// <summary>
        /// Is SRPPass can execute,
        /// CanExecute will invoke OnCanExecute,
        /// </summary>
        public static Func<SRPPass<T>, bool> OnCanExecute;

        /// <summary>
        /// Call before srppass execute
        /// </summary>
        public static Action<SRPPass<T>> OnBeforeExecute;

        /// <summary>
        /// Call end srppass execute
        /// </summary>
        public static Action<SRPPass<T>> OnEndExecute;

        /// <summary>
        /// Call this when OnCameraCleanup
        /// </summary>
        public static Action<SRPPass<T>> OnCameraRenderFinish;

        /// <summary>
        /// Call when first pass run success in rendering loop
        /// </summary>
        public static Action<RenderingData> OnFirstPassRunCameraSetup;

        public SRPPass(T feature)
        {
            Feature = feature;
            featureName = feature.name;
        }

        /// <summary>
        /// Is pass need reset last render target
        /// </summary>
        /// <returns></returns>
        public virtual bool IsTryRestoreLastTargets(Camera c) => false;

        /// <summary>
        /// Compare with tag or name
        /// </summary>
        /// <param name="cam"></param>
        /// <returns></returns>
        public bool IsGameCameraValid(Camera cam)
        {
            switch (Feature.gameCameraCompareType)
            {
                case SRPFeature.CameraCompareType.Name:
                    return cam.name.IsMatch(Feature.gameCameraTag, StringEx.NameMatchMode.Full);
                default:
                    return cam.CompareTag(Feature.gameCameraTag);

            }
        }

        /// <summary>
        /// Reset render target to LastColorTargetIds
        /// </summary>
        /// <param name="cmd"></param>
        public void TryRestoreCameraTargets(CommandBuffer cmd)
        {
            // unity 2021, use nameId, dont use this
#if UNITY_2022_1_OR_NEWER
            if (RenderTargetHolder.IsLastTargetValid())
            {
                // (scene,game) will flicker
                //ConfigureTarget(RenderTargetHolder.LastColorTargetHandles, RenderTargetHolder.LastDepthTargetHandle);

                //smothtimes ,mrt cannot work
                cmd.SetRenderTarget(RenderTargetHolder.LastColorTargetIds, RenderTargetHolder.LastDepthTargetHandle.nameID);
            }
#endif
        }

        /// <summary>
        /// This pass can execute
        /// 1 check Feature 
        /// 2 check cameraType,
        ///     2.1 check gameCameraTag when cameraType is Game
        /// </summary>
        /// <returns></returns>
        public virtual bool CanExecute()
        {
            if (Feature == null || !Feature.enabled)
                return false;

            var isEditorValid = Feature.isEditorOnly ? Application.isEditor : true;
            //check event
            var isMonoPass = true;
            if (OnCanExecute != null)
                isMonoPass = OnCanExecute.Invoke(this);
            var isTimeValid = true;
            if(Feature.frameInterval>0)
                isTimeValid = Time.frameCount % Feature.frameInterval == 0;

            //check gameCamera
            var isGameCamPass = true;
            if (camera && camera.IsGameCamera() && !string.IsNullOrEmpty(Feature.gameCameraTag))
                isGameCamPass = IsGameCameraValid(camera);

            return isGameCamPass && isMonoPass && isEditorValid && isTimeValid;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            this.camera = cameraData.camera;

            if (isFirstPass)
                OnFirstPassRunCameraSetup?.Invoke(renderingData);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            this.camera = cameraData.camera;
            this.context = context;

            if (! CanExecute())
                return;

            var cmd = CommandBufferEx.defaultCmd;
            cmd.name = featureName;
            cmd.Execute(ref context);

            if(IsTryRestoreLastTargets(camera))
                TryRestoreCameraTargets(cmd);

            //cmd.BeginSampleExecute(featureName, ref context);

            // ========== trigger before execute
            OnBeforeExecute?.Invoke(this);

            OnExecute(context, ref renderingData,cmd);

            //cmd.EndSampleExecute(featureName, ref context);
            cmd.Execute(ref context);

            // ========== trigger end execute
            OnEndExecute?.Invoke(this);

        }

        private void CheckRTHandles(ref CameraData cameraData)
        {
            var rh = cameraData.renderer.CameraColorTargetHandle();
            Debug.Log (rh.rt);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            OnCameraRenderFinish?.Invoke(this);
        }

        public override void OnFinishCameraStackRendering(CommandBuffer cmd)
        {
            RenderTargetHolder.Clear();
        }

        public abstract void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd);

    }
}
