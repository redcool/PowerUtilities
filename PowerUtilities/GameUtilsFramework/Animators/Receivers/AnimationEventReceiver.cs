using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BoxSouls
{
    public class AnimationEventReceiver : MonoBehaviour
    {
        public UnityEvent onOpenDamageTrigger;
        public UnityEvent onCloseDamageTrigger;

        public UnityEvent<bool> onPutBackWeapon;

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

        public void OnPutBackWeapon(int handId)
        {
            onPutBackWeapon?.Invoke(handId==0);
        }

    }
}
