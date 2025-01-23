using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameUtilsFramework
{
    public class RigidbodyCharacterController : MonoBehaviour
    {
        public RigidbodyCharater rigidChar = new RigidbodyCharater();
        public BaseInputControl inputControl;
        public float moveSpeed = 4;
        private void Start()
        {
            rigidChar.rigid = GetComponent<Rigidbody>();
            rigidChar.InitRigid();
            inputControl = inputControl ?? GetComponent<BaseInputControl>();

            rigidChar.rigid.freezeRotation = true;
        }

        private void FixedUpdate()
        {
            var movement = inputControl.movement;
            var moveDir = new Vector3(movement.x, 0, movement.y) * moveSpeed;
            rigidChar.MoveRigidbody(ref moveDir);
            Debug.DrawRay(transform.position, moveDir, Color.blue);
        }
    }
}
