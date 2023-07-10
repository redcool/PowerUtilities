#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEditor;
    using UnityEngine;
    
    public class GroupMinMaxSliderDrawer : BaseGroupItemDrawer
    {
        public GroupMinMaxSliderDrawer() : this("", "") { }
        public GroupMinMaxSliderDrawer(string groupName):this(groupName,""){}
        public GroupMinMaxSliderDrawer(string groupName, string tooltip) : base(groupName, tooltip)
        { 
            
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;

            Vector4 value = prop.vectorValue;

            if (value.z.Equals(value.w))
                value += new Vector4(0, 0, 0, 0.1f);
            if (value.z > value.w)
            {
                value.z = value.w;
                value.w = value.z;
            }
            if (value.x > value.y)
            {
                value.x = value.y;
                value.y = value.x;
            }
            value.x = Mathf.Max(value.x, value.z);
            value.y = Mathf.Min(value.y, value.w);
            
            //Slider
            Rect rangeRect = new Rect(position)
            {
                width = position.width - 110
            };

            //min
            Rect minRect = new Rect(position)
            {
                x = position.xMax - 105f,
                width = 50,
            };
            //max
            Rect maxRect = new Rect(position)
            {
                x = position.xMax - 50f,
                width = 50,
            };


            EditorGUI.MinMaxSlider(rangeRect, label, ref value.x, ref value.y, value.z, value.w);
            
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            value.x = EditorGUI.FloatField(minRect, value.x);
            value.y = EditorGUI.FloatField(maxRect,value.y);
            EditorGUI.indentLevel = indentLevel;

            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = value;
            }
        }
    }
}
#endif