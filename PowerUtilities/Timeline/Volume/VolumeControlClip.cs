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
    //[CustomEditor(typeof(VolumeControlClip))]
    public class VolumeControlClipEditor : Editor
    {
        //GUIContent clipProfileContent = new GUIContent("Clip Profile", "show VolumeControlClip's profile settings");
        //GUIContent profileContent = new GUIContent("Template Profile Details", "Show Template/profile  settings");
        //GUIContent bakeTemplateProfileButton = new GUIContent("Bake Template Profile", "copy template profile to clip profile");

        //public override void OnInspectorGUI()
        //{
        //    base.OnInspectorGUI();

        //    serializedObject.Update();

        //    serializedObject.ApplyModifiedProperties();
        //    var inst = target as VolumeControlClip;

        //    //if (GUILayout.Button(bakeTemplateProfileButton))
        //    //{
                
        //    //}
        //}

    }
#endif

    [Serializable]
    public class VolumeControlClip : PlayableAsset
    {
        [Header("--- 0.0.2")]
        //will clone this
        public VolumeControlBehaviour template;

        public string guid;
        public string GetGUID()
        {
            if (string.IsNullOrEmpty(guid))
                guid = GUID.Generate().ToString();
            return guid;
        }

        //template's instance
        //[HideInInspector]
        //public VolumeControlBehaviour instance;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var sp = ScriptPlayable<VolumeControlBehaviour>.Create(graph,template);
            var b = sp.GetBehaviour();
            b.TrySetup(owner,sp,GetGUID());

            //instance = b;

            //template.TrySetup(owner);
            return sp;
        }
        
    }
}
