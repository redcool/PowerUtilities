namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    public interface IUIElementEvent
    {
        /// <summary>
        /// register root's component event
        /// </summary>
        /// <param name="root"></param>
        public void AddEvent(VisualElement root);
    }
}