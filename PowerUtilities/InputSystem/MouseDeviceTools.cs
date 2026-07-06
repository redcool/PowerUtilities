#if UNITY_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace PowerUtilities
{

    public static class MouseDeviceTools
    {
        static Mouse virtualMouse;

        /// <summary>
        /// Get virtual mouse Device from InputSystem
        /// </summary>
        public static Mouse VirtualMouse
        {
            get
            {
                if (virtualMouse == null)
                {
                    var lastIndex = InputSystem.devices.IndexOf(device => device.name == nameof(virtualMouse));
                    if (lastIndex >= 0)
                    {
                        var lastDevice = InputSystem.devices[lastIndex];
                        InputSystem.RemoveDevice(lastDevice);
                    }
                    virtualMouse = InputSystem.AddDevice<Mouse>(nameof(virtualMouse));

                }
                return virtualMouse;
            }
        }
        /// <summary>
        /// Release virtual mouse device
        /// </summary>
        public static void Release()
        {
            if (virtualMouse != null)
                InputSystem.RemoveDevice(virtualMouse);
        }

        /// <summary>
        /// Trigger Click event with virtualMouse
        /// </summary>
        /// <param name="screenUV"></param>
        /// <param name="buttonId"></param>
        public static void Click(Vector2 screenUV, int buttonId = 0)
        {
            if (VirtualMouse == null)
                return;

            var screenPos = ScreenTools.ScreenSize * screenUV;
            var buttonsMask = 1 << buttonId;
            var mouseState = new MouseState
            {
                buttons = (ushort)buttonsMask,
                position = screenPos,
                clickCount = 1,
            };
            //mouseState.WithButton((MouseButton)buttonId);

            InputSystem.QueueStateEvent(VirtualMouse, mouseState);
            InputSystem.Update();

            mouseState.buttons = 0;
            InputSystem.QueueStateEvent(VirtualMouse, mouseState);
            InputSystem.Update();
        }

    }
}
#endif