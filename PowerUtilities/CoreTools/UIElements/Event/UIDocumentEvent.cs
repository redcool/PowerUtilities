using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIDocumentEvent : MonoBehaviour,IUIElementEvent
{
    public UIDocument doc;

    public void AddEvent(VisualElement root)
    {
        //root.Q<Toggle>().RegisterValueChangedCallback(TestToggle);
        root.Query<VisualElement>().Build().ForEach(e => Debug.Log(e));
    }

    private void TestToggle(ChangeEvent<bool> evt)
    {
        Debug.Log(evt.target + ":" + evt.previousValue + evt.newValue);
    }

    // Start is called before the first frame update
    void Start()
    {
        doc = GetComponent<UIDocument>();
        AddEvent(doc.rootVisualElement);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
