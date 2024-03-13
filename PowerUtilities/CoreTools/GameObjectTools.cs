namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class GameObjectTools
    {

        /// <summary>
        /// Get object's path from root to tr.
        /// Dont not include root
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="root"></param>
        /// <returns>path string or ""</returns>
        public static string GetHierarchyPath(this Transform tr, Transform root = null, string separator = "/")
        {
            if (!tr)
                return "";

            if (root == tr)
                return root.name;

            var sb = new StringBuilder();
            sb.Append(tr.name);

            while (true)
            {
                tr = tr.parent;
                if (!tr || tr == root)
                    break;

                sb.Insert(0, separator);
                sb.Insert(0, tr.name);
            }
            return sb.ToString();
        }

        public static string GetHierarchyPath(this Transform tr, string rootTrName, string separator = "/")
        {
            if (!tr || string.IsNullOrEmpty(rootTrName))
                return "";

            if (tr.name == rootTrName)
                return rootTrName;

            var sb = new StringBuilder();
            sb.Append(tr.name);

            while (true)
            {
                tr = tr.parent;
                if (!tr || tr.name == rootTrName)
                    break;

                sb.Insert(0, separator);
                sb.Insert(0, tr.name);
            }
            return sb.ToString();
        }

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            if (!comp)
                comp = go.AddComponent<T>();
            return comp;
        }

        public static Component GetOrAddComponent(this GameObject go, Type type)
        {
            if (type == default)
                return default;

            var comp = go.GetComponent(type);
            if (!comp)
                comp = go.AddComponent(type);
            return comp;
        }

        public static Component[] GetComponents(this GameObject go, string typeName, Func<string, string, bool> predicate)
        {
            if (predicate == null)
                return default;

            var comps = go.GetComponents(typeof(Component))
                .Where(comp => predicate(comp.GetType().Name, typeName))
                ;
            return comps.ToArray();
        }

        public static Component[] GetComponents(this GameObject go, string typeName, StringEx.NameMatchMode matchMode)
        {
            var comps = go.GetComponents(typeof(Component))
                .Where(comp => comp.GetType().Name.IsMatch(typeName, matchMode))
                ;
            return comps.ToArray();
        }

        public static void DestroyComponent<T>(this GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (!c)
                return;
#if UNITY_EDITOR
            Object.DestroyImmediate(c);
#else
            Object.Destroy(c);
#endif
        }

        /// <summary>
        /// Destroy children which has component T
        /// with undo in editor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="includeInactive"></param>
        public static void DestroyChildren<T>(this GameObject go, bool includeInactive = false) where T : Component
        {
            // Transform, 
            if(typeof(T) == typeof(Transform))
            {
                DestroyChildren(go, includeInactive);
                return;
            }

            // not Transform
            var childrenGos = go.GetComponentsInChildren<T>(includeInactive)
                .Select(c => c.gameObject)
                .ToArray();

            //childrenGos.ForEach(c =>
            foreach (var c in childrenGos)
            {
                if (c.gameObject == go)
                    continue;
#if UNITY_EDITOR
                Undo.DestroyObjectImmediate(c);
#else
                Object.Destroy(c);
#endif
            };
        }

        public static void DestroyChildren(this GameObject go, bool includeInactive = false)
        {
            for (int i = go.transform.childCount - 1; i >= 0; i--)
            {
                var c = go.transform.GetChild(i).gameObject;
#if UNITY_EDITOR
                Undo.DestroyObjectImmediate(c);
#else
                Object.Destroy(c);
#endif
            }
        }

        public static void Destroy(this Object obj,bool allowDestroyingAssets=false)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(obj,allowDestroyingAssets);
#else
            Object.Destroy(obj);
#endif
        }

        public static GameObject[] CreateLinkChildren(this GameObject go, params GameObject[] children)
        {
            if (children == null)
                return default;

            children.ForEach(c =>
            {
                if (go.transform.Find(c.name) == null)
                    c.transform.SetParent(go.transform, false);
            });
            return children;
        }

        public static void SetChildrenActive(this GameObject go,bool isActive)
        {
            for (int i = 0;i< go.transform.childCount; i++)
            {
                go.transform.GetChild(i).gameObject.SetActive(isActive);
            }
        }


        static void FindChildrenRecursive(ref List<Transform> list, Transform tr)
        {
            for (int i = 0; i < tr.childCount; i++)
            {
                var child = tr.GetChild(i);
                list.Add(child);

                FindChildrenRecursive(ref list, child);
            }
        }
        /// <summary>
        /// find children depth first
        /// </summary>
        /// <param name="list"></param>
        /// <param name="tr"></param>
        public static void FindChildrenRecursive(this GameObject go, ref List<Transform> list)
        {
            FindChildrenRecursive(ref list, go.transform);
        }

        public static void RenameHierarchy(this GameObject go,string replacedName,string newName)
        {
            if (string.IsNullOrEmpty(replacedName))
                return;

            var trs = go.GetComponentsInChildren<Transform>();
            trs.ForEach(tr =>
            {
                tr.gameObject.name = Regex.Replace(tr.gameObject.name, replacedName, newName);
            });
        }
    }
}