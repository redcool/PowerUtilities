using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPlayerControl : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 10f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public float rotationSpeed = 10;

    [Header("Cine")]
    public CinemachineFreeLook cinemachineFreeLook;
    [Header("Camera")]
    public bool isUseCameraOrbit = true;
    public Transform cameraTransform;
    public float lookSensitivity = .2f;
    public float cameraOrbitSpeed = 3;

    CharacterController controller;
    ThirdPlayerControlInputActions inputAction;
    Vector2 moveInput;
    Vector2 lookInput;
    Vector3 velocity;
    bool isSprinting;

    public Vector3 cameraOffset = new Vector3(0,10,-10); // camera orbit offset from player
    public float cameraYaw;
    public float cameraPitch;
    // Start is called before the first frame update
    void Awake()
    {
        controller = GetComponent<CharacterController>();

        inputAction = new ThirdPlayerControlInputActions();

        inputAction.GameInput.Jump.performed += ctx => Jump();

        inputAction.GameInput.Sprint.started += ctx => isSprinting = true;
        inputAction.GameInput.Sprint.canceled += ctx => isSprinting = false;
    }
    private void Jump()
    {
        if(controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    private void OnEnable()
    {
        inputAction.Enable();
    }
    private void OnDisable() => inputAction.Disable();

    // Update is called once per frame
    void Update()
    {
        moveInput = inputAction.GameInput.Movement.ReadValue<Vector2>();
        lookInput = inputAction.GameInput.Look.ReadValue<Vector2>();

        UpdateMovement();
    }
    private void LateUpdate()
    {
        if (isUseCameraOrbit)
            OrbitCamera(ref cameraPitch, ref cameraYaw);
    }

    private void UpdateMovement()
    {
        if(controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float3 moveDir = cameraTransform.right * moveInput.x + cameraTransform.forward * moveInput.y;
        moveDir.y = 0;
        moveDir = math.normalize(moveDir);

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        controller.Move(moveDir * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (moveInput.sqrMagnitude > 0.01f)
        {
            var targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = math.slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime); ;
        }
    }


    void OrbitCamera(ref float cameraPitch,ref float cameraYaw)
    {
        float2 orbit = lookInput * lookSensitivity * Time.deltaTime * cameraOrbitSpeed;
        cameraYaw += orbit.x;
        cameraPitch += orbit.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        var q = quaternion.Euler(cameraPitch, cameraYaw, 0);
        Vector3 targetOffsetDir = math.mul(q,cameraOffset);

        cameraTransform.position = transform.position + targetOffsetDir;
        cameraTransform.forward = math.normalize(-targetOffsetDir);
    }
}
