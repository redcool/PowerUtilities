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
            Debug.Log("TestCoroutine");
            yield return new WaitForSeconds(5);
            Debug.Log("end WaitForSeconds 5");

            yield return new WaitForEndOfFrame(10);
            Debug.Log("end WaitForEndOfFrame");

            var startTime = Time.time;
            yield return new WaitForActionDone(() =>
            {
                Debug.Log("WaitForActionDone");
                return Time.time - startTime > 10;
            });
            Debug.Log("end WaitForActionDone");

            yield return new WaitForDone() { CanMoveNext = true };
            Debug.Log("test 2");
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