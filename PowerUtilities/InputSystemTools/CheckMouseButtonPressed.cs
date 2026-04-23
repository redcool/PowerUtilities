#if UNITY_INPUT_SYSTEM
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Utilities;

public class CheckMouseButtonPressed : MonoBehaviour
{
    IDisposable anyButtonPress;
    InputSystemUIInputModule uiModule;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }
    public void OnEnable()
    {
        //InputSystem.onEvent+= (eventPtr,device) => {
        //    if (device == Mouse.current)
        //    {
        //        var mouseEvent = StateEvent.From(eventPtr);
        //        if (mouseEvent != null)
        //        {
        //            //Debug.Log($"Input Event: {eventPtr},{device}, middle simulate left ");
        //            Mouse.current.leftButton.WriteValueIntoEvent(Mouse.current.middleButton.isPressed ? 1f : 0, eventPtr);
        //        }
        //    }
        //};

        if (!uiModule)
        {
            uiModule = EventSystem.current?.GetComponent<InputSystemUIInputModule>();
        }

        anyButtonPress = InputSystem.onAnyButtonPress.Call(x => {
            Debug.Log($"Button Pressed: {x.displayName}");

            var pointerId= Mouse.current.deviceId;
            var isPressGo = uiModule.IsPointerOverGameObject(pointerId);
            Debug.Log($"Is Pointer Over GameObject: {isPressGo}");

            if (!isPressGo)
                return;

            var result = uiModule.GetLastRaycastResult(pointerId);
            if (result.isValid)
            {
                Debug.Log($"Raycast hit: {result.gameObject.name},{pointerId}");
            }
        });

    }

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
            SimulateMouseInput.TriggerMouseButton(0, true);
    }



    public void OnDisable()
    {
        anyButtonPress.Dispose();
    }

}
#endif