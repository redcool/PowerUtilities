namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CreateAssetMenu(menuName ="PowerUtilities/SOs/"+nameof(BaseUIElementEventSO))]
    public class BaseUIElementEventSO : ScriptableObject
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