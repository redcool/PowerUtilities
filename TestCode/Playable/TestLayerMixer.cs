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
    public AnimationPlayableOutput animOutput;

    public AnimationClip clip1;
    public AvatarMask clip1Mask;

    public RuntimeAnimatorController controller;
    public AvatarMask controllerMask;

    AnimationLayerMixerPlayable layerMixer;
    [Range(0,1)]public float weight;

    // Start is called before the first frame update
    void Start()
    {
        var anim = GetComponent<Animator>();

        PlayableGraphTools.CreateGraph(anim, ref graph, ref animOutput);

        clipPlay1 = graph.CreateClip(clip1);

        //var mixer = PlayableUtils.CreateMixer(graph, new[] { (clip1,0.5f), (clip2 ,0.5f)});
        AnimatorControllerPlayable animPlay = graph.CreateAnimator(controller);
        layerMixer = graph.CreateLayerMixer(new[] {
            (clipPlay1, .01f, clip1Mask,false),
            ((Playable)animPlay, 0.1f, controllerMask,true)
        });

        layerMixer.SetLayerAdditive(1, true);
        animOutput.SetSourcePlayable(layerMixer);
        graph.Play();

    }

    // Update is called once per frame
    void Update()
    {
        layerMixer.SetInputWeight(0, weight);
        layerMixer.SetInputWeight(1, 1 - weight);
    }
}
