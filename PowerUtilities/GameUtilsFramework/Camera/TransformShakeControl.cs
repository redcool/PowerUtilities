namespace GameUtilsFramework
{
    using PowerUtilities;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(TransformShakeControl))]
    public class TransformShakeControlEditor : PowerEditor<TransformShakeControl>
    {
        public override bool NeedDrawDefaultUI()
        => true;
        public override void DrawInspectorUI(TransformShakeControl inst)
        {
            if (GUILayout.Button("Play"))
            {
                inst.Shake();
            }
        }
    }
#endif

    public class TransformShakeControl : MonoBehaviour
    {
        //=============Options
        [Header("Options")]
        [Tooltip("elapsed time")]
        [Min(0.0001f)]
        public float time = 1f;

        [Tooltip("sphere radius")]
        public float distance = 1;

        [Tooltip("smooth damping, more big more slow")]
        public AnimationCurve smoothDampingCurve = new AnimationCurve(new Keyframe(0, 0.5f, 0.1f, 0.1f), new Keyframe(1, 1, .1f, .1f));

        //=============Atten
        [Header("Atten")]
        [Tooltip("auto play when start")]
        public bool isAutoPlay = true;

        [Tooltip("destroy when play done")]
        public bool isPlayOnce = false;

        [Tooltip("Get random postion per frame")]
        public bool isUpdatePerFrame = true;

        [Tooltip("which axis can random move")]
        public Vector3 axisAtten = Vector3.one;

        //=============Target
        [Header("Target")]
        [Tooltip("target, use current transform when empty")]
        public Transform target;

        float startTime;
        Vector3 startLocalPos;
        Vector3 nextPos, firstNextPos;

        Quaternion startLocalRot;

        // Start is called before the first frame update
        void Start()
        {
            if (!target)
                target = transform;

            startLocalPos = target.localPosition;
            startLocalRot = target.localRotation;

            if (isAutoPlay)
            {
                Shake();
            }
            else
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (isAutoPlay)
                Shake();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (!target || Time.time - startTime >= time)
            {
                enabled = false;
                target.localPosition = startLocalPos;
                target.localRotation = startLocalRot;

                if (isPlayOnce)
                    Destroy(this);

                return;
            }

            // attenuations
            float rate = CalcRate();

            // get smooth damping
            float smoothTime = CalcSmoothDamp(rate);

            // update position
            CalcNextPos(rate, smoothTime);

            target.localPosition = nextPos;

            //target.localRotation = Quaternion.Lerp(Random.rotationUniform,startLocalRot, smoothTime);
        }

        private void CalcNextPos(float rate, float smoothTime)
        {
            if (isUpdatePerFrame)
                nextPos = GetRandomPos(startLocalPos, distance, rate, smoothTime);
            else
            {
                nextPos = Vector3.Lerp(startLocalPos, firstNextPos, rate);
            }
            nextPos.Set(
                Mathf.Lerp(startLocalPos.x, nextPos.x, axisAtten.x),
                Mathf.Lerp(startLocalPos.y, nextPos.y, axisAtten.y),
                Mathf.Lerp(startLocalPos.z, nextPos.z, axisAtten.z)
                );
        }

        private float CalcSmoothDamp(float rate)
        {
            var smoothTime = 0.02f;
            if (smoothDampingCurve.length > 0)
            {
                smoothTime = 0.02f * smoothDampingCurve.Evaluate(rate * smoothDampingCurve[smoothDampingCurve.length - 1].time);
            }

            return smoothTime;
        }

        private float CalcRate()
        {
            var rate01 = Mathf.Clamp01((Time.time - startTime) / time);
            var rate = 1 - rate01;// 1-0
            return rate;
        }

        public static Vector3 GetRandomPos(Vector3 startLocalPos, float distance, float rate, float smoothTime)
        {
            var randPos = Random.insideUnitSphere * distance * rate;
            Vector3 currentVelocity = Vector3.zero;
            var nextPos = Vector3.SmoothDamp(startLocalPos, startLocalPos + randPos, ref currentVelocity, smoothTime);
            return nextPos;
        }

        public void Shake()
        {
            if (!target)
                return;

            startTime = Time.time;
            enabled = true;
            firstNextPos = GetRandomPos(startLocalPos, distance, 1, 0.02f);
        }
    }
}
