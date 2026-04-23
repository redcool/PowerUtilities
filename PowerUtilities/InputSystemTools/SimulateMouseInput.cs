#if UNITY_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class SimulateMouseInput
{
    public static void TriggerMouseButton(int buttonId, bool isPressed)
    {
        var mouse = Mouse.current;
        using (StateEvent.From(mouse, out var newEventPtr))
        {
            mouse.leftButton.WriteValueIntoEvent((buttonId == 0 && isPressed) ? 1f : 0, newEventPtr);
            mouse.rightButton.WriteValueIntoEvent((buttonId == 1 && isPressed) ? 1f : 0, newEventPtr);
            mouse.middleButton.WriteValueIntoEvent((buttonId == 2 && isPressed) ? 1f : 0, newEventPtr);
            InputSystem.QueueEvent(newEventPtr);
        }
    }

    public static void MoveMouse(Vector2 screenPos)
    {
        var mouse = Mouse.current;
        using (StateEvent.From(mouse, out var eventPtr))
        {
            mouse.position.WriteValueIntoEvent(screenPos, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }
}
#endif