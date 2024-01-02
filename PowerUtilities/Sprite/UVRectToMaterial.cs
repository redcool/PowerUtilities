namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;

#if UNITY_EDITOR

    [CustomEditor(typeof(UVRectToMaterial))]
    public class UVRectToMaterialEditor : PowerEditor<UVRectToMaterial>
    {
        public override bool NeedDrawDefaultUI() => true;
        public override string Version => "0.0.1";

        public override void DrawInspectorUI(UVRectToMaterial inst)
        {
            if (GUILayout.Button("Set sprite uv"))
            {
                inst.SetUV();
            }
        }
    }
#endif
    /// <summary>
    /// send sprite's uv in altas to material(_MainTex_ST
    /// </summary>
    [ExecuteAlways]
    public class UVRectToMaterial : MonoBehaviour
    {
        [HelpBox]
        public const string helpStr = "Send sprite's uv in atlas to material";

        public Sprite sprite;

        [Header("Material Options")]
        [Tooltip("generate material instance")]
        public bool isUseMaterialInstance;

        [Tooltip("shader corresponding texture name")]
        public string _MainTexName = "_MainTex";

        [Header("PowerVFX Options")]
        [Tooltip("disable texture auto offset")]
        public bool isDisableMainTexAutoOffset = true;

        [Tooltip("use powervfx minVersion")]
        public bool isUseMinVersion = true;

        Renderer render;

        [Header("DebugInfo")]
        //[EditorGroup("DebugInfo",true)]
        [SerializeField]
        //[ListItemDraw("x,y,z,w", "100,100,100,100")]
        Vector4 spriteRect;

        //[EditorGroup("DebugInfo")]
        [SerializeField]
        //[ListItemDraw("x:,x,y:,y,z:,z,w:,w", "15,50,15,80,15,80,15,80")]
        Vector4 spriteUVST;

        private UnityEngine.Object lastSelectionObject;

        public void SetUV()
        {
            if (!render)
                render = GetComponent<Renderer>();
            if (!sprite || !render)
                return;

            var mat = isUseMaterialInstance ? render.material : render.sharedMaterial;

            mat.SetTexture(_MainTexName, sprite.texture);

            var rect = sprite.rect;
            spriteRect = new Vector4(rect.x, rect.y, rect.width, rect.height);
            spriteUVST = sprite.GetSpriteUVScaleOffset();
            mat.SetVector($"{_MainTexName}_ST", spriteUVST);

            TrySetupPowerVFXMat(mat);
        }

        private void TrySetupPowerVFXMat(Material mat)
        {
            if (!mat.shader.name.Contains("PowerVFX"))
                return;

            mat.SetFloat("_MainTexOffsetStop", isDisableMainTexAutoOffset?1:0);

            if(isUseMinVersion != mat.IsKeywordEnabled(ShaderKeywords.MIN_VERSION))
            {
                mat.SetKeyword(ShaderKeywords.MIN_VERSION, isUseMinVersion);

                lastSelectionObject = Selection.activeObject;
                Selection.activeObject = null;

                StartCoroutine(WaitForSelectAgain());
            }
        }

        IEnumerator WaitForSelectAgain()
        {
            yield return new WaitForEndOfFrame();
            Selection.activeObject = lastSelectionObject;
        }
    }
}
