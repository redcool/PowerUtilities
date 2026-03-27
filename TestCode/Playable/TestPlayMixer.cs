using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class TestPlayMixer : MonoBehaviour
{
    public PlayableGraph graph;

    public AnimationClip[] clips;
    public float[] weights;

    AnimationMixerPlayable mixerPlay;
    // Start is called before the first frame update
    void Start()
    {
        var anim = GetComponent<Animator>();

        AnimationPlayableOutput animOutput = default;
        PlayableGraphTools.CreateGraph(anim, ref graph, ref animOutput);

        mixerPlay = graph.CreateMixer(clips);
        animOutput.SetSourcePlayable(mixerPlay);

        graph.Play();
    }

    void Update()
    {
        for (int i = 0; i < mixerPlay.GetInputCount(); i++)
        {
            mixerPlay.SetInputWeight(i, weights[i]);
        }
    }

    public void OnDisable()
    {
        graph.TryDestory();
    }
}
