using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class TestLayerMixer : MonoBehaviour
{
    public PlayableGraph graph;
    public AnimationClipPlayable clipPlay1;
    public AnimationPlayableOutput output;

    public AnimationClip clip1;
    public AvatarMask clip1Mask;

    public RuntimeAnimatorController controller;
    public AvatarMask controllerMask;

    AnimationLayerMixerPlayable mixer;
    [Range(0,1)]public float weight;

    // Start is called before the first frame update
    void Start()
    {
        var anim = GetComponent<Animator>();
        graph = PlayableGraph.Create("test p");
        output = AnimationPlayableOutput.Create(graph,"graph_output", anim);

        clipPlay1 = PlayableTools.CreateClip(graph, clip1, 2);

        //var mixer = PlayableUtils.CreateMixer(graph, new[] { (clip1,0.5f), (clip2 ,0.5f)});
        AnimatorControllerPlayable animPlay = PlayableTools.CreateAnimator(graph, controller);
        mixer = PlayableTools.CreateLayerMixer(graph, new[] {
            (clipPlay1, .01f, clip1Mask),
            ((Playable)animPlay, 0.1f, controllerMask)
        });

        output.SetSourcePlayable(mixer);
        graph.Play();
    }

    // Update is called once per frame
    void Update()
    {
        mixer.SetInputWeight(0, weight);
        mixer.SetInputWeight(1, 1 - weight);
    }
}
