using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameUtilsFramework
{
    public static class CameraTools
    {
        public static Vector3 CalcMoveDirection(Transform camTr,Vector2 moveInput)
        {
            var dir = camTr.forward * moveInput.y;
            dir += camTr.right * moveInput.x;
            dir.Normalize();
            return dir;
        }

        public static Transform RaycastTarget(Transform camTr,float maxDistance,LayerMask layer,Predicate<Collider> condition)
        {
            var ray = new Ray(camTr.position, camTr.forward);
            //var isHitted = Physics.Raycast(ray, out var hit, maxDistance, layer);
            var hits = Physics.RaycastAll(ray, maxDistance, layer);
            if (hits.Length == 0)
                return null;

            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (condition(hit.collider))
                    return hit.collider.transform;
            }

            return null;
        }
    }
}
