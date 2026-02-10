#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    [CustomEditor(typeof(RenderTexture))]
    public class RenderTextureInspectorEx : BaseEditorEx
    {

        static readonly GUIContent 
            saveOptionGUI = new GUIContent("Save Options","save render texture to file"),
            saveTexGUI = new GUIContent("Save","save to file");
        public bool istFoldSave;
        public TextureEncodeType texEncodeType;
        public override string GetDefaultInspectorTypeName()
        => "UnityEditor.RenderTextureEditor";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            istFoldSave = EditorGUILayout.Foldout(istFoldSave, saveOptionGUI,true);
            if (istFoldSave)
                DrawSaveFile();
            
        }

        private void DrawSaveFile()
        {
            EditorGUILayout.BeginHorizontal();
            texEncodeType = (TextureEncodeType)EditorGUILayout.EnumPopup(texEncodeType);
            var isSaveButtonDown = GUILayout.Button(saveTexGUI);
            EditorGUILayout.EndHorizontal();

            if (isSaveButtonDown)
            {
                var inst = target as RenderTexture;

                var assetFolder = AssetDatabaseTools.GetAssetFolder(inst);
                if (string.IsNullOrEmpty(assetFolder))
                    assetFolder = "Assets";

                inst.SaveRenderTexture(texEncodeType, assetFolder, inst.name, true);
                AssetDatabaseTools.SaveRefresh();

            }
        }
    }
}
#endif