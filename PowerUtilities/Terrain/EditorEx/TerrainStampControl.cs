namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.Mathematics;

#if UNITY_EDITOR
    using UnityEditor;
#endif
#if UNITY_SPLINES
    using UnityEngine.Splines;
#endif

    using UnityEngine;
    using UnityEngine.TerrainTools;

#if UNITY_EDITOR
    [CustomEditor(typeof(TerrainStampControl))]
    public class TerrainStampControlEditor : PowerEditor<TerrainStampControl>
    {
        public override bool NeedDrawDefaultUI() => true;
        public override string Version => "0.0.3";

        public override void DrawInspectorUI(TerrainStampControl inst)
        {
            base.DrawInspectorUI(inst);
        }

        void DrawHitPointLines(params Vector3[] points)
        {
            if (points.Length < 2) return;

            var inst = (TerrainStampControl)target;

            for (var x = 0; x < points.Length - 1; x++)
            {
                var p0 = points[x];
                var p1 = points[x + 1];

                TerrainTools.GetHitInfo(p0, out var startHitInfo);
                TerrainTools.GetHitInfo(p1, out var endHitInfo);
                var startGUIPos = HandleUtility.WorldToGUIPoint(startHitInfo.point);
                var endGUIPos = HandleUtility.WorldToGUIPoint(endHitInfo.point);

                var vector = endHitInfo.point - startHitInfo.point;
                var segmentCount = (int)(vector.magnitude / inst.distanceSegment);
                var linePoints = new List<Vector3>();
                var ratePerSegment = 1 / (float)segmentCount;
                for (var i = 0; i < segmentCount; i++)
                {
                    linePoints.Add(startHitInfo.point + vector * ratePerSegment * i);
                    linePoints.Add(startHitInfo.point + vector * ratePerSegment * (i + 1));
                }

                // draw pos gui
                Handles.BeginGUI();
                GUI.Box(new Rect(startGUIPos, new Vector2(150, 30)), $"{startHitInfo.point}");
                GUI.Box(new Rect(endGUIPos, new Vector2(150, 30)), $"{endHitInfo.point}");
                Handles.EndGUI();

                Handles.DrawAAPolyLine(linePoints.ToArray());
            }
        }

        private void OnSceneGUI()
        {
            var inst = (TerrainStampControl)target;

            DrawHitPointLines(inst.startPos, inst.endPos);

            DrawHitPointLines(inst.posList.ToArray());
        }
    }
#endif

    [ExecuteAlways]
    public class TerrainStampControl : MonoBehaviour
    {

        public enum PathPosType
        {
            UnitySpline,PosList
        }

        [HelpBox]
        public string helpBox = "Stamp base terrain tools";
        //----------------
        [EditorHeader("", "--- Brush Options")]
        [Tooltip("height map brush texture")]
        public Texture2D brushTexture;
        public Texture2D filterTexture;

        [Tooltip("brush size scale")]
        [Range(0.001f, 500)] public float brushSize = 20;

        [Tooltip("brush rotation")]
        [Range(0, 360)] public float brushRotation = 0;

        [Tooltip("brush dir,negative minus height,positive add height")]
        [Range(-1, 1)] public float brushOpacity = 0.1f;
        [Tooltip("segment's distance")]
        [Min(0.01f)] public float distanceSegment = 0.1f;

        //----------------
        [EditorHeader("", "--- Stamp Tool")]
        [Tooltip("ray trace origin")]
        public Vector3 pos = new Vector3(88, 10, -300);

        [EditorButton(onClickCall = "StampHeight")]
        public bool isStampHeight;

        //----------------
        [EditorHeader("", "--- Bridge Tool")]
        public Vector3 startPos;
        public Vector3 endPos;
        [EditorButton(onClickCall = "BridgeHeights")]
        public bool isBridgeHeights;

        //----------------PathTool
        [EditorHeader("", "--- Path Tools")]
        [Tooltip("PathTool Get pos from UnitySpline or posList")]
        public PathPosType pathPosType;
#if UNITY_SPLINES
        public SplineContainer splineContainer;
#endif

        //[EditorSceneView()]
        public List<Vector3> posList = new List<Vector3>();

        [EditorBoxAttribute("Path Tools Buttons", "isSetSpline,isReadSpline,isStampPaths", isShowFoldout = true, boxType = EditorBoxAttribute.BoxType.HBox)]
        [EditorButton(onClickCall = "SetSpline")]
        [Tooltip("set poslist to mainSpline")]
        public bool isSetSpline;

        [EditorButton(onClickCall = "ReadSpline")]
        [HideInInspector] public bool isReadSpline;

        [EditorButton(onClickCall = "StampPaths")]
        [HideInInspector] public bool isStampPaths;

        //============= debug
        [Range(0, 1)] public float testValue;

        public void SetSpline()
        {
#if UNITY_SPLINES
            if (!splineContainer)
                splineContainer = gameObject.GetOrAddComponent<SplineContainer>();
            if (!splineContainer)
                return;

            var spline = splineContainer.Spline;
            spline.AddRange(posList.Select(p => (float3)splineContainer.transform.InverseTransformPoint(p)));
#endif
        }

        public void ReadSpline()
        {
#if UNITY_SPLINES
            if (!splineContainer)
                return;
            var spline = splineContainer.Spline;
            posList.Clear();
            posList.AddRange(spline.Knots.Select(knot => splineContainer.transform.TransformPoint(knot.Position)));
#endif
        }

        public void StampPaths()
        {
            var worldPosList = new List<Vector3>();
#if UNITY_SPLINES
            if (pathPosType == PathPosType.UnitySpline)
            {
                if (!splineContainer)
                    return;

                var spline = splineContainer.Spline;
                var curveLen = spline.GetLength();
                var segments = Mathf.Ceil(curveLen / distanceSegment);

                for (int i = 0; i < segments; i++)
                {
                    splineContainer.Evaluate(i / segments, out var pos, out var tangent, out var upVector);
                    worldPosList.Add(pos);
                }
            }
#endif
            if (pathPosType == PathPosType.PosList)
            {
                worldPosList.AddRange(posList);
            }

            if (worldPosList.Count == 0)
                return;

            var hitInfoGroupList = WorldPosToTerrainHitInfo(worldPosList, "Terrain Paths");
            // brush terrain
            StampHeights(hitInfoGroupList);
        }

        public static List<(RaycastHit hitInfo, Vector3 pos)> WorldPosToTerrainHitInfo(List<Vector3> worldPoints, string undoName = "Terrain Stamp")
        {
            var list = new List<(RaycastHit hitInfo, Vector3 pos)>();
            var sets = new HashSet<Terrain>();
            foreach (var pos in worldPoints)
            {
                if (!TerrainTools.GetHitInfo(pos, out var hitInfo))
                    continue;

                var t = hitInfo.collider.GetComponent<Terrain>();
                if (!t)
                    continue;
                list.Add((hitInfo, pos));
                sets.Add(t);
            }
#if UNITY_EDITOR
            // maybe hit other terrain ?
            var ts = sets.ToArray();
            foreach (var t in ts)
                Undo.RecordObject(t, undoName);
#endif

            return list;
        }


        public void BridgeHeights()
        {
            TerrainTools.GetHitInfo(startPos, out var startHitInfo);
            TerrainTools.GetHitInfo(endPos, out var endHitInfo);

            var vector = endHitInfo.point - startHitInfo.point;
            var segmentCount = Mathf.CeilToInt(vector.magnitude / distanceSegment);
            var ratePerSegment = 1 / (float)segmentCount;

            // collect all worldPos
            var worldPosList = new List<Vector3>();
            for (var i = 1; i < segmentCount - 1; i++)
            {
                var pos = (startHitInfo.point + vector * ratePerSegment * i);
                worldPosList.Add(pos);
            }
            var hitInfoGroupList = WorldPosToTerrainHitInfo(worldPosList, "Terrain Bridges");
            hitInfoGroupList.Add((endHitInfo, endHitInfo.point));

            // draw line gizmos
#if UNITY_EDITOR
            foreach (var hitInfoGroup in hitInfoGroupList)
            {
                var pos = hitInfoGroup.pos;
                var hitInfo = hitInfoGroup.hitInfo;
                Debug.DrawRay(pos, Vector3.up * 10, Color.red, 10);
                Debug.DrawRay(hitInfo.point, hitInfo.normal * 8, Color.white, 10);
            }
#endif

            // brush terrain
            StampHeights(hitInfoGroupList);

        }
        /// <summary>
        /// Stampe Terrain Height Path
        /// </summary>
        /// <param name="hitInfoGroupList"></param>
        public void StampHeights(List<(RaycastHit hitInfo, Vector3 pos)> hitInfoGroupList)
        {
            StampHeights(hitInfoGroupList, TerrainTools.Get_SetExactHeightMat(), brushTexture, brushSize, brushRotation, brushOpacity, filterTexture);
        }

        public static void StampHeights(List<(RaycastHit hitInfo, Vector3 pos)> hitInfoGroupList, Material paintMat,
            Texture brushTexture, float brushSize, float brushRotation, float brushOpacity, Texture2D filterTexture = null, bool isNeedUndo = true)
        {
            Terrain lastTerrain = null;

            foreach (var hitInfoGroup in hitInfoGroupList)
            {
                var startPos = hitInfoGroup.pos;

                var t = hitInfoGroup.hitInfo.collider.GetComponent<Terrain>();
                if (!t)
                    continue;
#if UNITY_EDITOR
                // save currrent terrainData
                if (isNeedUndo)
                {
                    if (lastTerrain != t)
                    {
                        lastTerrain = t;
                        Undo.RecordObject(t.terrainData, "Terrain Paint - Raise or Lower Height");
                    }
                }
#endif
                var uv = t.WorldPosToTerrainUV(startPos);

                var targetHeight = startPos.y / t.terrainData.size.y * 0.5f;
                TerrainTools.SetupTerrainPaintMat(ref paintMat, brushTexture, new Vector4(brushOpacity, targetHeight), filterTexture);

                OnPaint(t, uv, brushSize, brushRotation, paintMat);
            }
        }

        public void StampHeight()
        {
            if (!TerrainTools.GetHitInfo(pos, out var hitInfo))
            {
                Debug.Log("hit nothing");
                return;
            }

            var t = hitInfo.collider.GetComponent<Terrain>();
            if (!t)
                return;
            var uv = t.WorldPosToTerrainUV(pos);

            var paintMat = TerrainTools.GetBuiltinPaintMaterial();
            TerrainTools.SetupTerrainPaintMat(ref paintMat, brushTexture, new Vector4(brushOpacity, 0), filterTexture);

            OnPaint(t,uv,brushSize, brushRotation, paintMat);
        }


        private void OnDrawGizmosSelected()
        {
            DrawHitInfo(pos, Color.blue);
            DrawHitInfo(startPos, Color.red);
            DrawHitInfo(endPos, Color.green);

            void DrawHitInfo(Vector3 pos, Color color)
            {
                if (!TerrainTools.GetHitInfo(pos, out var hitInfo))
                    return;

                Debug.DrawRay(hitInfo.point, hitInfo.normal * 100, color);
            }
        }


        public static bool OnPaint(Terrain terrain,Vector2 uvOnTerrain,float brushSize,float brushRotation,Material mat)
        {
            var brushTransform = TerrainPaintUtility.CalculateBrushTransform(terrain, uvOnTerrain, brushSize, brushRotation);
            var paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushTransform.GetBrushXYBounds());

            TerrainTools.RenderIntoPaintContext(paintContext, brushTransform,mat);
            TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Paint - Raise or Lower Height");
            return true;
        }
    }
}
