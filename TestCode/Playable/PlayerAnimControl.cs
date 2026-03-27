using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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


    Dictionary<AnimationClip, AnimationClipPlayable> playDict = new();

    // Start is called before the first frame update
    void Start()
    {

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
        UpdateLocomotion();
    }

    private void UpdateLocomotion()
    {
        var weight = PlayableEx.CalcWeight(maxSpeeds, speed, out var portId);
        Debug.Log(portId + ":" + weight);

        for (int i = 0; i < locomotionMixer.GetInputCount(); i++)
        {
            locomotionMixer.SetInputWeight(i, 0);
            locomotionMixer.GetInput(i).Pause();
        }

        if (portId != 0)
        {
            locomotionMixer.GetInput(portId-1).Play();
            locomotionMixer.SetInputWeight(portId - 1, 1 - weight);
        }
        locomotionMixer.GetInput(portId).Play();
        locomotionMixer.SetInputWeight(portId, weight);
    }
}
