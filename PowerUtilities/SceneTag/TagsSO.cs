#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.SceneManagement;
    using System.Linq;

    [CustomEditor(typeof(TagsSO))]
    public class TagsSOEditor : PowerEditor<TagsSO>
    {
        bool isTagsFolded;

        readonly GUIContent guiSyncScene = new GUIContent("Sync Hierarchy", "create tag objects in hierarchy");
        readonly GUIContent guiSyncMaterial = new GUIContent("Sync Material","sync tag objects material settings");

        CacheTool<SerializedObject, Editor> tagManagerEditor = new CacheTool<SerializedObject, Editor>();
        public override bool NeedDrawDefaultUI() => true;
        public override void DrawInspectorUI(TagsSO inst)
        {
            GUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Sync Tags"))
            {
                inst.tagInfoList.ForEach(info =>
                {
                    TagManager.AddTag(info.tag);
                });
            }

            if (GUILayout.Button("Clear Tags"))
            {
                TagManager.ClearTags();
            }

            if (GUILayout.Button(guiSyncScene))
            {
                SyncHierarchy(inst.tagInfoList);
            }

            if (GUILayout.Button(guiSyncMaterial))
            {
                SyncTagObjectsMaterial(inst.tagInfoList);
            }

            GUILayout.EndHorizontal();

            // show tag manager
            if (isTagsFolded = EditorGUILayout.Foldout(isTagsFolded, "Show TagManager", true))
            {
                // show color line
                var pos = EditorGUILayout.GetControlRect(GUILayout.Height(2));

                ColorUtility.TryParseHtmlString("#749C75", out var color);
                EditorGUITools.DrawBoxColors(pos, backgroundColor: color);

                var tagManagerSo = TagManager.GetTagLayerManager();
                var tagEditor = tagManagerEditor.Get(tagManagerSo, () => Editor.CreateEditor(tagManagerSo.targetObject));
                tagEditor.OnInspectorGUI();
            }
        }

        private void SyncTagObjectsMaterial(List<TagInfo> tagInfoList)
        {
            tagInfoList.ForEach(tagInfo =>
            {
                if (string.IsNullOrEmpty(tagInfo.tag))
                    return;

                var gos = GameObject.FindGameObjectsWithTag(tagInfo.tag);
                gos.ForEach(go =>
                {
                    if(go.TryGetComponent<MeshRenderer>(out var mr))
                    {
                        mr.sharedMaterials.ForEach(m => SyncTagObjectMaterial(tagInfo,m));
                    }
                });
            });
        }

        public void SyncTagObjectMaterial(TagInfo tagInfo, Material mat)
        {
            mat.renderQueue = tagInfo.renderQueue;
            mat.shaderKeywords = tagInfo.keywordList.ToArray();
        }

        private void SyncHierarchy(List<TagInfo> tagInfoList)
        {
            const string ROOT = "Root";
            var scene = SceneManager.GetActiveScene();
            var rootGo = scene.GetRootGameObjects()
                .Where(go => go.name ==ROOT)
                .FirstOrDefault();

            if (!rootGo)
                rootGo = new GameObject(ROOT);

            tagInfoList.ForEach(tagInfo =>
            {
                if(string.IsNullOrEmpty(tagInfo.tag))
                    return;
                // get root
                var tagTr = rootGo.transform.Find(tagInfo.tag);
                if (tagTr == null)
                {
                    var tagGo = new GameObject(tagInfo.tag);
                    tagTr = tagGo.transform;
                    tagTr.SetParent(rootGo.transform, false);
                }

                // spread tag to children
                tagTr.GetComponentsInChildren<Transform>().ForEach(tr =>
                {
                    tr.tag = tagInfo.tag;
                });

            });
        }
    }

    [Serializable]
    public class TagInfo
    {
        /// <summary>
        /// object 's tag
        /// </summary>
        public string tag;

        /// <summary>
        /// object material's renderQueue
        /// </summary>
        public int renderQueue = 2000;

        /// <summary>
        /// material's keywords
        /// </summary>
        public List<string> keywordList = new List<string>();
    }
    /// <summary>
    /// control project's tag
    /// 
    /// </summary>
    [SOAssetPath("Assets/PowerUtilities/Tags.asset")]
    [ProjectSettingGroup("PowerUtils/Tags")]
    public class TagsSO : ScriptableObject
    {
        public List<TagInfo> tagInfoList = new List<TagInfo>();
    }
}
#endif