using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PowerUtilities
{
    [ExecuteAlways]
    public class RectTransformConstraintSort : MonoBehaviour
    {
        [EditorButton(onClickCall = "SortChildren")]
        public bool isSortChildren;
        public bool isUpdateMode = true;
        public RectTransformConstraint[] constraintChildren;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (isUpdateMode)
                SortChildren();
        }


        public void SortChildren()
        {
            if (constraintChildren == null || constraintChildren.Length == 0)
            {
                constraintChildren = gameObject.GetComponentsInChildren<RectTransformConstraint>();
            }

            Array.Sort(constraintChildren, (a, b) => b.GetDistanceToCam() - a.GetDistanceToCam());

            for (int i = 0; i < constraintChildren.Length; i++)
            {
                constraintChildren[i].transform.SetSiblingIndex(i);
            }
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < constraintChildren.Length; i++)
            {
                Gizmos.color = Color.blue * ((float)(i+i)/constraintChildren.Length);
                Gizmos.DrawLine(constraintChildren[i].positionLock.target.position, constraintChildren[i].cam.transform.position);
            }
        }
    }
}
