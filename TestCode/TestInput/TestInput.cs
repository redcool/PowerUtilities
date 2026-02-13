using Newtonsoft.Json;
using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class TestInput : MonoBehaviour
{
    public InputActionAsset inputActions;

    public InputActionMap actionMap;
    public InputActionReference inputActionRef;

    [EditorButton(onClickCall = "OnTest")]
    public bool isTest;

    public PlayerInput playerInput;

    private void OnEnable()
    {
        inputActions.Enable();

        actionMap = inputActions.FindActionMap("GameInput");
        actionMap["Fire"].started += OnFire;
        actionMap["Fire"].performed += OnFire;
        actionMap["Fire"].canceled += OnFire;

        playerInput = GetComponent<PlayerInput>();
        Debug.Log(playerInput.currentControlScheme);

        var mapAct = actionMap["Fire"];
        mapAct.Disable();
        mapAct.PerformInteractiveRebinding()
            .WithControlsExcluding("<Mouse>/LeftButton")
            .OnComplete(operation =>
        {
            Debug.Log($"done, {mapAct.bindings[0].effectivePath}");
            operation.Dispose();
            mapAct.Enable();
        }).Start();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    public void OnTest()
    {
    }

    public void OnMovement(InputValue value)
    {
        Debug.Log(value.Get<Vector2>());
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if(context.started)
            Debug.Log("start fire");
        if(context.performed)
            Debug.Log("onfire performed");
        if(context.canceled)
            Debug.Log("canceled fire");
    }
}


