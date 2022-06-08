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
    }
}
