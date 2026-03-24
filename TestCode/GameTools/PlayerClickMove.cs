using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerClickMove : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        if(!cam)
            cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            var ray = cam.ScreenPointToRay(Mouse.current.position.value);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                agent.SetDestination(hitInfo.point);
            }
        }
    }
}
