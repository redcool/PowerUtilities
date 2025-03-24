namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Object = UnityEngine.Object;

    public static class CoroutineTool
    {
        static GameObject coroutineGO;
        public static GameObject CoroutineGO
        {
            get
            {
                if (coroutineGO == null)
                {
                    coroutineGO = new GameObject("CoroutineTool");
                    coroutineGO.AddComponent<CoroutineToolBehaviour>();
                    Object.DontDestroyOnLoad(coroutineGO);
                }
                return coroutineGO;
            }
        }

        public static bool TryMoveNext(IEnumerator enumerator)
        {
            var ite = enumerator;
            if (ite.Current == null)
                ite.MoveNext();

            if (ite.Current is WaitForDone)
            {
                var wait = ite.Current as WaitForDone;
                if (wait.CanMoveNext)
                {
                    return ite.MoveNext();
                }
            }
            return true;
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.update -= OnUpdate;
                EditorApplication.update += OnUpdate;
            }
#endif
            if(Application.isPlaying)
            {
                CoroutineToolBehaviour.OnUpdateEvent -= OnUpdate;
                CoroutineToolBehaviour.OnUpdateEvent += OnUpdate;
            }

            SceneManagerTools.AddSceneUnloaded(OnSceneUnloaded);
        }

        private static void OnSceneUnloaded(Scene arg0)
        {
            itemList.Clear();
        }

        static List<IEnumerator> itemList = new();
        public static void StartCoroutine(IEnumerator enumerator)
        {
            itemList.Add(enumerator);
        }

        public static void StopCoroutine(IEnumerator enumerator)
        {
            if(itemList.Contains(enumerator))
                itemList.Remove(enumerator);
        }

        public static void StopAllCoroutines()
        {
            itemList.Clear();
        }

        public static void OnUpdate()
        {
            for (int i = itemList.Count - 1; i >= 0; i--)
            {
                var item = itemList[i];
                var canMove = TryMoveNext(item);
                if (!canMove)
                    itemList.RemoveAt(i);
            }
        }
    }

    public class CoroutineToolBehaviour : MonoBehaviour
    {
        public static event Action OnUpdateEvent;

        void Update()
        {
            OnUpdateEvent?.Invoke();
        }

        void OnDestroy()
        {
            OnUpdateEvent = null;
        }
    }
}