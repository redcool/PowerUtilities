using GameUtilsFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerManager : MonoBehaviour
{
    CharacterController cc;
    InputControl inputControl;

    public bool updateRotation = false;

    public bool isGrounded;
    public float collidePosYOffset=0.5f;
    public float collideRadius=0.51f;
    public float gravity = -9.8f;
    public LayerMask groundLayer=1;

    public float jumpHeight = 5;
    public bool isJump;

    [SerializeField]Vector3 gravityVelocity;
    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        inputControl = GetComponent<InputControl>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputControl.isJump)
        {
            isJump = true;
            inputControl.isJump = false;
        }
        var movement = inputControl.movement * 4;
        var moveDir = new Vector3(movement.x, 0, movement.y);

        MoveController(ref moveDir);
    }

    public void MoveController(ref Vector3 moveDir)
    {
        //isGrounded = cc.isGrounded;
        //---------- collide
        var origin = transform.position;
        origin.y += collidePosYOffset;
        isGrounded = Physics.CheckSphere(origin, collideRadius, groundLayer); ;
        //if (Physics.SphereCast(origin, collideRadius, Vector3.down, out var hit, collideDistance, groundLayer))

        if (isGrounded)
        {
            if (gravityVelocity.y < 0)
            {
                gravityVelocity.y = 0;
            }
            MovementTools.AdjustVelocityToSlope(transform, ref moveDir, 1, groundLayer);
        }


        cc.Move(moveDir * Time.deltaTime);

        if (updateRotation && moveDir != Vector3.zero)
        {
            var forward = moveDir;
            forward.y = 0;
            transform.forward = Vector3.Slerp(transform.forward, forward, Time.deltaTime * 10);
        }

        if (isJump)
        {
            if (isGrounded)
                gravityVelocity.y += jumpHeight;

            isJump = false;
        }
        gravityVelocity.y += gravity * Time.deltaTime;
        cc.Move(gravityVelocity * Time.deltaTime);
    }
}
