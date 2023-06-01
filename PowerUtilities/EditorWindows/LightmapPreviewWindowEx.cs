#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.VisualScripting;
    using UnityEditor;
    using UnityEngine;

    public class LightmapPreviewWindowEx
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
            SceneView.duringSceneGui -= OnEditorUpdate;
            SceneView.duringSceneGui += OnEditorUpdate;
        }

        static void OnEditorUpdate(SceneView sv)
        {
            //sv.wantsMouseEnterLeaveWindow = true;
            //sv.wantsLessLayoutEvents = true;
            //sv.wantsMouseMove = true;

            var lightmapPreviewWin = EditorWindowTools.GetWindow("LightmapPreviewWindow");
            if (lightmapPreviewWin == null)
                return;

            //if (EditorWindowTools.IsFocused(lightmapPreviewWin))
            //    return;

            var e = Event.current;
            var mousePos = (e.mousePosition); // in sceneView

            Rect unityEditorPos = GetMainWindowPosition();

            var mousePosLightmapPreview = ViewToViewPosition(lightmapPreviewWin, mousePos);
            //var screenPos = GUIUtility.GUIToScreenPoint(mousePos);
            //var lightmapWindowPos = screenPos - lightmapPreviewWin.position.position;

            //Debug.Log(mousePosLightmapPreview);
        }

        public static Rect GetMainWindowPosition()
        {
            var unityEditorRect = EditorGUIUtility.GetMainWindowPosition();
            unityEditorRect.y -= 50;
            return unityEditorRect;
        }

        public static Vector2 ViewToViewPosition( EditorWindow curWindow,Vector2 viewPosOtherWindow)
        {
            var screenPos = GUIUtility.GUIToScreenPoint(viewPosOtherWindow);
            return screenPos - curWindow.position.position;
        }
    }
}
#endif