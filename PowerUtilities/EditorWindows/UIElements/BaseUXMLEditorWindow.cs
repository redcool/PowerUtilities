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

        /// <summary>
        /// use CloneTree(container) or othher
        /// </summary>
        public bool IsReplaceRootContainer = false;

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

            if (!IsReplaceRootContainer)
            {
                treeInstance = treeAsset.CloneTree();
                rootVisualElement.Add(treeInstance);
            }
            else
            {
                treeAsset.CloneTree(rootVisualElement);

            }

            var uss = treeStyleSheet ?? lazyDefaultStyleSheet.Value;
            rootVisualElement.styleSheets.Add(uss);

            // event
            if (eventInstance == null)
            {
                eventInstance = this as IUIElementEvent;
            }

            AddTreeEvents();
        }

        /// <summary>
        /// set eventMono or(eventInstance) then call this
        /// </summary>
        public void AddTreeEvents()
        {
            if(eventMono)
                eventInstance = Activator.CreateInstance(eventMono.GetClass()) as IUIElementEvent;



            if (eventInstance != null)
            {
                eventInstance.AddEvent(rootVisualElement);
            }

        }

    }
}
#endif