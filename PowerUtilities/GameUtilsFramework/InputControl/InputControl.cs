namespace GameUtilsFramework
{
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if INPUT_SYSTEM_ENABLED
    using UnityEngine.InputSystem;
#else
    public class InputValue
    {

        ////TODO: add automatic conversions
        public TValue Get<TValue>()
            where TValue : struct
        {
            return default(TValue);
        }

        ////TODO: proper message if value type isn't right
        public bool isPressed => false;

    }
#endif


    public class InputControl : MonoBehaviour
    {
        public Vector2 movement;
        public Vector2 look;
        public bool isSprint,isRolling,isJump,tryLock,isActionHolding,
            LB, LT, RB, RT
            ;

        public float MovementLength =>  movement.magnitude;
        public void OnMovement(InputValue v) => movement = v.Get<Vector2>();

        public void OnLook(InputValue v) => look = v.Get<Vector2>(); 

        public void OnSprint(InputValue v) => isSprint = v.isPressed;

        public void OnRoll(InputValue v) => isRolling = true;
        public void OnJump(InputValue v) => isJump = v.isPressed;

        public void OnLB(InputValue v) => LB = v.isPressed;
        public void OnLT(InputValue v) => LT = v.isPressed;
        public void OnRB(InputValue v) => RB = v.isPressed;
        public void OnRT(InputValue v) => RT = v.isPressed;
        public void OnTryLock(InputValue v) => tryLock = v.isPressed;

        public void OnActionHolding(InputValue v)=> isActionHolding = v.isPressed;


        public bool IsButtonTriggeredAndReset(ref bool buttonId)
        {
            var t = buttonId;
            buttonId = false;
            return t;
        }

        public bool IsLeftHandAttack() => !isActionHolding && RT;
        public bool IsRightHandAttack() => !isActionHolding && RB;
        public void ResetLeftHandAttack() => RT = false;
        public void ResetRightHandAttack() => RB = false;

        public bool IsHoldLeftWeapon() => isActionHolding && RT;
        public bool IsHoldRightWeapon() => isActionHolding && RB;
    }

}