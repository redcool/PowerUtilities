namespace PowerUtilities
{

    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

#if UNITY_EDITOR
    using UnityEditor;
    using System.Reflection;
    using Object = UnityEngine.Object;

    [CustomEditor(typeof(UIDocumentEventRegister))]
    public class UIDocumentEventRegisterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var inst = target as UIDocumentEventRegister;
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if(EditorGUI.EndChangeCheck())
            {
                inst.monos
                    .ForEach((mono, id) =>
                    {
                        if (mono && !mono.GetClass().IsImplementOf(typeof(IUIElementEvent)))
                            inst.monos[id] = null;
                    });
            }
            if (inst.monos != null)
            {
                inst.eventTypeQNames = inst.monos
                    .Select(mono => mono ? mono.GetClass().AssemblyQualifiedName : default)
                    .ToList();

            }

            serializedObject.ApplyModifiedProperties();
        }

        
    }

#endif

    public class UIDocumentEventRegister : MonoBehaviour
    {
        public List<UIDocument> docs = new List<UIDocument>();        
#if UNITY_EDITOR
        public List<MonoScript> monos = new List<MonoScript>();
#endif
        public List<string> eventTypeQNames= new List<string>();
        List<IUIElementEvent> eventInstances;


        void OnEnable()
        {
            if(docs.Count == 0)
            {
                docs[0] = GetComponent<UIDocument>();
            }
            if (eventTypeQNames == null || eventTypeQNames.Count != docs.Count)
            {
                Debug.Log("eventTypes is none");
                return;
            }

            eventInstances = eventTypeQNames
                .Select(typeQName => Activator.CreateInstance(Type.GetType(typeQName)) as IUIElementEvent)
                .ToList();

            eventInstances.ForEach((eventInst, id) => eventInst?.AddEvent(docs[id]?.rootVisualElement));
        }

        private void OnDisable()
        {
            eventInstances = null;
        }

        public void AddEventQName(string typeQName)
        {
            eventTypeQNames.Add(typeQName);
        }
    }

}