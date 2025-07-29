using PowerUtilities.RenderFeatures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public class SetShaderVariables : MonoBehaviour
    {
        [EditorButton(onClickCall = "SetVariables")]
        public bool isSetGlobalVars;

        [Tooltip("SetVariables per frame")]
        public bool isUpdateMode;

        [Header("--- Set Global Variables")]
        [Tooltip("binding global float variables")]
        public List<ShaderValue<float>> floatValues = new List<ShaderValue<float>>();
        public List<ShaderValue<int>> intValues = new List<ShaderValue<int>>();
        public List<ShaderValue<Vector4>> vectorValues = new List<ShaderValue<Vector4>>();
        public List<ShaderValue<Texture>> textureValues = new List<ShaderValue<Texture>>();

        [Header("Set MainCamera Info")]
        [Tooltip("override isSetMainCameraInfo")]
        public bool isOverrideSetMainCameraInfo;

        [Tooltip("setup camera's rotation matrix(_CameraRot,_CameraYRot),Bill.shader use these")]
        public bool isSetMainCameraInfo = true;

        [Header("set unscaled time")]
        [Tooltip("override isOverrideUnscaledTime")]
        public bool isOverrideUnscaledTime;

        [Tooltip("replace _Time(xyzw) ,use Time.unscaledTime")]
        public bool isSetUnscaledTime;

        [Header("Global Lod Settings,effect subShader")]
        public bool isOverrideGlobalMaxLod;
        public int globalMaxLod = 600;

        [Header("MIN_VERSION")]
        [Tooltip("override isMinVersionOn")]
        public bool isOverrideMinVersion;

        [Tooltip("subshader lod [100,300]")]
        public bool isMinVersionOn;

        [Header("Set Per Shader")]
        public List<Shader> shaderList = new();
        public int shaderMaxLod = 600;

        [EditorButton(onClickCall = "SetShadersMaxLod")]
        public bool isSetShaderLod;

        // Start is called before the first frame update
        void OnEnable()
        {
            SetVariables();
        }

        // Update is called once per frame
        void Update()
        {
            if (isUpdateMode)
                SetVariables();
        }

        public void SetVariables()
        {
            foreach (var v in floatValues)
                if (v.IsValid) Shader.SetGlobalFloat(v.name, v.value);

            foreach (var v in vectorValues)
                if (v.IsValid) Shader.SetGlobalVector(v.name, v.value);

            foreach (var v in intValues)
                if (v.IsValid) Shader.SetGlobalInt(v.name, v.value);

            foreach (var v in textureValues)
                if (v.IsValid) Shader.SetGlobalTexture(v.name, v.value);


            if (isOverrideSetMainCameraInfo && isSetMainCameraInfo)
            {
                var cam = Camera.main;
                if (cam)
                {
                    Shader.SetGlobalMatrix("_CameraRot", Matrix4x4.Rotate(Quaternion.Euler(cam.transform.eulerAngles)));
                    Shader.SetGlobalMatrix("_CameraYRot", Matrix4x4.Rotate(Quaternion.Euler(0, cam.transform.eulerAngles.y, 0)));
                }
            }

            if (isOverrideGlobalMaxLod)
                Shader.globalMaximumLOD = globalMaxLod;

            if (isOverrideUnscaledTime && isSetUnscaledTime)
            {
                SetShaderTimeValues(Time.unscaledTime, Time.unscaledDeltaTime, Time.smoothDeltaTime);
            }

            SetShadersMaxLod();
        }

        public static void SetShaderTimeValues(float time, float deltaTime, float smoothDeltaTime)
        {
            //float timeEights = time / 8f;
            //float timeFourth = time / 4f;
            //float timeHalf = time / 2f;

            // Time values
            Vector4 timeVector = time * new Vector4(1f / 20f, 1f, 2f, 3f);
            //Vector4 sinTimeVector = new Vector4(Mathf.Sin(timeEights), Mathf.Sin(timeFourth), Mathf.Sin(timeHalf), Mathf.Sin(time));
            //Vector4 cosTimeVector = new Vector4(Mathf.Cos(timeEights), Mathf.Cos(timeFourth), Mathf.Cos(timeHalf), Mathf.Cos(time));
            //Vector4 deltaTimeVector = new Vector4(deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime);
            //Vector4 timeParametersVector = new Vector4(time, Mathf.Sin(time), Mathf.Cos(time), 0.0f);

            Shader.SetGlobalVector(ShaderPropertyIds.time, timeVector);
            //cmd.SetGlobalVector(ShaderPropertyIds.sinTime, sinTimeVector);
            //cmd.SetGlobalVector(ShaderPropertyIds.cosTime, cosTimeVector);
            //cmd.SetGlobalVector(ShaderPropertyIds.deltaTime, deltaTimeVector);
            //cmd.SetGlobalVector(ShaderPropertyIds.timeParameters, timeParametersVector);
        }

        public void SetShadersMaxLod()
        {
            foreach (var s in shaderList)
            {
                if (!s)
                    continue;
                s.maximumLOD = shaderMaxLod;
            }
        }
    }
}
