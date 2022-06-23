using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BoxSoul
{
    [Serializable]
    public class StateKeyValue
    {
        public enum When
        {
            Enter,Update,Exit
        }
        public enum ValueType
        {
            Bool,Int,Float,Trigger
        }

        public string stateName;
        public bool boolValue;
        public float floatValue;

        [Header("When")]
        public When when;

        public ValueType valueType;

    }
    public class SetStateVariables : StateMachineBehaviour
    {

        public enum SendMessageDir
        {
            Self,Upward, Broadcast
        }

        [Header("State Parameters")]
        public StateKeyValue[] keyValues;

        [Header("Functions")]
        public SendMessageDir messageDir = SendMessageDir.Self;
        public string[] enterFunctionNames;
        public string[] updateFunctionNames;
        public string[] exitFunctionNames;

        void UpdateStates(Animator animator,StateKeyValue.When when)
        {
            for (int i = 0; i < keyValues.Length; i++)
            {
                var kv = keyValues[i];
                if (kv.when != when)
                    continue;

                // set value by type

                if (kv.valueType == StateKeyValue.ValueType.Trigger)
                {
                    if (kv.boolValue)
                        animator.SetTrigger(kv.stateName);
                    else
                        animator.ResetTrigger(kv.stateName);
                }
                else if (kv.valueType == StateKeyValue.ValueType.Int)
                {
                    animator.SetInteger(kv.stateName, (int)kv.floatValue);
                }
                else if (kv.valueType == StateKeyValue.ValueType.Float)
                {
                    animator.SetFloat(kv.stateName, kv.floatValue);
                }
                else
                    animator.SetBool(kv.stateName, kv.boolValue);
            }
        }

        void InvokeFunctions(Animator anim, string[] functions)
        {
            for (int i = 0; i < functions.Length; i++)
            {
                var func = functions[i];

                if (messageDir == SendMessageDir.Self)
                    anim.SendMessage(func, SendMessageOptions.DontRequireReceiver);
                else if (messageDir == SendMessageDir.Upward)
                    anim.SendMessageUpwards(func, SendMessageOptions.DontRequireReceiver);
                else
                    anim.BroadcastMessage(func, SendMessageOptions.DontRequireReceiver);
            }
        }

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            UpdateStates(animator, StateKeyValue.When.Enter);
            InvokeFunctions(animator,enterFunctionNames);
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            UpdateStates(animator, StateKeyValue.When.Update);
            InvokeFunctions(animator, updateFunctionNames);
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            UpdateStates(animator, StateKeyValue.When.Exit);
            InvokeFunctions(animator, exitFunctionNames);
        }

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
}