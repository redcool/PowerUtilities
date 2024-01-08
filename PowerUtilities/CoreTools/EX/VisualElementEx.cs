using UnityEngine.UIElements;

namespace PowerUtilities.UIElements
{
    public static class VisualElementEx
    {
        /// <summary>
        /// Add children foreach
        /// </summary>
        /// <param name="ve"></param>
        /// <param name="isClearContainer"></param>
        /// <param name="children"></param>
        public static void Add(this VisualElement ve, bool isClearContainer, params VisualElement[] children)
        {
            if (isClearContainer)
                ve.Clear();
            children.ForEach(child => { if (child != null) ve.Add(child); });
        }

        /// <summary>
        /// Get parent element upforwards when its parent is null
        /// </summary>
        /// <param name="ve"></param>
        /// <returns></returns>
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