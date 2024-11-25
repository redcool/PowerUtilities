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

        }

        private void OnDisable()
        {
            //Debug.Log("clip disable");
            profileEditor = null;
        }

        void DrawClipVolume(SerializedProperty templateProp)
        {
            DrawTemplateChildProp(templateProp, nameof(VolumeControlBehaviour.volumeRef));
            DrawTemplateChildProp(templateProp, nameof(VolumeControlBehaviour.volumeWeight));
            DrawTemplateChildProp(templateProp, nameof(VolumeControlBehaviour.clipVolume));
            DrawTemplateChildProp(templateProp, nameof(VolumeControlBehaviour.clipVolumeProfile));

        }

        void DrawTemplateVolume(SerializedProperty templateProp)
        {
            DrawTemplateChildProp(templateProp, nameof(VolumeControlBehaviour.profile));
        }
        static void DrawTemplateChildProp(SerializedProperty templateProp, string subPropName)
        {
            var prop = templateProp.FindPropertyRelative(subPropName);
            EditorGUILayout.PropertyField(prop, true);
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var inst = target as VolumeControlClip;
            var templateProp = serializedObject.FindProperty(nameof(VolumeControlClip.template));
            base.OnInspectorGUI();

            EditorTools.CreateEditor(inst.template.profile, ref profileEditor);

            //DrawClipVolume(templateProp);

            if (GUILayout.Button(bakeTemplateProfileButton))
            {
                inst.template.clipVolumeProfile = Instantiate(inst.template.profile);

                //save


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

            //DrawTemplateVolume(templateProp);
            // template profile
            EditorGUITools.BeginFoldoutHeaderGroupBox(ref isProfileFolded, profileContent, () =>
            {
                profileEditor?.OnInspectorGUI();
            });

            serializedObject.ApplyModifiedProperties();
        }

        void TryInitclipProfile(VolumeControlClip inst)
        {
            if (clipProfile)
                return;

            clipProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            //clipProfile.components = inst.template.components;

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
