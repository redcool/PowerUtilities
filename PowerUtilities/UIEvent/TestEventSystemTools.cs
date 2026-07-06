#if UNITY_INPUT_SYSTEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PowerUtilities
{
    public class TestEventSystemTools : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Debug.Log("right click");
                EventSystemTools.ClickScreen(new Vector2(0.5f, 0.5f),0);
            }
        }
    }
}
#endif