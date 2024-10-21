using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TerrainTools;

namespace PowerUtilities
{
    public class TerrainStampControl : MonoBehaviour
    {
        [Header("Stamp Height")]
        [Tooltip("stamp height map")]
        public Texture2D brushTexture;
        [Tooltip("ray trace origin")]
        public Vector3 pos = new Vector3(88, 10, -300);

        [Tooltip("stamp size scale")]
        [Range(0.001f,500)]public float brushSize = 20;

        [Tooltip("stamp rotation")]
        [Range(0,360)]public float brushRotation = 0;

        [Tooltip("negative minus height,positive add height")]
        [Range(-1,1)]public float brushOpacity = 0.1f;

        [EditorButton(onClickCall = "StampHeight")]
        public bool isStampHeight;


        public void StampHeight()
        {
            var ray = new Ray(pos, Vector3.down);
            var isHit = Physics.Raycast(ray, out RaycastHit hitInfo,float.MaxValue);
            if (!isHit)
            {
                Debug.Log("hit nothing");
                return;
            }
            Debug.DrawRay(hitInfo.point,hitInfo.normal*100, Color.blue,1);

            var t = hitInfo.collider.GetComponent<Terrain>();
            var td = t.terrainData;

            var localPos = transform.InverseTransformPoint(pos);
            var uv = new Vector2(localPos.x / td.size.x, localPos.z / td.size.z);
            PaintTerrain(t, uv, brushTexture, brushSize, brushRotation, brushOpacity);
        }

        public static bool PaintTerrain(Terrain terrain, Vector2 uvOnTerrain,Texture2D brushTexture,float brushSize,float brushRotation,float brushOpaque)
        {
            // Get the current BrushTransform under the mouse position relative to the Terrain
            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, uvOnTerrain, brushSize, brushRotation);
            // Get the PaintContext for the current BrushTransform. This has a sourceRenderTexture from which to read existing Terrain texture data
            // and a destinationRenderTexture into which to write new Terrain texture data
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds());
            // Call the common rendering function used by OnRenderBrushPreview and OnPaint
            RenderIntoPaintContext(paintContext, brushTexture, brushXform, brushOpaque);
            // Commit the modified PaintContext with a provided string for tracking Undo operations. This function handles Undo and resource cleanup for you
            TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Paint - Raise or Lower Height");
#if UNITY_EDITOR
            Undo.RecordObject(terrain.terrainData, "Terrain Paint");
#endif
            // Return whether or not Trees and Details should be hidden while painting with this Terrain Tool
            return true;
        }

        public static void RenderIntoPaintContext(PaintContext paintContext, Texture brushTexture, BrushTransform brushXform,float brushOpacity)
        {
            // Get the built-in painting Material reference
            Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
            // Bind the current brush texture
            mat.SetTexture("_BrushTex", brushTexture);
            // Bind the tool-specific shader properties
            var opacity = brushOpacity;
            mat.SetVector("_BrushParams", new Vector4(opacity, 0.0f, 0.0f, 0.0f));
            // Setup the material for reading from/writing into the PaintContext texture data. This is a necessary step to setup the correct shader properties for appropriately transforming UVs and sampling textures within the shader
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
            // Render into the PaintContext's destinationRenderTexture using the built-in painting Material - the id for the Raise/Lower pass is 0.
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        }

    }
}
