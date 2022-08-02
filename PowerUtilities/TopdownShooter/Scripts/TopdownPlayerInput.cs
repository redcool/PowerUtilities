namespace TopdownShooter
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class TopdownPlayerInput : MonoBehaviour
    {
        public Vector2 movement;
        public Vector2 look;
        public bool fire;

        public bool lockCursor = true;

        // Start is called before the first frame update
        void Start()
        {

        }

        void OnMovement(InputValue value)
        {
            movement = value.Get<Vector2>();
        }

        void OnLook(InputValue value)
        {
            look = value.Get<Vector2>();
        }

        void OnFire(InputValue v)
        {
            fire = v.isPressed;
        }

        private void OnApplicationFocus(bool focus)
        {
            Cursor.SetCursor(null, Vector2.one, CursorMode.Auto);
            //Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        }

    }
}