#if UNITY_EDITOR && UNITY_SPLINES
using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine.TerrainTools;
using UnityEngine.Splines;
using Unity.Mathematics;
using PowerUtilities;
using UnityEditor.PackageManager;

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
        return "Sculpt/Terrain Path Tool";
    }

    // Description for the Terrain Tool. This appears in the tool UI.
    public override string GetDescription()
    {
        return "use unity spline modifies the Terrain heightmap.";
    }

    // Override this function to add UI elements to the inspector
    public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
    {
        editContext.ShowBrushesGUI(5, BrushGUIEditFlags.Select);
        //brushTexture = (Texture2D)EditorGUILayout.ObjectField(brushTexture, typeof(Texture2D), false);
        splineContainer = (SplineContainer)EditorGUILayout.ObjectField("SplineContainer",splineContainer, typeof(SplineContainer), true);

        m_BrushOpacity = EditorGUILayout.Slider("Opacity", m_BrushOpacity, 0, 1);
        m_BrushSize = EditorGUILayout.Slider("Size", m_BrushSize, .001f, 100f);
        m_BrushRotation = EditorGUILayout.Slider("Rotation", m_BrushRotation, 0, 360);
        distancePerSegment = EditorGUILayout.Slider("distancePerSegment", distancePerSegment, 0.01f, 100);
    }

    // Ease of use function for rendering modified Terrain Texture data into a PaintContext. This is used in both OnRenderBrushPreview and OnPaint.
    private void RenderIntoPaintContext(PaintContext paintContext, Texture brushTexture, BrushTransform brushXform)
    {
        // Get the built-in painting Material reference
        Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
        // Bind the current brush texture
        mat.SetTexture("_BrushTex", brushTexture);
        // Bind the tool-specific shader properties
        var opacity = Event.current.control ? -m_BrushOpacity : m_BrushOpacity;
        mat.SetVector("_BrushParams", new Vector4(opacity, 0.0f, 0.0f, 0.0f));
        // Setup the material for reading from/writing into the PaintContext texture data. This is a necessary step to setup the correct shader properties for appropriately transforming UVs and sampling textures within the shader
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        // Render into the PaintContext's destinationRenderTexture using the built-in painting Material - the id for the Raise/Lower pass is 0.
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
    }

    // Render Tool previews in the SceneView
    public override void OnRenderBrushPreview(Terrain terrain, IOnSceneGUI editContext)
    {
        // Dont render preview if this isnt a Repaint
        if (Event.current.type != EventType.Repaint) return;

        if (!splineContainer)
            return;

        var curve = splineContainer.Spline;
        var segments = Mathf.CeilToInt(curve.GetLength() / distancePerSegment);
        for (int i = 0; i < segments; i++)
        {
            splineContainer.Evaluate(i / segments, out var pos, out var tangent, out var upVector);

            TerrainTools.GetHitInfo(pos, out var hitInfo);
            var uv = terrain.WorldPosToTerrainUV(hitInfo.point);
            RenderBrushPreview(terrain, editContext, uv);
        }
    }

    private void RenderBrushPreview(Terrain terrain, IOnSceneGUI editContext,float2 terrainUV)
    {
        // Only do the rest if user mouse hits valid terrain
        if (!editContext.hitValidTerrain) return;

        // Get the current BrushTransform under the mouse position relative to the Terrain
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, terrainUV, m_BrushSize, m_BrushRotation);
        // Get the PaintContext for the current BrushTransform. This has a sourceRenderTexture from which to read existing Terrain texture data.
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
        // Get the built-in Material for rendering Brush Previews
        Material previewMaterial = TerrainPaintUtilityEditor.GetDefaultBrushPreviewMaterial();
        // Render the brush preview for the sourceRenderTexture. This will show up as a projected brush mesh rendered on top of the Terrain
        TerrainPaintUtilityEditor.DrawBrushPreview(paintContext, TerrainBrushPreviewMode.SourceRenderTexture, editContext.brushTexture, brushXform, previewMaterial, 0);
        // Render changes into the PaintContext destinationRenderTexture
        RenderIntoPaintContext(paintContext, editContext.brushTexture, brushXform);
        // Restore old render target.
        RenderTexture.active = paintContext.oldRenderTexture;
        // Bind the sourceRenderTexture to the preview Material. This is used to compute deltas in height
        previewMaterial.SetTexture("_HeightmapOrig", paintContext.sourceRenderTexture);
        // Render a procedural mesh displaying the delta/displacement in height from the source Terrain texture data. When modifying Terrain height, this shows how much the next paint operation will alter the Terrain height
        TerrainPaintUtilityEditor.DrawBrushPreview(paintContext, TerrainBrushPreviewMode.DestinationRenderTexture, editContext.brushTexture, brushXform, previewMaterial, 1);
        // Cleanup resources
        TerrainPaintUtility.ReleaseContextResources(paintContext);
    }

    // Perform painting operations that modify the Terrain texture data
    public override bool OnPaint(Terrain terrain, IOnPaint editContext)
    {
        // Get the current BrushTransform under the mouse position relative to the Terrain
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, m_BrushSize, m_BrushRotation);
        // Get the PaintContext for the current BrushTransform. This has a sourceRenderTexture from which to read existing Terrain texture data
        // and a destinationRenderTexture into which to write new Terrain texture data
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds());
        // Call the common rendering function used by OnRenderBrushPreview and OnPaint
        RenderIntoPaintContext(paintContext, editContext.brushTexture, brushXform);
        // Commit the modified PaintContext with a provided string for tracking Undo operations. This function handles Undo and resource cleanup for you
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Paint - Raise or Lower Height");

        // Return whether or not Trees and Details should be hidden while painting with this Terrain Tool
        return true;
    }

   
}

#endif