#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace PowerUtilities
{
    public static class LocalUndo
    {
        static int index = 0;
        static int maxIndex = 0;
        public static int Index => index;
        public static int MaxIndex => maxIndex;
        public static void Record(Object obj, string name)
        {
            index++;
            maxIndex++;

            Undo.RecordObject(obj, name);
        }
        public static void PerformUndo()
        {
            if (index <= 0)
                return;

            index--;
            Undo.PerformUndo();
        }
        public static void PerformRedo()
        {
            if (index >= maxIndex)
                return;

            index++;
            Undo.PerformRedo();
        }

        public static void Clear()
        {
            maxIndex = 0;
            index = 0;
        }

        public static void Init()
        {
            Clear();
        }
    }
}
#endif