using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameUtilsFramework
{
    /// <summary>
    /// Rigidbody Character control
    /// </summary>
    [Serializable]
    public class RigidbodyCharater
    {
        public Rigidbody rigid;

        [Header("Ground")]
        public LayerMask groundLayer = 1;

        [Tooltip("less than will do projection on collide plane")]
        [Range(0.5f,1f)]public float projectionOnPlaneRate = 0.9f;

        [Tooltip("less than will decide is on ground")]
        [Range(0.02f, 1)] public float groundClearance = 0.1f;

        [Header("Collide")]
        [Tooltip("raycast from rigid.position + collidePosOffset")]
        public float collidePosOffset = 0.5f;

        [Tooltip("sphere radius to check,great than collideMinDistance will do sphere Check")]
        [Min(0.1f)] public float sphereCheckRadius = 0.5f;

        [Header("Sweep Ray")]
        public Vector3 rayDir = Vector3.down;
        public float rayLength = 0.5f;
        //public float rayRadius = 0.2f;

        [Header("Gravity")]
        public float gravity = -18;

        [Header("Jump")]
        public float jumpHeight = 10;
        public bool isJump;

        [Header("RootMotion Velocity")]
        public Vector3 rootMotionVelocity;

        [Header("States")]
        public bool isGrounded;
        public float inAirTime;

        float vecY;

        /// <summary>
        /// call MoveRigidbody in FixedUpdte
        /// </summary>
        /// <param name="movementInput"></param>
        public void MoveRigidbody(ref Vector3 moveDir)
        {
            Assert.IsNotNull(rigid);

            isGrounded = false;

            //moveDir.y = rigid.velocity.y;
            moveDir.y = vecY;

            // apply root motion
            moveDir += rootMotionVelocity;

            var origin = rigid.position;
            origin.y += collidePosOffset;


            // raycast downward
            if (Physics.Raycast(origin, rayDir, out var hit, rayLength, groundLayer))
            {
                //add a little up offset
                if (hit.distance <= Mathf.Abs(collidePosOffset) + groundClearance)
                {
                    isGrounded = true;
                }
                else
                {
                    isGrounded = Physics.CheckSphere(origin, sphereCheckRadius, groundLayer);
                }

            }

            Debug.DrawRay(origin, rayDir * rayLength, Color.green);
            //if (Physics.SphereCast(origin, rayRadius, rayDir, out var hit, rayLength,groundLayer))
            //{
            //    isGrounded = true;
            //}

            if (isGrounded)
            {
                if (Vector3.Dot(hit.normal, Vector3.up) < projectionOnPlaneRate)
                    moveDir = Vector3.ProjectOnPlane(moveDir, hit.normal);

                if (isJump)
                {
                    isJump = false;

                    vecY = jumpHeight ;
                    moveDir.y = vecY;
                }

                if (vecY < 0)
                {
                    vecY = 0;
                }

                inAirTime = 0;
            }

            if (!isGrounded)
            {
                vecY += gravity * Time.fixedDeltaTime;
                inAirTime += Time.fixedDeltaTime;
            }


            rigid.velocity = moveDir;
        }
    }
}
