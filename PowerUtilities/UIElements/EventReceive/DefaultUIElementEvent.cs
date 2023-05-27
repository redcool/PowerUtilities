namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class DefaultUIElementEvent : IUIElementEvent
    {
        public void AddEvent(VisualElement root)
        {
            Debug.Log("addEvent "+root);
            if (root == null)
                return;

            root.Query<Toggle>().Build()
                .ForEach(toggle => toggle.RegisterValueChangedCallback(OnValueChanged));


            root.Query<Button>().Build()
                .ForEach(bt => bt.RegisterCallback<ClickEvent>(OnButtonClicked));
        }

        public void OnValueChanged(ChangeEvent<bool> ev)
        {
            Debug.Log($"OnValueChanged {ev.target},lase: {ev.previousValue} ,new :{ev.newValue}");
        }

        public void OnButtonClicked(ClickEvent ev)
        {
            Debug.Log("clicked "+ev.target);
        }
    }
}