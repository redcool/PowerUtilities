using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class TestPlayableTools : MonoBehaviour
{
    public PlayableGraph graph;
    public AnimationClipPlayable clipPlay1;
    public AnimationClipPlayable clipPlay2; 
    public AnimationPlayableOutput output;
    public AnimationClip clip1,clip2;

    public RuntimeAnimatorController controller;

    // Start is called before the first frame update
    void Start()
    {
        var anim = GetComponent<Animator>();
        graph = PlayableGraph.Create("test p");
        output = AnimationPlayableOutput.Create(graph,"graph_output", anim);

        clipPlay1 = PlayableTools.CreateClip(graph, clip1, 2);

        //var mixer = PlayableUtils.CreateMixer(graph, new[] { (clip1,0.5f), (clip2 ,0.5f)});
        AnimatorControllerPlayable animPlay = PlayableTools.CreateAnimator(graph, controller);
        var mixer = PlayableTools.CreateMixer(graph, new[] { (clipPlay1, .01f),((Playable)animPlay,0.1f) });

        output.SetSourcePlayable(mixer);
        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
