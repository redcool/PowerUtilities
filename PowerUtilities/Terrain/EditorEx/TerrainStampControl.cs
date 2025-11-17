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
        public Vector3 heightOffset = Vector3.up * 2;

        void DrawHitPointLines(Color color, params Vector3[] points)
        {
            if (points.Length < 2) return;

            var inst = (TerrainStampControl)target;

            for (var x = 0; x < points.Length - 1; x++)
            {
                var p0 = points[x] + heightOffset;
                var p1 = points[x + 1] + heightOffset;

                TerrainTools.GetHitInfo(p0, out var startHitInfo);
                TerrainTools.GetHitInfo(p1, out var endHitInfo);

                // draw sphere to hitPoint
                DrawPoint(inst, p0, startHitInfo);
                // draw last point
                if (x == points.Length - 2)
                {
                    DrawPoint(inst, p1, endHitInfo);
                }

                // draw line
                Handles.color = color;
                var vector = endHitInfo.point - startHitInfo.point;
                var segmentCount = (int)(vector.magnitude / 0.5f);
                var linePoints = new List<Vector3>();
                var ratePerSegment = 1f / segmentCount;
                for (var i = 0; i < segmentCount; i++)
                {
                    linePoints.Add(startHitInfo.point + vector * ratePerSegment * i);
                    linePoints.Add(startHitInfo.point + vector * ratePerSegment * (i + 1));
                }

                // draw pos gui
                if (x % inst.posGUIIntervalCount == 0)
                {
                    var startGUIPos = HandleUtility.WorldToGUIPoint(startHitInfo.point);
                    var endGUIPos = HandleUtility.WorldToGUIPoint(endHitInfo.point);
                    Handles.BeginGUI();
                    GUI.Box(new Rect(startGUIPos, new Vector2(150, 30)), $"{startHitInfo.point}");
                    if (x == points.Length - 2)
                    {
                        GUI.Box(new Rect(endGUIPos, new Vector2(150, 30)), $"{endHitInfo.point}");
                    }
                    Handles.EndGUI();
                }

                // lines
                Handles.DrawAAPolyLine(linePoints.ToArray());
            }
            Handles.color = Color.white;
        }

        private void DrawHitPoint(Vector3 pos)
        {
            TerrainTools.GetHitInfo(pos, out var startHitInfo);
            DrawPoint(inst, pos, startHitInfo);
            DrawBrush(startHitInfo.point);

            var startGUIPos = HandleUtility.WorldToGUIPoint(startHitInfo.point);
            Handles.BeginGUI();
            GUI.Box(new Rect(startGUIPos, new Vector2(150, 30)), $"{startHitInfo.point}");
            Handles.EndGUI();
        }
        void DrawBrush(Vector3 hitPos)
        {
            if (!inst)
                return;

            var c = Color.Lerp(Color.black, Color.red, inst.brushOpacity * 0.5f + 0.5f);
            c.a = Mathf.Abs(inst.brushOpacity * 0.5f);
            c.a = Math.Max(0.2f, c.a);
            Handles.color = c;
            // brush size
            Handles.CubeHandleCap(0, hitPos, Quaternion.Euler(0, inst.brushRotation, 0), inst.brushSize, EventType.Repaint);
        }
        void Draw2HitPoint(Vector3 startPos, Vector3 endPos)
        {
            var p0 = startPos + heightOffset;
            var p1 = endPos + heightOffset;

            TerrainTools.GetHitInfo(p0, out var startHitInfo);
            TerrainTools.GetHitInfo(p1, out var endHitInfo);
            DrawPoint(inst, p0, startHitInfo);
            DrawPoint(inst, p1, endHitInfo);
            // draw pos gui
            var startGUIPos = HandleUtility.WorldToGUIPoint(startHitInfo.point);
            var endGUIPos = HandleUtility.WorldToGUIPoint(endHitInfo.point);
            Handles.BeginGUI();
            GUI.Box(new Rect(startGUIPos, new Vector2(150, 30)), $"{startHitInfo.point}");
            GUI.Box(new Rect(endGUIPos, new Vector2(150, 30)), $"{endHitInfo.point}");
            Handles.EndGUI();

            // lines
            Handles.DrawLine(p0, p1);
        }
        private static void DrawPoint(TerrainStampControl inst, Vector3 pos, RaycastHit posHitInfo)
        {
            if (!inst)
                return;

            Handles.color = Color.white * 0.5f;
            Handles.SphereHandleCap(0, pos, Quaternion.identity, inst.posSphereSize, EventType.Repaint);

            Handles.color = Color.red * 0.5f;
            Handles.SphereHandleCap(0, posHitInfo.point, Quaternion.identity, inst.posSphereSize, EventType.Repaint);
            Handles.DrawLine(pos, posHitInfo.point);

        }

        private void OnSceneGUI()
        {
            var inst = (TerrainStampControl)target;
            if (!inst) return;

            if (inst.isShowStampToolGizmos)
                DrawHitPoint(inst.pos);

            if (inst.isShowBridgeToolGizmos)
            {
                //Draw2HitPoint(inst.startPos, inst.endPos);
                DrawHitPointLines(Color.blue * 0.5f, inst.startPos, inst.endPos);
            }
            if (inst.isShowPathToolGizmos)
                DrawHitPointLines(Color.blue * 0.5f, inst.posList.ToArray());
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
        [EditorHeader("", "--- Brush Options",indentLevel =0)]
        [Tooltip("height map brush texture")]
        public Texture2D brushTexture;
        public Texture2D filterTexture;

        [Tooltip("brush size scale")]
        [Range(0.001f, 500)] public float brushSize = 20;

        [Tooltip("brush rotation")]
        [Range(0, 360)] public float brushRotation = 0;

        [Tooltip("brush dir,negative minus height,positive add height")]
        [Range(-1, 1)] public float brushOpacity = 0.1f;

        [Tooltip("sample spline segment's distance,BridgeTool,PathTools use")]
        [Min(0.01f)] public float distanceSegment = 1f;

        //---------------- stamp tool
        [EditorHeader("", "--- Stamp Tool", indentLevel = 0)]
        [HelpBox] public string stampToolDesc = "Stamp Terrain once at pos";
        [EditorBox("", "posTr,pos",boxType = EditorBoxAttribute.BoxType.HBox,boxStyle = "HelpBox")]
        [Tooltip("use this tr position")]
        public Transform posTr;

        [Tooltip("ray trace origin")]
        [HideInInspector]
        [EditorDisableGroup(targetPropName = nameof(posTr))]
        public Vector3 pos = new Vector3(88, 10, -300);

        [EditorButton(onClickCall = nameof(StampHeight))]
        public bool isStampHeight;

        //---------------- bridge tool
        [EditorHeader("", "--- Bridge Tool", indentLevel = 0)]
        [HelpBox] public string bridgeToolDesc = "Bridge Terrain startPos to endPos";
        [EditorBox("", "startPosTr,startPos", boxType = EditorBoxAttribute.BoxType.HBox, boxStyle = "HelpBox")]

        [Tooltip("startPos use startPosTr.position")]
        public Transform startPosTr;
        [Tooltip("bridge start position")]
        [HideInInspector]
        [EditorDisableGroup(targetPropName = nameof(startPosTr))]
        public Vector3 startPos;

        [EditorBox("", "endPosTr,endPos", boxType = EditorBoxAttribute.BoxType.HBox, boxStyle = "HelpBox")]
        [Tooltip("endPos use endPosTr.position")]
        public Transform endPosTr;

        [Tooltip("bridge end position")]
        [HideInInspector]
        [EditorDisableGroup(targetPropName = nameof(endPosTr))]
        public Vector3 endPos;

        [EditorButton(onClickCall = nameof(BridgeHeights))]
        public bool isBridgeHeights;

        //----------------PathTool
        [EditorHeader("", "--- Path Tools", indentLevel = 0)]
        [HelpBox] public string pathToolDesc = "Set Terrain height flow with spline or posList";
        [Tooltip("PathTool Get pos from UnitySpline(need UnitySpline package) or posList")]
        public PathPosType pathPosType;
#if UNITY_SPLINES
        public SplineContainer splineContainer;
#endif

        public List<Vector3> posList = new List<Vector3>();

        [EditorBox("Path Tools Buttons", "isSetSpline,isReadSpline,isStampPaths", isShowFoldout = true, boxType = EditorBoxAttribute.BoxType.HBox,boxStyle ="HelpBox")]
        [EditorButton(onClickCall = nameof(SetSpline))]
        [Tooltip("set poslist to mainSpline")]
        public bool isSetSpline;

        [EditorButton(onClickCall = nameof(ReadSpline))]
        [HideInInspector] public bool isReadSpline;

        [EditorButton(onClickCall = nameof(StampPaths))]
        [HideInInspector] public bool isStampPaths;

        //------------------------- Debug Options
        [EditorGroup("Debug Options",true)]
        [Range(1,10)]public float posSphereSize = 5;
        [EditorGroup("Debug Options")]
        [Min(4)]public int posGUIIntervalCount = 4;
        [EditorGroup("Debug Options")]
        [Tooltip("sample count from spline, need click ReadSpline button")]
        [Min(10)]public int splineSampleCount = 100;

        [EditorGroup("Debug Options")]
        public bool isShowStampToolGizmos=true,isShowBridgeToolGizmos=true,isShowPathToolGizmos = true;


#if UNITY_EDITOR
        private void Update()
        {
            if (posTr)
                pos = posTr.position;

            if (startPosTr)
                startPos = startPosTr.position;

            if (endPosTr)
                endPos = endPosTr.position;
        }
#endif

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
            // knots position
            //posList.AddRange(spline.Knots.Select(knot => splineContainer.transform.TransformPoint(knot.Position)));

            // line sample position
            var count = splineSampleCount;
            for (int i = 0; i < count; i++)
            {
                spline.Evaluate((float)i / (count - 1), out var pos, out var tangent, out var up);
                posList.Add(splineContainer.transform.TransformPoint(pos));
            }
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

            // brush terrain
            StampHeights(hitInfoGroupList);

        }
        /// <summary>
        /// Stampe Terrain Height Path
        /// </summary>
        /// <param name="hitInfoGroupList"></param>
        public void StampHeights(List<(RaycastHit hitInfo, Vector3 pos)> hitInfoGroupList)
        {
            StampHeights(hitInfoGroupList, TerrainTools.Get_SetExactHeightMat(), brushTexture, brushSize, brushRotation, Mathf.Clamp01(brushOpacity), filterTexture);
        }

        public static void StampHeights(List<(RaycastHit hitInfo, Vector3 pos)> hitInfoGroupList, Material paintMat,
            Texture brushTexture, float brushSize, float brushRotation, float brushOpacity, Texture2D filterTexture = null, bool isNeedUndo = true)
        {
            Terrain lastTerrain = null;

            foreach (var hitInfoGroup in hitInfoGroupList)
            {
                if (!hitInfoGroup.hitInfo.collider)
                {
                    return;
                }
                var startPos = hitInfoGroup.pos;

                var t = hitInfoGroup.hitInfo.collider.GetComponent<Terrain>();
                if (!t)
                    continue;
#if UNITY_EDITOR
                // save currrent terrainData
                if (isNeedUndo)
                {
                    if (CompareTools.CompareAndSet(ref lastTerrain,t))
                    {
                        Undo.RecordObject(t.terrainData, "Terrain Paint - Raise or Lower Height");
                    }
                }
#endif
                var uv = t.WorldPosToTerrainUV(startPos);

                var targetHeight = startPos.y / t.terrainData.size.y * 0.5f;
                TerrainTools.SetupTerrainPaintMat(ref paintMat, brushTexture, new Vector4(brushOpacity, targetHeight), filterTexture);

                TerrainTools.PaintTerrainHeight(t, uv, brushSize, brushRotation, paintMat);
            }
        }
        public void StampHeight()
        {
            TerrainTools.StampTerrainHeight(pos, brushTexture, filterTexture, brushOpacity, brushSize, brushRotation);
        }

    }
}
