using UnityEngine;
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
        /// 
        /// p4          p5 
        ///     p1 p2
        ///     p0 p3
        /// p6          p7
        /// </summary>
        /// <param name="vertices"></param>
        public static void DrawLineCube(Vector3[] vertices, Color color = default)
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
            }, color);
        }


    }
}
