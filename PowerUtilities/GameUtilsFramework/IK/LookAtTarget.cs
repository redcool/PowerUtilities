using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameUtilsFramework
{
    public class LookAtTarget : MonoBehaviour
    {
        public Vector3 upwards = Vector3.up;
        public float rotSpeed = 10;

        [Tooltip("dot, > 0 is positive dir, 0 is perpendicular, < 0 is negative")]
        [Range(0,1)]public float dirDotLimit = 0.4f;

        [Header("Trs")]
        public Transform headTr;
        public Transform bodyTr;
        public Transform targetTr;

        Quaternion lastRot;

        // Start is called before the first frame update
        void Start()
        {

            if(!headTr)
                headTr = transform;

            if(!bodyTr)
                bodyTr = transform.root;

            lastRot = headTr.rotation;

            if(!targetTr || !bodyTr)
            {
                enabled=false;
                return;
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            var headDir = targetTr.position - headTr.position;

            Debug.DrawRay(headTr.position, headDir);
            Debug.DrawRay(headTr.position, bodyTr.forward);

            var dirDot = Vector3.Dot(headDir, bodyTr.forward);
            var targetRot = Quaternion.LookRotation(headDir,upwards) * lastRot;
            targetRot = dirDot > dirDotLimit ? targetRot : lastRot;
            
            headTr.rotation = Quaternion.Slerp(headTr.rotation, targetRot ,rotSpeed*Time.deltaTime);
        }
    }
}
