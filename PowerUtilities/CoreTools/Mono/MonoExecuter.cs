using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace PowerUtilities.Test
{

    public class MonoExecuter : MonoBehaviour
    {
        [HelpBox]
        public string helpBox = "Execute mono manual";

        public MonoBehaviour mono;
        MonoBehaviour lastMono;

        [EditorButton(onClickCall = nameof(StartRun))]
        public bool isStart;

        public bool isStop;
        public int id;

        CancellationTokenSource cts;

        MethodInfo startFunc, updateFunc,onDisableFunc,onEnableFunc,onDestoryFunc;

        public void ClearMethods()
        {
            startFunc = null;
            updateFunc = null;
            onDisableFunc = null;
            onEnableFunc = null;
            onDestoryFunc = null;
        }
        public void TrySetupMethods()
        {
            var type = mono.GetType();
            ReflectionTools.GetMethod(type, ref startFunc, "Start");
            ReflectionTools.GetMethod(type, ref updateFunc, "Update");
            ReflectionTools.GetMethod(type, ref onDisableFunc, "OnDisable");
            ReflectionTools.GetMethod(type, ref onEnableFunc, "OnEnable");
            ReflectionTools.GetMethod(type, ref onDestoryFunc, "OnDestroy");
        }

        async void StartRun()
        {
            isStop = false;
            if (mono == null)
                return;
            // save first
            lastMono = mono;

            TrySetupMethods();

            // cancel last
            cts?.Cancel();
            if (cts != null && cts.IsCancellationRequested)
            {
                DestoryMono(mono);
            }
            cts = new CancellationTokenSource();
            var token = cts.Token;

            onEnableFunc?.Invoke(mono, ReflectionTools.EMPTY_ARGS);
            startFunc?.Invoke(mono, ReflectionTools.EMPTY_ARGS);

            while (!token.IsCancellationRequested && !isStop)
            {
                updateFunc?.Invoke(mono, ReflectionTools.EMPTY_ARGS);
                await Task.Delay(16);

                if(lastMono != mono)
                {
                    DestoryMono(lastMono);
                    lastMono = mono;
                    break;
                };
                //Debug.Log($"{id} update");
            }
            
            ClearMethods();
            Debug.Log($"{mono} {id} exec finish");
            id++;
        }

        private void DestoryMono(MonoBehaviour mono)
        {
            onDisableFunc?.Invoke(mono, ReflectionTools.EMPTY_ARGS);
            onDestoryFunc?.Invoke(mono, ReflectionTools.EMPTY_ARGS);
        }
    }
}
