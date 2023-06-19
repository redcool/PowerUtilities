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
        public RigidbodyCharater c = new RigidbodyCharater();
        public BaseInputControl inputControl;
        public float moveSpeed = 4;
        private void Start()
        {
            c.rigid = GetComponent<Rigidbody>();
            inputControl = inputControl ?? GetComponent<BaseInputControl>();
        }

        private void FixedUpdate()
        {
            var movement = inputControl.movement;
            var moveDir = new Vector3(movement.x, 0, movement.y) * moveSpeed;
            c.MoveRigidbody(ref moveDir);
            //Debug.DrawRay(transform.position, moveDir, Color.blue);
        }
    }
}
