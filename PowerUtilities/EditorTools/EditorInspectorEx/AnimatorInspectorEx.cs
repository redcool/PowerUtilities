#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace PowerUtilities
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Animator))]
    public class AnimatorInspectorEx : BaseEditorEx
    {
        public override string GetDefaultInspectorTypeName() => "UnityEditor.AnimatorInspector";

        public bool isBakeAnimTextureFolded;

        public bool isAnimTextureSupported;
        public override void OnEnable()
        {
            base.OnEnable();
            isAnimTextureSupported = AssetDatabaseTools.FindAssetPathAndLoad<TextAsset>(out var _, "AnimTextureBakeSettings", ".cs");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var inst= (target as Animator);

            if (isAnimTextureSupported && GUILayout.Button("Add BakeAnimTexture"))
            {
                var type = ReflectionTools.GetAppDomainTypes<MonoBehaviour>(type => type.FullName == "AnimTexture.BakeAnimTexture")
                    .FirstOrDefault();
                if (type != null)
                    inst.gameObject.GetOrAddComponent(type);
            }
        }

    }
}
#endif