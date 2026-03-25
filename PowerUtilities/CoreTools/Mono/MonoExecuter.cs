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


        [EditorButton(onClickCall = nameof(StartRun))]
        public bool isStart;

        public bool isStop;
        public int count;

        CancellationTokenSource cts;

        MethodInfo startFunc, updateFunc;
        async void StartRun()
        {
            isStop = false;
            if (mono == null)
                return;
            ReflectionTools.GetMethod(mono.GetType(), ref startFunc, "Start");
            ReflectionTools.GetMethod(mono.GetType(), ref updateFunc, "Update");


            cts?.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            //mono.SendMessage("Start",SendMessageOptions.DontRequireReceiver);
            startFunc?.Invoke(mono, ReflectionTools.EMPTY_ARGS);
            while (!token.IsCancellationRequested && !isStop)
            {
                //mono.SendMessage("Update", SendMessageOptions.DontRequireReceiver);
                updateFunc?.Invoke(mono, ReflectionTools.EMPTY_ARGS);
                await Task.Delay(16);
            }
            count++;
            Debug.Log("ExecuteMonoManual finish");
        }

    }
}
