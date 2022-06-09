using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameUtilsFramework
{
    public static class MovementTools
    {

        public static bool AdjustVelocityToSlope(Transform playerTr,ref Vector3 velocity,float distance,LayerMask layer)
        {
            if(Physics.Raycast(playerTr.position+Vector3.up*0.1f,Vector3.down,out var hit,distance,layer))
            {
                if (hit.normal != Vector3.up)
                {
                    velocity = Vector3.ProjectOnPlane(velocity, hit.normal);
                    return true;
                }
            }
            return false;
        }
    }
}
