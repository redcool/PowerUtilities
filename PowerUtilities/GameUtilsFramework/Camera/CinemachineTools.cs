#if INPUT_SYSTEM_ENABLED && CINEMACHINE_ENABLED
namespace GameUtilsFramework
{
    using Cinemachine;

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    using UnityEngine.InputSystem;

    public static class CinemachineTools
    {
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

    }
}
#endif