#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PowerUtilities
{
    /// <summary>
    /// abstract uxml editor window
    /// </summary>
    public abstract class BaseUXMLEditorWindow : EditorWindow
    {
        public const string ROOT_MENU = "PowerUtilities/Window";

        // uxml
        public VisualTreeAsset treeAsset;
        protected VisualElement treeInstance;

        // events
        /// <summary>
        /// create eventInstance when it is null
        /// </summary>
        public MonoScript eventMono;
        /// <summary>
        /// set this, wont create eventMono's instance
        /// </summary>
        public IUIElementEvent eventInstance;

        /// <summary>
        /// uss
        /// use defaultStyleSheet if not set
        /// </summary>
        public StyleSheet treeStyleSheet;
        public Lazy<StyleSheet> lazyDefaultStyleSheet = new Lazy<StyleSheet>(
            () => AssetDatabaseTools.FindAssetPathAndLoad<StyleSheet>(out var _, "DefaultStyles", "uss")
            );

        /// <summary>
        /// create ui elements use (treeAsset),
        /// 
        /// set treeAsset then call CreateGUI
        /// 
        /// </summary>
        public virtual void CreateGUI()
        {
            rootVisualElement.Clear();

            if (!treeAsset)
                return;

            treeInstance = treeAsset.CloneTree();
            rootVisualElement.Add(treeInstance);

            var uss = treeStyleSheet ?? lazyDefaultStyleSheet.Value;
            rootVisualElement.styleSheets.Add(uss);

            //
            AddTreeEvents();
        }

        /// <summary>
        /// set eventMono or(eventInstance) then call this
        /// </summary>
        public void AddTreeEvents()
        {
            if (treeInstance == null)
                return;

            if(eventMono)
                eventInstance = Activator.CreateInstance(eventMono.GetClass()) as IUIElementEvent;

            if (eventInstance != null)
            {
                eventInstance.AddEvent(treeInstance);
            }

        }

    }
}
#endif