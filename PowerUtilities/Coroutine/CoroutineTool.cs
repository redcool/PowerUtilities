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
        public static bool isStopAllWhenSceneUnloaded = true;

        static GameObject coroutineGO;

        static List<IEnumerator> itemList = new();
        static List<IEnumerator> fixedItemList = new();

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
            if (Application.isPlaying)
            {
                SetupCoroutineGo();

                CoroutineToolBehaviour.OnUpdateEvent -= OnUpdate;
                CoroutineToolBehaviour.OnUpdateEvent += OnUpdate;

                CoroutineToolBehaviour.OnFixedUpdateEvent -= OnFixedUpdate;
                CoroutineToolBehaviour.OnFixedUpdateEvent += OnFixedUpdate;
            }

            SceneManagerTools.AddSceneUnloaded(OnSceneUnloaded);
        }

        private static void OnSceneUnloaded(Scene arg0)
        {
            if (isStopAllWhenSceneUnloaded)
                StopAllCoroutines();
        }

        public static GameObject CoroutineGO
        {
            get
            {
                SetupCoroutineGo();
                return coroutineGO;
            }
        }

        private static void SetupCoroutineGo()
        {
            if (coroutineGO == null)
            {
                coroutineGO = new GameObject("CoroutineTool");
                coroutineGO.AddComponent<CoroutineToolBehaviour>();
                Object.DontDestroyOnLoad(coroutineGO);
            }
        }

        static bool TryMoveNext(IEnumerator enumerator)
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

        /// <summary>
        /// Iterate all 
        /// </summary>
        /// <param name="itemList"></param>
        static void OnUpdate(List<IEnumerator> itemList)
        {
            for (int i = itemList.Count - 1; i >= 0; i--)
            {
                var item = itemList[i];
                var canMove = TryMoveNext(item);
                if (!canMove)
                    itemList.RemoveAt(i);
            }
        }

        static void OnUpdate() => OnUpdate(itemList);
        static void OnFixedUpdate() => OnUpdate(fixedItemList);
        /// <summary>
        /// Start Coroutine
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="isFxiedUpdate">use FixedUpdate or Update </param>
        public static void StartCoroutine(IEnumerator enumerator,bool isFxiedUpdate=false)
        {
            var list = isFxiedUpdate ? fixedItemList : itemList;
            list.Add(enumerator);
        }
        /// <summary>
        /// Stop Coroutine
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="isFxiedUpdate">use FixedUpdate or Update </param>
        public static void StopCoroutine(IEnumerator enumerator, bool isFxiedUpdate = false)
        {
            var list = isFxiedUpdate ? fixedItemList : itemList;
            if (list.Contains(enumerator))
                list.Remove(enumerator);
        }
        /// <summary>
        /// Stop All Coroutines
        /// </summary>
        public static void StopAllCoroutines()
        {
            fixedItemList.Clear();
            itemList.Clear();
        }


    }

    public class CoroutineToolBehaviour : MonoBehaviour
    {
        public static event Action OnUpdateEvent,OnFixedUpdateEvent;


        void Update()
        {
            OnUpdateEvent?.Invoke();
        }
        void FixedUpdate()
        {
            OnFixedUpdateEvent?.Invoke();
        }

        void OnDestroy()
        {
            OnUpdateEvent = null;
            OnFixedUpdateEvent = null;
        }
    }
}