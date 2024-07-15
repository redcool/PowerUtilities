﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{

    public abstract class SRPPass : ScriptableRenderPass, IDisposable
    {
        /// <summary>
        /// Dispose 
        /// </summary>
        protected bool disposed;
        public void Dispose()
        {
            if (disposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);
            disposed = true;
        }
        /// <summary>
        /// Dispose managed asset
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
        }
        ~SRPPass()
        {
            Dispose(false);
        }
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
        protected Camera camera;
        /// <summary>
        /// current ScriptableRenderContext,can access from OnExecute
        /// </summary>
        protected ScriptableRenderContext context;
        public string featureName;

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
            if(Feature == null || !Feature.enabled)
                return false;

            if (Feature.isEditorOnly)
                return Application.isEditor;

            if (camera.IsGameCamera() &&!string.IsNullOrEmpty(Feature.gameCameraTag))
                return IsGameCameraValid(camera);

            return true;
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

            OnExecute(context, ref renderingData,cmd);
            //cmd.EndSampleExecute(featureName, ref context);

            cmd.Execute(ref context);
        }

        public override void OnFinishCameraStackRendering(CommandBuffer cmd)
        {
            RenderTargetHolder.Clear();
        }

        public abstract void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd);

    }
}
