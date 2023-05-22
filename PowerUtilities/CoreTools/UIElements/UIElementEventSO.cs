namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CreateAssetMenu(menuName ="PowerUtilities/UIElements/"+nameof(UIElementEventSO))]
    public class UIElementEventSO : ScriptableObject ,IUIElementEvent
    {
        public void AddEvent(VisualElement root)
        {
            if (root == null)
                return;

            root.Q<Toggle>().RegisterValueChangedCallback(TestToggle);

            root.Q<Button>().clicked += () => {
                Debug.Log("test click");
            };
        }

        void TestToggle(ChangeEvent<bool> ce)
        {
            Debug.Log(ce.target+":"+ce.previousValue +":"+ce.newValue);
        }
    }
}