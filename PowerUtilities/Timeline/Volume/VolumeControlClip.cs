using System;
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
        GUIContent profileContent = new GUIContent("Template Profile", "Show Template/profile  settings");
        GUIContent copyProfileButton = new GUIContent("Bake Volume Profile","copy template profile to clip profile");

        Editor profileEditor;
        private bool isProfileFolded;

        // clip owner(editor only)
        VolumeProfile clipProfile;
        Editor clipProfileEditor;
        bool isClipProfileFolded;

        private void OnEnable()
        {
            //Debug.Log("onenable");
            if (profileEditor)
            {
                profileEditor.serializedObject.Dispose();
                profileEditor = null;
            }

            var inst = target as VolumeControlClip;
            EditorTools.CreateEditor(inst.template.profile, ref profileEditor);
        }

        private void OnDisable()
        {
            //Debug.Log("clip disable");
            profileEditor = null;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var inst = target as VolumeControlClip;

            if(GUILayout.Button(copyProfileButton))
            {
                inst.template.components = inst.template.profile.components;

                // reset
                profileEditor = null;
                clipProfileEditor = null;
                clipProfile = null;
                OnEnable();
                return;
            }

            //clip profile
            EditorGUITools.BeginFoldoutHeaderGroupBox(ref isClipProfileFolded, clipProfileContent, () =>
            {
                TryInitclipProfile(inst);
                clipProfileEditor?.OnInspectorGUI();
            });
            // profile
            EditorGUITools.BeginFoldoutHeaderGroupBox(ref isProfileFolded, profileContent , () =>
            {
                profileEditor?.OnInspectorGUI();
            });
        }

        void TryInitclipProfile(VolumeControlClip inst)
        {
            if (clipProfile)
                return;

            clipProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            clipProfile.components = inst.template.components;

            if (clipProfileEditor)
                clipProfileEditor = null;

            EditorTools.CreateEditor(clipProfile, ref clipProfileEditor);
        }
    }
#endif

    [Serializable]
    public class VolumeControlClip : PlayableAsset
    {
        public VolumeControlBehaviour template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var sp = ScriptPlayable<VolumeControlBehaviour>.Create(graph,template);
            var b = sp.GetBehaviour();
            b.TrySetup(owner);

            return sp;
        }
        
    }
}
