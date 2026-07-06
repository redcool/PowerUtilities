using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PowerUtilities
{
    /// <summary>
    /// UI Event Tools
    /// </summary>
    public static class EventSystemTools
    {
        static List<RaycastResult> raycastResults = new List<RaycastResult>();

        /// <summary>
        /// Get Gameobjects in screenUV(EventSystem raycastAll), trigger pointerEvent(click,down,up) 
        /// </summary>
        /// <param name="screenUV"></param>
        /// <param name="buttonId"></param>
        public static void ClickScreen(Vector2 screenUV,int buttonId = 0)
        {
            var screenPos = ScreenTools.ScreenSize * screenUV;

            var evData = new PointerEventData(EventSystem.current)
            {
                button = (PointerEventData.InputButton)buttonId,
                position = screenPos,
                pressPosition = screenPos,
            };
            // get items
            EventSystem.current.RaycastAll(evData, raycastResults);
            if (raycastResults.Count == 0)
                return;
            var rootGo = raycastResults[0].gameObject;

            // execute pinter events
            ExecuteEvents.ExecuteHierarchy(rootGo, evData,ExecuteEvents.pointerClickHandler);
            ExecuteEvents.ExecuteHierarchy(rootGo, evData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(rootGo, evData, ExecuteEvents.pointerUpHandler);
        }
    }
}
