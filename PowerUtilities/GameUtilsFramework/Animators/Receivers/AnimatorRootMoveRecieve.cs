using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace BoxSouls
{
    public class AnimatorRootMoveRecieve : MonoBehaviour
    {
        public UnityEvent<Vector3> onAnimatorMoved;
        public Action<Vector3> onAnimatorMoved1;

        [Header("Debug Info")]
        public Animator anim;
        public Vector3 velocity;
        private void Start()
        {
            anim = GetComponent<Animator>();
            Assert.IsNotNull(anim);

        }

        public void OnAnimatorMove()
        {
            var pos = anim.deltaPosition;
            velocity = pos / Time.deltaTime;

            //playerControl.playerLocomotion.rootMotionVelocity = velocity;
            onAnimatorMoved1?.Invoke(velocity);

            onAnimatorMoved.Invoke(velocity);
        }

    }
}
