using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameUtilsFramework
{
    public class CameraShakeControl : MonoBehaviour
    {
        public float time = 1f;
        public float distance = 1;
        public AnimationCurve smoothDampingCurve = new AnimationCurve(new Keyframe(0, 0, 0.1f, 0.1f), new Keyframe(1, 1, .1f, .1f));

        public bool autoPlay = true;

        public Transform target;

        float startTime;
        Vector3 startPos;
        // Start is called before the first frame update
        void Start()
        {
            if (!target)
                target = GetComponent<Camera>()?.transform;

            if (autoPlay)
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
            if (autoPlay)
                Shake();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (!target || Time.time - startTime > time)
            {
                enabled = false;
                target.position = startPos;
                return;
            }

            var rate = Mathf.Clamp01((Time.time - startTime)/time);
            rate = 1- rate;


            // get smooth damping
            var smoothTime = 0.02f;
            if (smoothDampingCurve.length >0) {
                smoothTime = 0.02f * smoothDampingCurve.Evaluate(rate*smoothDampingCurve[smoothDampingCurve.length-1].time);
            }

            // update position
            var randPos = Random.insideUnitSphere * distance * rate;
            Vector3 currentVelocity = Vector3.zero;
            var nextPos = Vector3.SmoothDamp(startPos, startPos + randPos, ref currentVelocity, smoothTime);

            target.position = nextPos;

        }

        public void Shake()
        {
            startTime = Time.time;
            startPos = target.position;
            enabled = true;
        }
    }
}
