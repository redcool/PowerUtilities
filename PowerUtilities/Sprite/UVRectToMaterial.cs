namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using UnityEngine.UI;
    using Unity.Mathematics;
    using Object = UnityEngine.Object;

#if UNITY_EDITOR

    [CustomEditor(typeof(UVRectToMaterial))]
    public class UVRectToMaterialEditor : PowerEditor<UVRectToMaterial>
    {
        public override bool NeedDrawDefaultUI() => true;
        public override string Version => "0.0.4";

        readonly GUIContent SetUVBySpriteRect = new GUIContent("SetUVBySprite", "Set material uv by sprite size ");
        readonly GUIContent SetScaleBySpriteSize = new GUIContent("SetTransform", "Set (Scale,Position) by sprite size");

        public override void DrawInspectorUI(UVRectToMaterial inst)
        {
            GUILayout.Label("Options", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(SetUVBySpriteRect))
            {
                inst.SetUV();
            }
            if (GUILayout.Button(SetScaleBySpriteSize))
            {
                inst.SetTransform();
            }
            GUILayout.EndHorizontal();
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
        public string helpStr = "Send sprite's uv in atlas to material";

        public Sprite sprite;
        Sprite lastSprite;

        [Header("Material Options")]
        [Tooltip("use material instance for 3d mesh, only work in PlayingMode (not EditorMode)")]
        public bool isUseMaterialInstance = true;

        [Tooltip("shader corresponding texture name")]
        public string _MainTexName = "_MainTex";

        [Tooltip("sprite's start uv,xy:uv start,z: sprite rendering on?")]
        public string _SpriteUVStartName = "_SpriteUVStart";

        [EditorHeader("","--- Color")]
        [Tooltip("change material's color")]
        public string _ColorName = "_Color";
        public Color color = Color.white;

        [Header("PowerVFX Options")]
        [Tooltip("disable texture auto offset")]
        public bool isDisableMainTexAutoOffset = true;

        [Tooltip("use powervfx minVersion")]
        public bool isUseMinVersion = true;


        Renderer render; // 3d renderer
        Image uiImage; // ui
        Material mat; // current used material

        //=========================
        [Header("DebugInfo")]
        [EditorGroup("DebugInfo", true)]
        [SerializeField]
        [ListItemDraw("x:,x,y:,y,z:,z,w:,w", "15,80,15,80,15,80,15,80", isShowTitleRow = true)]
        Vector4 spriteRect;

        [EditorGroup("DebugInfo")]
        [SerializeField]
        [ListItemDraw("x:,x,y:,y,z:,z,w:,w", "15,80,15,80,15,80,15,80", isShowTitleRow = true)]
        Vector4 spriteUVST;

        Object lastSelectionObject;

        MaterialPropertyBlock block; // block will break srp batch, will apply in EditorMode

        private void Update()
        {
            if (block == null)
                block = new MaterialPropertyBlock();

            if (lastSprite != sprite)
            {
                lastSprite = sprite;
                SetUV();
            }

            if (mat)
            {
                mat.SetColor(_ColorName, color, block);
            }
        }

        private void OnEnable()
        {
            SetupMeshFilter(this);

            SetUV();
        }

        private void OnDisable()
        {
            if (mat)
            {
                //mat.SetVector($"{_MainTexName}_ST", spriteUVST);
                mat.SetVector(_SpriteUVStartName, new Vector4(0, 0, 0, 0));
            }
        }

        public static void SetupMeshFilter(UVRectToMaterial inst)
        {
            var mf = inst.GetComponent<MeshFilter>();
            if (!mf)
            {
                mf = inst.gameObject.AddComponent<MeshFilter>();
                mf.sharedMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            }
        }

        public void SetUV()
        {
            SetupComponents();
            if (!sprite || (!render && !uiImage))
                return;

            mat = GetMaterial();
            if (!mat)
                return;

            mat.SetTexture(_MainTexName, sprite.texture, block);

            var rect = sprite.rect;
            // debug
            spriteRect = new Vector4(rect.x, rect.y, rect.width, rect.height);
            // xy : tiling, zw:offset
            spriteUVST = sprite.GetSpriteUVScaleOffset();
            mat.SetVector($"{_MainTexName}_ST", spriteUVST, block);

            // xy : offset,powervfx use this ,do sprite uv move
            mat.SetVector(_SpriteUVStartName, new Vector4(spriteUVST.z, spriteUVST.w, 1, 0), block);
            

            TrySetupPowerVFXMat(mat);

#if UNITY_EDITOR
            ApplyBlock();

            //========= inner methods
            void ApplyBlock()
            {
                if (render && !Application.isPlaying)
                {
                    render.SetPropertyBlock(block);
                }
            }
#endif
        }

        public void SetTransform()
        {
            if (!sprite)
                return;

            var w = sprite.rect.width;
            var h = sprite.rect.height;
            var sx = w / sprite.pixelsPerUnit;
            var sy = h / sprite.pixelsPerUnit;

            var scale = transform.localScale;
            scale.Set(sx, sy, scale.z);
            transform.localScale = scale;

            var offset = new Vector2(sprite.pivot.x/sprite.rect.width-0.5f, sprite.pivot.y/sprite.rect.height-0.5f);
            var pos = transform.localPosition;
            pos.x = -scale.x * offset.x;
            pos.y = -scale.y * offset.y;
            transform.localPosition = pos;
        }

        private void SetupComponents()
        {
            if (!render)
                render = GetComponent<Renderer>();

            if (!uiImage)
            {
                uiImage = GetComponent<Image>();
            }

            // setup Image
            if (uiImage && uiImage.sprite)
            {
                sprite = uiImage.sprite;
                uiImage.sprite = null; // only use material's texture
            }
        }

        Material GetMaterial()
        {
            if (render)
            {
                if (!Application.isPlaying)
                    return render.sharedMaterial;

                return isUseMaterialInstance ? render.material : render.sharedMaterial;
            }

            if (uiImage)
            {
                // dont change default UI material
                if (uiImage.materialForRendering == uiImage.defaultMaterial)
                    return null;

                return uiImage.materialForRendering;
            }

            return null;
        }

        private void TrySetupPowerVFXMat(Material mat)
        {
            if (!mat.shader.name.Contains("PowerVFX"))
                return;

            mat.SetVector($"{_MainTexName}_ST", new Vector4(spriteUVST.x, spriteUVST.y, 0, 0), block); // dont need offset,offset used for uv move
            mat.SetFloat("_MainTexOffsetStop", isDisableMainTexAutoOffset ? 1 : 0,block);

            if (isUseMinVersion != mat.IsKeywordEnabled(ShaderKeywords.MIN_VERSION))
            {
                mat.SetKeyword(ShaderKeywords.MIN_VERSION, isUseMinVersion);
            }
        }

    }
}
