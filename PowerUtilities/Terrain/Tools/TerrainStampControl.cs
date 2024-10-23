using Codice.CM.Common.Merge;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;

#endif
using UnityEngine;
using UnityEngine.TerrainTools;

namespace PowerUtilities
{
#if UNITY_EDITOR
    [CustomEditor(typeof(TerrainStampControl))]
    public class TerrainStampControlEditor : PowerEditor<TerrainStampControl>
    {
        public override bool NeedDrawDefaultUI() => true;
        public override string Version => "0.0.2";

        public override void DrawInspectorUI(TerrainStampControl inst)
        {
            base.DrawInspectorUI(inst);


        }

        private void OnSceneGUI()
        {
            var inst = (TerrainStampControl)target;

            inst.GetHitInfo(inst.startPos, out var startHitInfo);
            inst.GetHitInfo(inst.endPos, out var endHitInfo);
            var startGUIPos = HandleUtility.WorldToGUIPoint(startHitInfo.point);
            var endGUIPos = HandleUtility.WorldToGUIPoint(endHitInfo.point);

            var vector = endHitInfo.point - startHitInfo.point;
            var segmentCount = (int)(vector.magnitude / inst.distanceSegment);
            var linePoints = new List<Vector3>();
            var ratePerSegment = 1 / (float)segmentCount;
            for ( var i = 0; i < segmentCount; i++)
            {
                linePoints.Add(startHitInfo.point + vector * ratePerSegment * i);
                linePoints.Add(startHitInfo.point + vector * ratePerSegment * (i+1));
            }

            Handles.BeginGUI();
            GUI.Button(new Rect(startGUIPos, new Vector2(150, 30)), $"{startHitInfo.point}");
            GUI.Button(new Rect(endGUIPos, new Vector2(150, 30)), $"{endHitInfo.point}");
            Handles.EndGUI();

            Handles.DrawAAPolyLine(linePoints.ToArray());
        }
    }
#endif

    [Serializable]
    public class TerrainPaintInfo
    {
        public Terrain terrain;
        public Vector2 uvOnTerrain;
        public Texture2D brushTexture;
        public float brushSize;
        public float brushRotation;
        public float brushOpacity;
        public bool isNeedUndo = true;
        public Material paintMat;
        public Func<Vector4> onGetBrushParams = null;

        public void Setup(Terrain t,Vector2 uv,Texture2D brushTexture,float brushSize,float brushRotation,float brushOpacity,bool isNeedUndo,Material paintMat)
        {
            this.terrain = t;
            this.uvOnTerrain = uv;
            this.brushTexture = brushTexture;   
            this.brushSize = brushSize;
            this.brushRotation = brushRotation;
            this.brushOpacity = brushOpacity;
            this.isNeedUndo = isNeedUndo;
            this.paintMat = paintMat ?? TerrainPaintUtility.GetBuiltinPaintMaterial();
        }
    }

    [ExecuteAlways]
    public class TerrainStampControl : MonoBehaviour
    {
        [HelpBox]
        public string helpBox = "Stamp base terrain tools";
        //----------------
        [Header("Brush")]
        [Tooltip("height map brush texture")]
        public Texture2D brushTexture;
        [Tooltip("brush size scale")]
        [Range(0.001f,500)]public float brushSize = 20;

        [Tooltip("brush rotation")]
        [Range(0,360)]public float brushRotation = 0;

        [Tooltip("brush dir,negative minus height,positive add height")]
        [Range(-1,1)]public float brushOpacity = 0.1f;
        //----------------
        [Header("Stamp Tool")]
        [Tooltip("ray trace origin")]
        public Vector3 pos = new Vector3(88, 10, -300);

        [EditorButton(onClickCall = "StampHeight")]
        public bool isStampHeight;

        //----------------
        [Header("Bridge Tool")]
        public Vector3 startPos;
        public Vector3 endPos;
        [Tooltip("segment's distance")]
        [Min(0.01f)]public float distanceSegment = 0.1f;
        [EditorButton(onClickCall = "BridgeHeights")]
        public bool isBridgeHeights;

        // params vo
        TerrainPaintInfo paintInfo = new TerrainPaintInfo();

        [Range(0,1)]public float testValue;

        public void BridgeHeights()
        {
            GetHitInfo(startPos, out var startHitInfo);
            GetHitInfo(endPos, out var endHitInfo);

            var vector = endHitInfo.point - startHitInfo.point;
            var segmentCount = Mathf.CeilToInt(vector.magnitude / distanceSegment);
            var ratePerSegment = 1 / (float)segmentCount;

            var hitInfoGroupList = new List<(RaycastHit hitInfo,Vector3 pos)>();
            // add start
            //hitInfoGroupList.Add((startHitInfo, startHitInfo.point));

            for (var i = 1; i < segmentCount -1; i++)
            {
                var pos = (startHitInfo.point + vector * ratePerSegment * i);

                if(! GetHitInfo(pos,out var hitInfo))
                    continue;

                var t = hitInfo.collider.GetComponent<Terrain>();
                if (!t)
                    continue;

                hitInfoGroupList.Add((hitInfo,pos));
#if UNITY_EDITOR
                Undo.RecordObject(t, "Terrain Bridge");
#endif
            }
            // draw line gizmos
            hitInfoGroupList.Add((endHitInfo,endHitInfo.point));
            foreach (var hitInfoGroup in hitInfoGroupList)
            {
                var pos = hitInfoGroup.pos;
                var hitInfo = hitInfoGroup.hitInfo;
                Debug.DrawRay(pos, Vector3.up * 10, Color.red, 10);
                Debug.DrawRay(hitInfo.point, hitInfo.normal * 8, Color.white, 10);
            }
            
            // brush terrain
            foreach (var hitInfoGroup in hitInfoGroupList)
            {
                var startPos = hitInfoGroup.pos;

                var t = hitInfoGroup.hitInfo.collider.GetComponent<Terrain>();
                
                var uv = t.WorldPosToTerrainUV(startPos);

                paintInfo.Setup(t, uv, brushTexture, brushSize, brushRotation, brushOpacity, false, TerrainTools.Get_SetExactHeightMat());

                var targetHeight = startPos.y / t.terrainData.size.y * 0.5f;
                paintInfo.onGetBrushParams = ()=> new Vector4(brushOpacity, targetHeight);

                paintInfo.paintMat.SetTexture("_FilterTex", Texture2D.whiteTexture);

                PaintTerrain(paintInfo);
            }

        }

        public void StampHeight()
        {
            if (!GetHitInfo(pos,out var hitInfo))
            {
                Debug.Log("hit nothing");
                return;
            }

            var t = hitInfo.collider.GetComponent<Terrain>();
            if (!t)
                return;
            var uv = t.WorldPosToTerrainUV(pos);

            paintInfo.Setup(t, uv, brushTexture, brushSize, brushRotation, brushOpacity, true, null);
            PaintTerrain(paintInfo);
        }

        public bool GetHitInfo(Vector3 pos, out RaycastHit hitInfo)
        {
            var ray = new Ray(pos, Vector3.down);
            return Physics.Raycast(ray, out hitInfo, float.MaxValue);
        }

        private void OnDrawGizmosSelected()
        {
            DrawHitInfo(pos,Color.blue);
            DrawHitInfo(startPos, Color.red);
            DrawHitInfo(endPos, Color.green);

            void DrawHitInfo(Vector3 pos, Color color)
            {
                if (!GetHitInfo(pos,out var hitInfo))
                    return;

                Debug.DrawRay(hitInfo.point, hitInfo.normal * 100, color);
            }
        }


        public static bool PaintTerrain(TerrainPaintInfo info)
        {
#if UNITY_EDITOR
            if (info.isNeedUndo)
                Undo.RecordObject(info.terrain.terrainData, "Terrain Paint - Raise or Lower Height");
#endif
            // Get the current BrushTransform under the mouse position relative to the Terrain
            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(info.terrain, info.uvOnTerrain, info.brushSize, info.brushRotation);
            // Get the PaintContext for the current BrushTransform. This has a sourceRenderTexture from which to read existing Terrain texture data
            // and a destinationRenderTexture into which to write new Terrain texture data
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(info.terrain, brushXform.GetBrushXYBounds());
            // Call the common rendering function used by OnRenderBrushPreview and OnPaint
            RenderIntoPaintContext(paintContext, brushXform, info);
            // Commit the modified PaintContext with a provided string for tracking Undo operations. This function handles Undo and resource cleanup for you
            TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Paint - Raise or Lower Height");
            // Return whether or not Trees and Details should be hidden while painting with this Terrain Tool
            return true;
        }

        public static void RenderIntoPaintContext(PaintContext paintContext, BrushTransform brushXform,TerrainPaintInfo info)
        {
            Material mat = info.paintMat;
            // Bind the current brush texture
            mat.SetTexture("_BrushTex", info.brushTexture);
            // Bind the tool-specific shader properties
            var brushParams = new Vector4(info.brushOpacity,0);
            if (info.onGetBrushParams != null)
                brushParams = info.onGetBrushParams();

            mat.SetVector("_BrushParams", brushParams);
            // Setup the material for reading from/writing into the PaintContext texture data. This is a necessary step to setup the correct shader properties for appropriately transforming UVs and sampling textures within the shader
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
            // Render into the PaintContext's destinationRenderTexture using the built-in painting Material - the id for the Raise/Lower pass is 0.
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        }

    }
}
