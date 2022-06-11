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

        [Header("Collide")]
        [Tooltip("raycast from rigid.position + collidePosOffset")]
        public float collidePosOffset = 0.5f;
        [Min(0.1f)] public float collideRadius = 0.45f; // should less than collidePosOffset
        public float collideDistance = 0.1f;
        public Vector3 collideDir = Vector3.down;


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
            isGrounded = Physics.SphereCast(origin, collideRadius, collideDir, out var hit, collideDistance, groundLayer);

            Debug.DrawRay(origin, collideDir * collideDistance, Color.green);

            if (isGrounded)
            {
                if (Vector3.Dot(hit.normal, Vector3.up) < projectionOnPlaneRate)
                    moveDir = Vector3.ProjectOnPlane(moveDir, hit.normal);

                if (vecY < 0)
                {
                    vecY = 0;
                }

                inAirTime = 0;
            }

            if (isJump)
            {
                isJump = false;

                vecY = jumpHeight;
                moveDir.y = vecY;
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
