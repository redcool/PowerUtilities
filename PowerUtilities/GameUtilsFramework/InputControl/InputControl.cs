using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputControl : MonoBehaviour
{
    public Vector2 movement;
    public Vector2 look;
    public bool isSprint,
        LB, LT, RB, RT
        ;

    public float movementLength;
    public void OnMovement(InputValue v)
    {
        movement = v.Get<Vector2>();
        movementLength = movement.magnitude;
    }

    public void OnLook(InputValue v)
    {
        look = v.Get<Vector2>();
    }

    public void OnSprint(InputValue v)
    {
        isSprint = v.isPressed;
    }

    public void OnLB(InputValue v) => LB = v.isPressed;
    public void OnLT(InputValue v) => LT = v.isPressed;
    public void OnRB(InputValue v) => RB = v.isPressed;
    public void OnRT(InputValue v) => RT = v.isPressed;

}
