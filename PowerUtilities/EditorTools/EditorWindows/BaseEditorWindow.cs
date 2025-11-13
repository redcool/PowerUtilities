#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// Base EditorWindow
    /// Show scriptObj (ObjectField)
    /// </summary>
    public class BaseEditorWindow : EditorWindow
    {
        public Object scriptObj;

        public virtual void OnEnable()
        {
            scriptObj = AssetDatabaseTools.FindAssetPathAndLoad<Object>(out _, GetType().Name);    
        }

        public virtual void OnGUI()
        {
            EditorGUITools.DrawDisableScope(true, () =>
            {
                EditorGUILayout.ObjectField(GUIContentEx.TempContent("Script : ", "Script draw gui "), scriptObj, typeof(Object), false);
            });

        }
    }

}
#endif