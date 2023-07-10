#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEditor;
    using UnityEngine;
    
    public class GroupVectorDrawer : BaseGroupItemDrawer
    {
        
        private bool m_IsEditor;
        private bool m_IsStartEditor = true;
        private Quaternion m_Rotation = Quaternion.identity;
        private GameObject m_SelectGameObj;
        private MaterialProperty m_Property;

        public GroupVectorDrawer() : this("", "") { }
        public GroupVectorDrawer(string groupName):this(groupName,""){}
        public GroupVectorDrawer(string groupName, string tooltip) : base(groupName, tooltip)
        { 
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            
            m_Property = prop;
            EditorGUI.showMixedValue = prop.hasMixedValue;
            
            Color oldColor = GUI.color;
            if (m_IsEditor)
                GUI.color = Color.green;
            
            Rect vecRect = new Rect(position)
            {
                width = position.width - 78f
            };

            Vector3 value = new Vector3(prop.vectorValue.x, prop.vectorValue.y, prop.vectorValue.z);
            value = EditorGUI.Vector3Field(vecRect, label, value);
            
            Rect butRect = new Rect(position)
            {
                x = position.xMax - 63f,
                y = (position.height > 20) ? position.y + 20 : position.y,
                width = 60f,
                height = EditorGUIUtility.singleLineHeight
            };
            butRect.y -= 20f;
            m_IsEditor = GUI.Toggle(butRect, m_IsEditor, "Set", "Button");
            
            if (m_IsEditor && m_IsStartEditor)
                CreateHandle(value);
            else if (!m_IsEditor && !m_IsStartEditor)
                DeleteHandle();

            GUI.color = oldColor;
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = new Vector4(value.x, value.y, value.z, prop.vectorValue.w);
            }
        }
        
        private void CreateHandle(Vector3 dir)
        {
            Tools.current = Tool.None;
            m_SelectGameObj = Selection.activeGameObject;
            m_Rotation = Quaternion.FromToRotation(Vector3.forward, dir);
            SceneView.duringSceneGui += HandleDraw;
            SceneView.RepaintAll();
            m_IsStartEditor = false;
        }
        
        private void DeleteHandle()
        {
            SceneView.duringSceneGui -= HandleDraw;
            SceneView.RepaintAll();
            m_SelectGameObj = null;
            m_IsStartEditor = true;
        }
        
        private void HandleDraw(SceneView sceneView)
        {
            if (m_SelectGameObj == null || m_SelectGameObj != Selection.activeGameObject)
            {
                DeleteHandle();
                m_IsEditor = false;
                return;
            }

            Vector3 pos = m_SelectGameObj.transform.position;
            
            m_Rotation = Handles.RotationHandle(m_Rotation, pos);
            
            Vector3 newDir = m_Rotation * Vector3.forward;

            m_Property.vectorValue = new Vector4(newDir.x, newDir.y, newDir.z, m_Property.vectorValue.w);
            
            Handles.color = Color.green;
            float size = HandleUtility.GetHandleSize(pos);
            Handles.ConeHandleCap(0, pos, m_Rotation.normalized, size, EventType.Repaint);
        }
    }
}
#endif