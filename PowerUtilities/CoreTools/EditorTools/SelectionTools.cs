#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    public static class SelectionTools
    {
        public static T[] GetSelectedComponents<T>() where T : Component
        {
            return Selection.gameObjects.Select(go => go.GetComponent<T>()).Where(t => t).ToArray();
        }
    }
}
#endif