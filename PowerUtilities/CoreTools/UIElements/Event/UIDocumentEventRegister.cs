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
                    .ToArray();

            }

            serializedObject.ApplyModifiedProperties();
        }

        
    }

#endif

    public class UIDocumentEventRegister : MonoBehaviour
    {
        public UIDocument[] docs;
        public string[] eventTypeQNames;
#if UNITY_EDITOR
        public MonoScript[] monos;
#endif
        IUIElementEvent[] eventInstances;


        void OnEnable()
        {
            if(docs.Length == 0)
            {
                docs[0] = GetComponent<UIDocument>();
            }
            if (eventTypeQNames == null || eventTypeQNames.Length != docs.Length)
            {
                Debug.Log("eventTypes is none");
                return;
            }

            eventInstances = eventTypeQNames
                .Select(typeQName => Activator.CreateInstance(Type.GetType(typeQName)) as IUIElementEvent)
                .ToArray();

            eventInstances.ForEach((eventInst, id) => eventInst?.AddEvent(docs[id]?.rootVisualElement));
        }

        private void OnDisable()
        {
            eventInstances = null;
        }
    }

}