#if UNITY_EDITOR
using UnityEngine.UIElements;

namespace PowerUtilities.UIElements
{
    public static class VisualElementEx
    {
        public static void Add(this VisualElement ve, bool isClearContainer, params VisualElement[] children)
        {
            if (isClearContainer)
                ve.Clear();
            children.ForEach(child => { if (child != null) ve.Add(child); });
        }

        public static VisualElement GetRootElement(this VisualElement ve)
        {
            var p = ve.parent;
            while(p != null)
            {
                ve = p;
                p = p.parent;
            }
            return ve;
        }
    }
}
#endif