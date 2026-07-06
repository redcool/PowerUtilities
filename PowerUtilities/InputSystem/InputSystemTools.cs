#if UNITY_INPUT_SYSTEM
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Utilities;

namespace PowerUtilities
{
    public static class InputSystemTools
    {
        static InputSystemUIInputModule uiModule;

        static IDisposable anyButtonPressCallback;

        public static InputSystemUIInputModule UIModule
        {
            get
            {
                return SingletonTools.Get(ref uiModule, () => EventSystem.current?.GetComponent<InputSystemUIInputModule>());
            }
        }

        /// <summary>
        /// Add InputSystem.onAnyButtonPress
        /// </summary>
        /// <param name="onPress"></param>
        public static void AddAnyButtonPress(Action<InputControl> onPress)
        {
            if (onPress == null)
                return;

            anyButtonPressCallback?.Dispose();

            anyButtonPressCallback = InputSystem.onAnyButtonPress.Call(onPress);
        }

        public static void ShowPressInfo(InputControl x)
        {
            Debug.Log($"Button Pressed: {x.displayName}");

            var pointerId = Mouse.current.deviceId;
            var isPressGo = UIModule.IsPointerOverGameObject(pointerId);
            Debug.Log($"Is Pointer Over GameObject: {isPressGo}");

            if (!isPressGo)
                return;

            var result = UIModule.GetLastRaycastResult(pointerId);
            if (result.isValid)
            {
                Debug.Log($"Raycast hit: {result.gameObject.name},{pointerId}");
            }
        }

        public static void Release()
        {
            anyButtonPressCallback?.Dispose();
        }
    }
}
#endif