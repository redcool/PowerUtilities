using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{

    [ExecuteAlways]
    public class TestBezierCurve : MonoBehaviour
    {
        [Range(0, 1)] public float value;
        public Transform tr0, tr1, tr2, tr3;
        public Transform target;
        public float[] weights = new[]{
            1,1,1,1f
        };

        public int segments = 100;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            ShowBezier3();
            DrawCurve3();
        }

        private void ShowBezier2()
        {
            var pos = BezierCurve.Bezier(tr0.position, tr1.position, tr2.position, value, weights[0], weights[1], weights[2]);
            target.position = pos;
        }
        private void DrawCurve2()
        {
            var rate = 1f / segments;
            var t = 0f;
            for (int i = 0; i < segments; i++)
            {
                var pos0 = BezierCurve.Bezier(tr0.position, tr1.position, tr2.position, t, weights[0], weights[1], weights[2]);
                t += rate;
                var pos1 = BezierCurve.Bezier(tr0.position, tr1.position, tr2.position, t, weights[0], weights[1], weights[2]);

                Debug.DrawLine(pos0, pos1);
            }
        }

        private void ShowBezier3()
        {
            var pos = BezierCurve.Bezier(tr0.position, tr1.position, tr2.position, tr3.position, value, weights[0], weights[1], weights[2], weights[3]);
            target.position = pos;
        }

        private void DrawCurve3()
        {
            var rate = 1f / segments;
            var t = 0f;
            for (int i = 0; i < segments; i++)
            {
                var pos0 = BezierCurve.Bezier(tr0.position, tr1.position, tr2.position, tr3.position, t, weights[0], weights[1], weights[2], weights[3]);
                t += rate;
                var pos1 = BezierCurve.Bezier(tr0.position, tr1.position, tr2.position, tr3.position, t, weights[0], weights[1], weights[2], weights[3]);

                Debug.DrawLine(pos0, pos1);
            }
        }
    }
}
