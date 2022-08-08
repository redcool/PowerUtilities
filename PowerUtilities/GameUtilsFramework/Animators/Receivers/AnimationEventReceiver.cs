using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BoxSouls
{
    /// <summary>
    /// Receive animation events
    /// put on Animator node
    /// 
    /// add event Receiver by inspector
    /// </summary>
    public class AnimationEventReceiver : MonoBehaviour
    {
        public UnityEvent onOpenDamageTrigger;
        public UnityEvent onCloseDamageTrigger;
        public UnityEvent onPutBackWeapon;

        Animator anim;
        // Start is called before the first frame update
        void Start()
        {
            anim = GetComponent<Animator>();
        }

        void OnOpenDamageTrigger()
        {
            onOpenDamageTrigger?.Invoke();
        }

        void OnCloseDamageTrigger()
        {
            onCloseDamageTrigger?.Invoke();
        }

        void OpenBool(string varName)
        {
            anim.SetBool(varName, true);
        }
        void CloseBool(string varName)
        {
            anim.SetBool(varName, false);
        }

        public void OnPutBackWeapon()
        {
            onPutBackWeapon?.Invoke();
        }

    }
}
