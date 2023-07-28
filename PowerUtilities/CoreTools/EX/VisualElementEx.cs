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
    }
}
#endif