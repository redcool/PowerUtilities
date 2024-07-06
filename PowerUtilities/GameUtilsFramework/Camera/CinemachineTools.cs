namespace GameUtilsFramework
{
#if UNITY_6000_0_OR_NEWER
    using Unity.Cinemachine;
#else
    using Cinemachine;
#endif

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if INPUT_SYSTEM_ENABLED
    using UnityEngine.InputSystem;
#endif

    public static class CinemachineTools
    {

#if INPUT_SYSTEM_ENABLED

        /// <summary>
        /// reset cinemachine Input
        /// </summary>
        /// <param name="axisInput"></param>
        public static void SetupCinemachineInput(InputAction axisInput)
        {
            CinemachineCore.GetInputAxis = (axisName) =>
            {
                var dir = axisInput.ReadValue<Vector2>();
                if (axisName == "Mouse X")
                    return dir.x;
                else if (axisName == "Mouse Y")
                    return dir.y;
                return 0;
            };
        }

#endif
    }
}