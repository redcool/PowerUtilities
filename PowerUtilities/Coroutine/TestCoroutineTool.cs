namespace PowerUtilities.Coroutine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TestCoroutineTool : MonoBehaviour
    {

        IEnumerator TestCoroutine()
        {
            yield return 12; // num,wait for a frame
            Debug.Log("TestCoroutine");

            Debug.Log("start WaitForSeconds 5");
            yield return new WaitForSeconds(5);
            Debug.Log("end WaitForSeconds 5");

            Debug.Log("start WaitForEndOfFrame 10");
            yield return new WaitForEndOfFrame(10);
            Debug.Log("end WaitForEndOfFrame 10");

            var startTime = DateTime.Now.Second;
            Debug.Log("start WaitForActionDone " + startTime);
            yield return new WaitForActionDone(() =>
            {
                Debug.Log(DateTime.Now.Second + ":" + startTime);
                var lastTime = (DateTime.Now.Second - startTime);
                Debug.Log("WaitForActionDone "+lastTime);
                return lastTime > 10;
            });
            Debug.Log("end WaitForActionDone");

        }

        [EditorButton(onClickCall = "OnClick")]
        public bool isTest;

        [EditorButton(onClickCall = "OnClickStopAll")]
        public bool isStopAll;

        void OnClick()
        {
            CoroutineTool.StartCoroutine(TestCoroutine());
        }

        void OnClickStopAll()
        {
            CoroutineTool.StopAllCoroutines();
        }

        // Start is called before the first frame update
        void Start()
        {
            //CoroutineTool.StartCoroutine(TestCoroutine());

            CoroutineTool.StartCoroutine(TestCoroutine(),true);
        }

    }


}