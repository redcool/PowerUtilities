using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayerAnimControl : MonoBehaviour
{
    [Header("Locomotion")]
    public AnimationClip idle, walk, run;
    public float[] maxSpeeds = new[] { 2f, 5, 7 };
    [Min(0)]public float speed;
    

    public AnimationClip beHit, jump, dead;

    Animator anim;

    PlayableGraph graph;

    AnimationLayerMixerPlayable layerMixer;
    AnimationMixerPlayable locomotionMixer;

    NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        anim = GetComponent<Animator>();
        AnimationPlayableOutput animOutput = default;
        PlayableGraphTools.CreateGraph(anim, ref graph, ref animOutput);

        //graph.CreateClips(playDict, clips);

        locomotionMixer = graph.CreateMixer(idle, walk, run);

        animOutput.SetSourcePlayable(locomotionMixer);
        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        var speed = agent.velocity.magnitude;
        locomotionMixer.UpdateAllInputWeight(maxSpeeds, speed);


    }

}
