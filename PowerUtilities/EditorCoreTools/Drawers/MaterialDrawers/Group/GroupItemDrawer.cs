#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    /// <summary>
    /// Material's Group Item Attribute
    /// </summary>
    public class GroupItemDrawer : BaseGroupItemDrawer
    {
        public GroupItemDrawer(string groupName) : base(groupName, null) { }
        public GroupItemDrawer(string groupName,string tooltip) : base(groupName,tooltip) { }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            //var shader = ((Material)editor.target).shader;
            //var propId = shader.FindPropertyIndex(prop.name);
            //var attrs = shader.GetPropertyAttributes(propId);

            //foreach (var item in attrs)
            //{
            //    Debug.Log(item);
            //}
            MaterialEditorEx.ShaderProperty(editor, position, prop, label, 0, false);
        }

    }

}
#endif