//#define DEBUG_ON
#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEditorInternal;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.SceneManagement;
    using Object = UnityEngine.Object;

    /// <summary>
    /// lightmap preview enhanced
    ///     1 press control key select lightmap object
    ///     
    /// </summary>
    public class LightmapPreviewWindowEx
    {
        const string WINDOW_TOOLTIPS = @"Power Lightmap preview ,1 Press Ctrl select object";
        static bool lastControlPressed;

        static List<Renderer> rendererList = new List<Renderer>();
        static EditorWindow lightmapPreviewWin;

        [InitializeOnLoadMethod]
        static void Init()
        {
            SceneView.duringSceneGui -= OnSceneGUIUpdate;
            SceneView.duringSceneGui += OnSceneGUIUpdate;

            EditorSceneManager.activeSceneChangedInEditMode -=SceneManager_activeSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode +=SceneManager_activeSceneChanged;

        }

        static void Clear()
        {
            rendererList.Clear();
        }

        private static void SceneManager_activeSceneChanged(Scene a, Scene b)
        {
            if (a == b)
                return;

            Clear();
        }

        static void OnSceneGUIUpdate(SceneView sv)
        {
            var settings = ScriptableObjectTools.CreateGetInstance<PowerUtilSettings>();
            if (!settings.isCheckLightmapPreviewWin)
                return;

            var e = Event.current;
            var mousePos = (e.mousePosition); // in sceneView
            //Handles.BeginGUI();
            //Handles.DrawLine(mousePos, mousePos+new Vector2(100,0));
            //GUI.Button(new Rect(0,0,100,100),"test");
            //Handles.EndGUI();

            if(lightmapPreviewWin == null)
                lightmapPreviewWin = EditorWindowTools.GetWindow("LightmapPreviewWindow");

            if (lightmapPreviewWin == null 
                || !InternalEditorUtility.isApplicationActive 
                //|| EditorWindow.focusedWindow != lightmapPreviewWin
                || e.type == EventType.Layout // layout, coord error, repaint is useful
                || e.control == lastControlPressed
                )
                return;
            // save key states
            lastControlPressed = e.control;

            
            var lightmapTexture = GetCachedLightmapTexture(lightmapPreviewWin);
            if (!lightmapTexture)
                return;

            lightmapPreviewWin.titleContent.tooltip= WINDOW_TOOLTIPS;

            if (rendererList.Count == 0)
                SetupRenderers();


            //light map info
            var lightmapIndex = GetLightmapIndex(lightmapPreviewWin);
            var lightmapScaledRect = GetLightmapScaledRect(lightmapPreviewWin, lightmapTexture);


            Rect unityEditorPos = GetMainWindowPosition();

            //get mouse position on lightmap texture space
            var mousePosOnLightmap = ViewToViewPosition(lightmapPreviewWin, mousePos);
            mousePosOnLightmap -= lightmapScaledRect.position;

            var uv = mousePosOnLightmap / lightmapScaledRect.size;
            uv.y = 1f - uv.y;
#if DEBUG_ON
            Debug.Log(mousePosOnLightmap+",uv:"+uv+" ,rect: "+lightmapScaledRect +":"+e.type);
#endif
            var obj = GetLightmappedObject(lightmapIndex, uv);
            if (obj)
            {
                Selection.activeObject = obj;
            }
            lightmapPreviewWin.wantsMouseMove = true;
        }
        public static int GetLightmapIndex(object lightmapPreviewWin)
        {
            var m_LightmapIndex = lightmapPreviewWin.GetType().GetField("m_LightmapIndex", BindingFlags.NonPublic| BindingFlags.Instance);
            return (int)m_LightmapIndex?.GetValue(lightmapPreviewWin);
        }

        public static Rect GetLightmapScaledRect(object lightmapPreviewWin,Texture lightmapTex)
        {
            var type = lightmapPreviewWin.GetType();

            //var m_CachedTexture = type.GetField("m_CachedTexture", BindingFlags.Instance| BindingFlags.NonPublic);
            //var cachedTexture = m_CachedTexture.GetValue(lightmapPreviewWin);
            //var texture = (Texture)m_CachedTexture.FieldType.GetField("texture").GetValue(cachedTexture);

            var m_ZoomablePreview = type.GetField("m_ZoomablePreview", BindingFlags.NonPublic | BindingFlags.Instance);
            var zoomalbePreview = m_ZoomablePreview.GetValue(lightmapPreviewWin);

            var pShownArea = m_ZoomablePreview.FieldType.GetProperty("shownArea");
            // scale rect
            var showArea = (Rect)pShownArea?.GetValue(zoomalbePreview);

            var pDrawRect = m_ZoomablePreview.FieldType.GetProperty("drawRect");
            // --width,height 
            var drawRect = (Rect)pDrawRect?.GetValue(zoomalbePreview);

            var viewSize = drawRect.size - new Vector2(5, 5);
            var scale = Mathf.Min(viewSize.x/lightmapTex.width, viewSize.y/lightmapTex.height);

            var offsetPos = showArea.position * viewSize;

            var textureSize = new Vector2(lightmapTex.width, lightmapTex.height) * scale / showArea.size;
            return new Rect(offsetPos.x, offsetPos.y, textureSize.x, textureSize.y);
        }
        private static void SetupRenderers()
        {
            rendererList =
                //Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None) // unity_2021_3_18 later
                Object.FindObjectsOfType<Renderer>(true)
                .Where(r => r.gameObject.HasStaticFlag(StaticEditorFlags.ContributeGI) && r.lightmapIndex != -1)
                .ToList();
        }

        public static Object GetLightmappedObject(int lightmapId, Vector2 uvPosStartsAtBottom)
        {
            if (lightmapId == -1)
                return null;

#if DEBUG_ON
            rendererList.ForEach(r =>
            {
                var rect = new Rect(new Vector2(r.lightmapScaleOffset.z, r.lightmapScaleOffset.w), new Vector2(r.lightmapScaleOffset.x, r.lightmapScaleOffset.y));
                Debug.Log(rect +" : "+ uvPosStartsAtBottom+":"+ rect.Contains(uvPosStartsAtBottom));
            });
#endif
            return rendererList.Where(r => r.lightmapIndex == lightmapId
                && new Rect(new Vector2(r.lightmapScaleOffset.z, r.lightmapScaleOffset.w),new Vector2( r.lightmapScaleOffset.x, r.lightmapScaleOffset.y)).Contains(uvPosStartsAtBottom)
                ).FirstOrDefault()
                ;
        }

        public static Texture GetCachedLightmapTexture(Object lightmapPreviewWindow)
        {
            var type = lightmapPreviewWindow.GetType();
            var m_CachedTexture = type.GetField("m_CachedTexture", BindingFlags.Instance| BindingFlags.NonPublic);
            var cachedTexture = m_CachedTexture.GetValue(lightmapPreviewWindow);
            var texture = (Texture)m_CachedTexture.FieldType.GetField("texture").GetValue(cachedTexture);
            return texture;
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