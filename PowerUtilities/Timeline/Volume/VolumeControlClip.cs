using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

namespace PowerUtilities.Timeline
{
#if UNITY_EDITOR
    [CustomEditor(typeof(VolumeControlClip))]
    public class VolumeControlClipEditor : Editor
    {
        GUIContent clipProfileContent = new GUIContent("Clip Profile", "show VolumeControlClip's profile settings");
        GUIContent profileContent = new GUIContent("Template Profile Details", "Show Template/profile  settings");
        GUIContent bakeTemplateProfileButton = new GUIContent("Bake Template Profile", "copy template profile to clip profile");


        private void OnEnable()
        {
            var inst = target as VolumeControlClip;

        }

        private void OnDisable()
        {
            //Debug.Log("clip disable");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var inst = target as VolumeControlClip;

            if (GUILayout.Button(bakeTemplateProfileButton))
            {
                
            }
        }

    }
#endif

    [Serializable]
    public class VolumeControlClip : PlayableAsset
    {
        //will clone this
        public VolumeControlBehaviour template;

        //template's instance
        public VolumeControlBehaviour instance;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var sp = ScriptPlayable<VolumeControlBehaviour>.Create(graph,template);
            var b = sp.GetBehaviour();
            b.TrySetup(owner);

            instance = b;

            return sp;
        }
        
    }
}
