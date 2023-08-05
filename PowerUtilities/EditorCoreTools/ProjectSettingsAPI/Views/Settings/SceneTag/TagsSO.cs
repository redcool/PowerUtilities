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

        readonly GUIContent guiSyncScene = new GUIContent("Sync Hierarchy", "create or sync tag objects in hierarchy");
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

            if (GUILayout.Button("Clear Tags",GUILayout.Width(100)) && EditorUtility.DisplayDialog("Warning","clear all tags?","yes"))
            {
                TagManager.ClearTags();
            }

            if (GUILayout.Button(guiSyncScene))
            {
                CreateOrSyncHierarchy(inst.tagInfoList);
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
            mat.shaderKeywords = tagInfo.keywords.SplitBy();
        }

        private void CreateOrSyncHierarchy(List<TagInfo> tagInfoList)
        {
            const string ROOT = "Root"
                ,LIGHTMAPED = "Lighmaped"
                ,NO_LIGHTMAPED = "NoLightmaped";


            var scene = SceneManager.GetActiveScene();
            var rootGo = scene.GetRootGameObjects()
                .Where(go => go.name ==ROOT)
                .FirstOrDefault();

            if (!rootGo)
                rootGo = new GameObject(ROOT);

            GameObject lightmapGO,noLightmapGO;
            var list = GameObjectTools.CreateLinkChildren(rootGo, lightmapGO = new GameObject(LIGHTMAPED), noLightmapGO = new GameObject(NO_LIGHTMAPED));

            list.ForEach(root2Go =>
            {
                tagInfoList.ForEach(tagInfo =>
                {
                    if (string.IsNullOrEmpty(tagInfo.tag))
                        return;
                    // get root
                    var tagTr = root2Go.transform.Find(tagInfo.tag);
                    if (tagTr == null)
                    {
                        var tagGo = new GameObject(tagInfo.tag);
                        tagTr = tagGo.transform;
                        tagTr.SetParent(root2Go.transform, false);
                    }
                    tagTr.tag = tagInfo.tag;
                });
            });

        }
    }

    [CustomPropertyDrawer(typeof(TagInfo))]
    public class TagInfoDrawer : PropertyDrawer
    {
        void SetPos(ref Rect pos,float offsetX,float w)
        {
            pos.x += offsetX;
            pos.width = w;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var tag = property.FindPropertyRelative("tag");
            var queue = property.FindPropertyRelative("renderQueue");
            var keywords = property.FindPropertyRelative("keywords");

            var pos = position;
            SetPos(ref pos, 0, 120);
            EditorGUI.PropertyField(pos, tag, GUIContent.none);

            SetPos(ref pos, pos.width, 10);
            EditorGUI.LabelField(pos,"q:");

            SetPos(ref pos, pos.width, 50);
            EditorGUI.PropertyField(pos,queue,GUIContent.none);

            SetPos(ref pos, pos.width, 20);
            EditorGUI.LabelField(pos,"kw:");

            SetPos(ref pos, pos.width, position.width - pos.x);
            EditorGUI.PropertyField(pos, keywords,GUIContent.none);

            EditorGUI.EndProperty();
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
        [Tooltip("keywords,split by ,")]
        public string keywords="";
        public override string ToString()
        {
            return $"{tag},q:{renderQueue},kw:{keywords.Length}";
        }
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