#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.SceneManagement;
    using System.Linq;
    using System;

    [CustomEditor(typeof(TagsSO))]
    public class TagsSOEditor : PowerEditor<TagsSO>
    {
        bool isTagsFolded;

        readonly GUIContent guiSyncScene = new GUIContent("Sync Hierarchy", "create or sync tag objects in hierarchy");
        readonly GUIContent guiSyncMaterial = new GUIContent("Sync Material","sync tag objects material settings");
        readonly GUIContent guiSynnChildrenTag = new GUIContent("Sync ChildrenTag","sync selected object's children tag");

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

            if (GUILayout.Button("Clear Tags", GUILayout.Width(100)))
            //&& EditorUtility.DisplayDialog("Warning","clear all tags?","yes"))
            {
                inst.tagInfoList.ForEach(info =>
                {
                    TagManager.RemoveTag(info.tag);
                });
                TagManager.ClearTags(prop => prop.stringValue =="");
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
                DrawColorLine(pos);

                var tagManagerSo = TagManager.GetTagLayerManager();
                var tagEditor = tagManagerEditor.Get(tagManagerSo, () => Editor.CreateEditor(tagManagerSo.targetObject));
                tagEditor.OnInspectorGUI();
            }

            // editor extends
            DrawColorLine(EditorGUILayout.GetControlRect(GUILayout.Height(2)));

            GUILayout.BeginHorizontal("Editor Extends", "Box");
            if (GUILayout.Button(guiSynnChildrenTag))
            {
                SyncChildrenTag();
            }
            GUILayout.EndHorizontal();

        }

        public static void DrawColorLine(Rect pos,string colorStr= "#749C75")
        {
            ColorUtility.TryParseHtmlString(colorStr, out var color);
            EditorGUITools.DrawBoxColors(pos, backgroundColor: color);
        }

        private void SyncChildrenTag()
        {
            var go = Selection.activeGameObject;
            if (!go)
                return;
            var trs = go.GetComponentsInChildren<Transform>();

            var isConfirm = EditorUtility.DisplayDialog("warning", $"change children tag ,count {trs.Length} ", "yes");
            if (!isConfirm)
                return;

            trs.ForEach(tr =>
            {
                tr.gameObject.tag = go.tag;
            });
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


    /// <summary>
    /// control project's tag
    /// 
    /// </summary>
    [SOAssetPath("Assets/PowerUtilities/Tags.asset")]
    [ProjectSettingGroup("PowerUtils/Tags")]
    public class TagsSO : ScriptableObject
    {
        [ListItemDraw("tag,q:,renderQueue,kw:,keywords", "120,10,50,20,0")]
        public List<TagInfo> tagInfoList = new List<TagInfo>();
    }
}
#endif