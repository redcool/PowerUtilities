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
    public AnimationPlayableOutput output;
    // Start is called before the first frame update
    void Start()
    {
        graph = PlayableGraph.Create("Blend clips");

        var clipInfos = new(AnimationClip clip, float weight)[clips.Length];
        for (int i = 0; i < clipInfos.Length; i++)
        {
            clipInfos[i].clip = clips[i];
            clipInfos[i].weight = weights[i];
        }

        var mixer = PlayableTools.CreateMixer(graph, clipInfos);

        output = AnimationPlayableOutput.Create(graph, "anim output", GetComponent<Animator>());
        output.SetSourcePlayable(mixer);
        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
