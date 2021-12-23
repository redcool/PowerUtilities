namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEngine;
    using System.Collections;
    using UnityEditor;
    using System.Collections.Generic;

    public class MaterialRefInfo
    {
        public Material mat;
        public List<GameObject> objects = new List<GameObject>();
        public bool isFold;
        public Vector2 scrollPos;

    }

    public class GroupBatcherWindow : EditorWindow
    {

        Dictionary<Material, MaterialRefInfo> dict = new Dictionary<Material, MaterialRefInfo>();
        Vector2 rootScrollPos;
        bool isRootFold;

        [MenuItem("PowerUtilities/BatchTools/GroupBatcherWindow")]
        static void Init()
        {
            var win = GetWindow<GroupBatcherWindow>();
            win.Show();
        }
        void OnSelectionChange()
        {
            Classify(Selection.activeGameObject);
            Repaint();
        }

        void OnGUI()
        {
            if (!Selection.activeGameObject)
            {
                EditorGUILayout.HelpBox("Select a gameObject in Hierarchy.", MessageType.Info);
                return;
            }

            if (dict.Count == 0)
            {
                //Classify(Selection.activeGameObject);
                EditorGUILayout.HelpBox("Select a active gameObject in Hierarchy.", MessageType.Info);
                return;
            }
            EditorGUILayout.HelpBox(Selection.activeGameObject.name, MessageType.Info);

            ShowClassify();
            ShowButtons();
        }

        void Classify(GameObject go)
        {
            if (!go)
                return;

            dict.Clear();
            var mrs = go.GetComponentsInChildren<MeshRenderer>();

            foreach (var mr in mrs)
            {
                if (!dict.ContainsKey(mr.sharedMaterial))
                {
                    dict.Add(mr.sharedMaterial, new MaterialRefInfo { mat = mr.sharedMaterial });
                }
                dict[mr.sharedMaterial].objects.Add(mr.gameObject);
            }
        }

        List<List<CombineInstance>> GetMeshGroupsByMaterial(MaterialRefInfo info)
        {
            var rootList = new List<List<CombineInstance>>();
            rootList.Add(new List<CombineInstance>());

            var vertexCount = 0;
            var groupId = 0;
            foreach (var go in info.objects)
            {
                var mf = go.GetComponent<MeshFilter>();
                if (mf && mf.sharedMesh)
                {
                    if (vertexCount + mf.sharedMesh.vertexCount > ushort.MaxValue)
                    {
                        rootList.Add(new List<CombineInstance>());
                        groupId++;
                        vertexCount = 0;
                    }
                    vertexCount += mf.sharedMesh.vertexCount;
                    var combine = new CombineInstance();
                    combine.mesh = mf.sharedMesh;
                    combine.transform = go.transform.localToWorldMatrix;

                    rootList[groupId].Add(combine);
                }
            }
            return rootList;
        }

        void BakeMeshes()
        {
            if (dict.Count == 0)
                return;

            var rootName = Selection.activeGameObject.name;
            var newRoot = new GameObject(rootName + "_Combine");

            foreach (var kv in dict)
            {
                var mat = kv.Key;
                var info = kv.Value;
                var groupId = 0;

                var rootList = GetMeshGroupsByMaterial(info);
                foreach (var group in rootList)
                {
                    CombineMesh(newRoot, group.ToArray(), mat, groupId++);
                }
            }
            EditorGUIUtility.PingObject(newRoot);
        }

        void CombineMesh(GameObject rootGo, CombineInstance[] combines, Material mat, int id)
        {
            var mesh = new Mesh();
            mesh.CombineMeshes(combines);

            var newGo = new GameObject(string.Format("{0}_g{1}", mat.name, id));
            newGo.transform.SetParent(rootGo.transform);

            var newFilter = newGo.AddComponent<MeshFilter>();
            newFilter.sharedMesh = mesh;

            var newRenderer = newGo.AddComponent<MeshRenderer>();
            newRenderer.sharedMaterial = mat;
        }

        void ShowClassify()
        {
            EditorGUI.indentLevel = 1;
            isRootFold = EditorGUILayout.Foldout(isRootFold, string.Format("Material Count:{0}", dict.Keys.Count));
            if (!isRootFold)
                return;

            rootScrollPos = EditorGUILayout.BeginScrollView(rootScrollPos);
            foreach (var kv in dict)
            {
                var info = kv.Value;

                EditorGUI.indentLevel = 2;
                EditorGUILayout.BeginHorizontal();
                info.isFold = EditorGUILayout.Foldout(info.isFold, string.Format("{0},Ref Count:{1}", kv.Key.name, info.objects.Count));
                EditorGUILayout.ObjectField(kv.Key, typeof(Material), true);
                EditorGUILayout.EndHorizontal();

                if (info.isFold)
                {
                    info.scrollPos = EditorGUILayout.BeginScrollView(info.scrollPos, GUILayout.ExpandHeight(false));
                    foreach (var obj in info.objects)
                    {
                        EditorGUI.indentLevel = 3;
                        EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void ShowButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set EditorOnly Tag"))
            {
                Selection.activeGameObject.tag = "EditorOnly";
            }

            if (GUILayout.Button(string.Format("Bake {0} Meshes", dict.Keys.Count)))
            {
                BakeMeshes();
            }
            GUILayout.EndHorizontal();
        }
    }
#endif
}