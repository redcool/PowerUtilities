#if UNITY_INPUT_SYSTEM
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Utilities;

namespace PowerUtilities
{

    public class TestMouseButtonClick : MonoBehaviour
    {
        public bool isAddAnyButtonPress;
        public bool isTestMouseRightButton;
        public void OnEnable()
        {
            if (isAddAnyButtonPress)
                InputSystemTools.AddAnyButtonPress(InputSystemTools.ShowPressInfo);
        }

        void Update()
        {
            if (isTestMouseRightButton && Mouse.current.rightButton.wasPressedThisFrame)
            {
                MouseDeviceTools.ClickMouse(new Vector2(0.5f, 0.5f), 0);
            }
        }

        public void OnDisable()
        {
            MouseDeviceTools.Release();
            InputSystemTools.Release();
        }

    }
}
#endif