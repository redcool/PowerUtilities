using UnityEngine;
using UnityEngine.Rendering;
using Color = UnityEngine.Color;
using Vector3 = UnityEngine.Vector3;

namespace PowerUtilities
{
    public static class DebugTools
    {
        /// <summary>
        /// draw lines pairs
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        public static void DrawLines((Vector3 start, Vector3 end)[] edges, Color color = default, float duration = 0)
        {
            color = color == default ? Color.white : color;
            for (int i = 0; i < edges.Length; i++)
            {
                var edge = edges[i];

                Debug.DrawLine(edge.start, edge.end, color, duration);
            }
        }

        /// <summary>
        /// draw line strip,{(0,1),(1,2),...}
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        public static void DrawLineStrip(Vector3[] vertices, Color color = default, float duration = 0)
        {
            if (vertices.Length < 2)
                return;

            color = color == default ? Color.white : color;

            for (int i = 0; i < vertices.Length; i++)
            {
                var p0 = vertices[i];
                var nextId = (i + 1) % vertices.Length;
                var p1 = vertices[nextId];

                Debug.DrawLine(p0, p1, color, duration);
            }
        }

        /// <summary>
        /// spread lines
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        public static void DrawLineSpread(Vector3[] vertices,Color color = default, float duration = 0)
        {
            if (vertices.Length < 2)
                return;

            var start = vertices[0];
            for (int i = 1;i < vertices.Length; i++)
            {
                Debug.DrawLine(start, vertices[i], color, duration);
            }
        }

        /// <summary>
        /// line fans
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="color"></param>
        /// <param name="duration"></param>
        public static void DrawLineFan(Vector3[] vertices, Color color = default, float duration = 0)
        {
            if (vertices.Length < 2)
                return;

            var v0 = vertices[0];
            var v1 = vertices[1];
            Debug.DrawLine(v0, v1, color, duration);

            for (int i = 2; i < vertices.Length; i++)
            {
                v0 = vertices[i - 2];
                v1 = vertices[i - 1];
                var v = vertices[i];
                Debug.DrawLine(v0, v, color, duration);
                Debug.DrawLine(v, v1, color, duration);
            }
        }

        public static void DrawLineCube(Bounds bounds, Color color = default, float duration = 0)
        {
            var min = bounds.min;
            var max = bounds.max;

            var vertices = new[]
            {
                min,
                new Vector3(min.x,min.y,max.z),
                new Vector3(max.x,min.y,max.z),
                new Vector3(max.x,min.y,min.z),

                new Vector3(min.x,max.y,min.z),
                new Vector3(min.x,max.y,max.z),
                max,
                new Vector3(max.x,max.y,min.z),
            };

            DrawLineCube(vertices, color,duration);
        }

        /// <summary>
        /// 
        /// p5          p6 
        ///     p1 p2
        ///     p0 p3
        /// p4          p7
        /// </summary>
        /// <param name="vertices"></param>
        public static void DrawLineCube(Vector3[] vertices, Color color = default, float duration = 0)
        {
            if (vertices.Length < 8)
                return;

            var p0 = vertices[0];
            var p1 = vertices[1];
            var p2 = vertices[2];
            var p3 = vertices[3];
            var p4 = vertices[4];
            var p5 = vertices[5];
            var p6 = vertices[6];
            var p7 = vertices[7];

            DrawLines(new[] {
                (p0,p1),(p1,p2), (p2,p3),(p3,p0), // near
                (p4,p5),(p5,p6),(p6,p7),(p7,p4), // far
                (p0,p4),(p3,p7),
                (p1,p5),(p2,p6),
            }, color,duration);
        }

        public static void DrawLineArrow(Vector3 start,Vector3 dir,Color color = default,float duration=0)
        {
            var perpendicular = Vector3.Cross(Vector3.right, dir).normalized;
            //perpendicular = Quaternion.Euler(45, 0, 0) * perpendicular;
            //perpendicular.y -= 0.5f;

            var end = start + dir;
            var end7 = start + dir * 0.7f;
            var e1 = end7 + perpendicular;
            var e2 = end7 - perpendicular;

            DrawLines(new[] {(start,end),(end,e1),(end,e2)},color, duration);
        }

        public static void DrawAxis(Vector3 pos, Vector3 right, Vector3 up, Vector3 forward, float length = 1)
        {
            Debug.DrawRay(pos, right * length, Color.red);
            Debug.DrawRay(pos, up * length, Color.green);
            Debug.DrawRay(pos, forward * length, Color.blue);
        }

        public static void DrawCircle(Vector3 start,Vector3 end,int segments = 10)
        {
            var halfDir = (end - start) * 0.5f;
            var center = start + halfDir;

            var angleStep = 360f / segments;
            var segStart = center + halfDir;
            var angle = 0f;

            for (int i = 0; i <= segments; i++)
            {
                angle = (i+1) * angleStep;

                var segEnd = Quaternion.Euler(0, angle, 0) * halfDir;
                Debug.DrawLine(segStart, segEnd);
                segStart = segEnd;
            }
        }
    }
}
