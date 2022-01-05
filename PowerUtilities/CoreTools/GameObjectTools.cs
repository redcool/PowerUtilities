namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;

    public static class GameObjectTools
    {
        /// <summary>
        /// Get object's path from root to tr.
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="root"></param>
        /// <returns>path string or ""</returns>
        public static string GetHierarchyPath(this Transform tr, Transform root = null,string separator="/")
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
                if (tr == null || tr == root)
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
    }
}