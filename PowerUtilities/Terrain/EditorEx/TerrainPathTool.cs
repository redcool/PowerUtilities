#if UNITY_EDITOR && UNITY_SPLINES
using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine.TerrainTools;
using UnityEngine.Splines;
using Unity.Mathematics;
using PowerUtilities;
using UnityEditor.PackageManager;
using System.Collections.Generic;

class TerrainPathTool : TerrainPaintTool<TerrainPathTool>
{
    //public Texture2D brushTexture;
    public SplineContainer splineContainer;

    private float m_BrushOpacity = 0.1f;
    private float m_BrushSize = 25f;
    private float m_BrushRotation = 0f;

    public float distancePerSegment = 10;

    // Name of the Terrain Tool. This appears in the tool UI.
    public override string GetName()
    {
        return "Sculpt/*Terrain Path Tool";
    }

    // Description for the Terrain Tool. This appears in the tool UI.
    public override string GetDescription()
    {
        return "Use unity spline modifies the Terrain heightmap.\n" +
            "hold [Control + MouseWheel] to adjust Brush Opacity.\n" +
            "hold [Control + MouseMove], leftRight:Brush Rotation,upDown : Brush Size";
    }

    // Override this function to add UI elements to the inspector
    public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
    {
        editContext.ShowBrushesGUI(5, BrushGUIEditFlags.Select);
        //brushTexture = (Texture2D)EditorGUILayout.ObjectField(brushTexture, typeof(Texture2D), false);
        GUILayout.BeginHorizontal();
        splineContainer = (SplineContainer)EditorGUILayout.ObjectField("SplineContainer",splineContainer, typeof(SplineContainer), true);
        if (!splineContainer)
        {
            if (GUILayout.Button("Create"))
            {
                var splineGo = new GameObject("Spline");
                splineContainer = splineGo.AddComponent<SplineContainer>();
            }
        }
        GUILayout.EndHorizontal();

        m_BrushOpacity = EditorGUILayout.Slider("Opacity", m_BrushOpacity, -1, 1);
        m_BrushSize = EditorGUILayout.Slider("Size", m_BrushSize, .001f, 100f);
        m_BrushRotation = EditorGUILayout.Slider("Rotation", m_BrushRotation, 0, 360);
        distancePerSegment = EditorGUILayout.Slider("distancePerSegment", distancePerSegment, 0.5f, 100);
    }

    public override void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext)
    {
        if (!editContext.hitValidTerrain)
            return;

        var e = Event.current;
        if(e.control && e.type == EventType.ScrollWheel)
        {
            const float k_mouseWheelToHeightRatio = 0.001f;
            m_BrushOpacity += e.delta.y * k_mouseWheelToHeightRatio;

            UseEventRepaint(editContext, e);
        }
        if (e.control && e.type == EventType.MouseMove)
        {
            m_BrushSize += e.delta.y * 0.1f;
            m_BrushRotation += e.delta.x * 0.5f;
            UseEventRepaint(editContext, e);
        }

        static void UseEventRepaint(IOnSceneGUI editContext, Event e)
        {
            e.Use();
            editContext.Repaint();
        }
    }

    // Render Tool previews in the SceneView
    public override void OnRenderBrushPreview(Terrain terrain, IOnSceneGUI editContext)
    {
        // Dont render preview if this isnt a Repaint
        if (Event.current.type != EventType.Repaint) return;

        if (!splineContainer)
            return;

        var curve = splineContainer.Spline;
        var segments = Mathf.Ceil(curve.GetLength() / distancePerSegment);
        for (int i = 0; i < segments; i++)
        {
            splineContainer.Evaluate(i / segments, out var pos, out var tangent, out var upVector);

            if( ! TerrainTools.GetHitInfo(pos, out var hitInfo))
                continue;

            var uv = terrain.WorldPosToTerrainUV(hitInfo.point);
            RenderBrushPreview(terrain, editContext, uv, m_BrushOpacity, m_BrushSize, m_BrushRotation);
        }
    }

    public static void RenderBrushPreview(Terrain terrain, IOnSceneGUI editContext, float2 terrainUV, float brushOpacity, float brushSize, float brushRotation)
    {
        if (!editContext.hitValidTerrain) return;

        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, terrainUV, brushSize, brushRotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
        Material previewMaterial = TerrainTools.GetDefaultBrushPreviewExMaterial();

        TerrainPaintUtilityEditor.DrawBrushPreview(paintContext, TerrainBrushPreviewMode.SourceRenderTexture, editContext.brushTexture, brushXform, previewMaterial, 0);

        var paintMat = TerrainTools.GetBuiltinPaintMaterial();
        paintMat.SetTexture("_BrushTex", editContext.brushTexture);
        paintMat.SetVector("_BrushParams", new Vector4(brushOpacity, 0.0f, 0.0f, 0.0f));

        TerrainTools.RenderIntoPaintContext(paintContext, brushXform, paintMat);

        RenderTexture.active = paintContext.oldRenderTexture;
        previewMaterial.SetTexture("_HeightmapOrig", paintContext.sourceRenderTexture);
        TerrainPaintUtilityEditor.DrawBrushPreview(paintContext, TerrainBrushPreviewMode.DestinationRenderTexture, editContext.brushTexture, brushXform, previewMaterial, 1);

        TerrainPaintUtility.ReleaseContextResources(paintContext);
    }

    // Perform painting operations that modify the Terrain texture data
    public override bool OnPaint(Terrain terrain, IOnPaint editContext)
    {
        if (!splineContainer)
            return false;

        var curve = splineContainer.Spline;
        var segments = Mathf.Ceil(curve.GetLength() / distancePerSegment);
        var worldPosList = new List<Vector3>();
        for (int i = 0; i < segments; i++)
        {
            splineContainer.Evaluate(i / segments, out var pos, out var tangent, out var upVector);
            worldPosList.Add(pos);
        }

        var hitInfoGroupList =TerrainStampControl.WorldPosToTerrainHitInfo(worldPosList, "Terrain Paths");

        var paintMat = TerrainTools.Get_SetExactHeightMat();

        // brush terrain
        TerrainStampControl.StampHeights(hitInfoGroupList, paintMat, editContext.brushTexture, m_BrushSize, m_BrushRotation, m_BrushOpacity, null);
        return false;
    }

   
}

#endif