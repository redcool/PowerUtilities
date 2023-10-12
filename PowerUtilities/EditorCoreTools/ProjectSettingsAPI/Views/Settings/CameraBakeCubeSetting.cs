namespace PowerUtilities {
#if UNITY_EDITOR
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(CameraBakeCubeSetting))]
    public class CameraBakeCubeSettingEditor : PowerEditor<CameraBakeCubeSetting>
    {        public override void DrawInspectorUI(CameraBakeCubeSetting inst)
        {
            inst.targetTr = (Transform)EditorGUILayout.ObjectField("target camera", inst.targetTr, typeof(Transform), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));

            inst.savePathObj = EditorGUILayout.ObjectField("save path",inst.savePathObj, typeof(Object), false);


            if (GUILayout.Button("BakeCube"))
            {
                if (inst.savePathObj)               {
                    inst.savePath = PathTools.GetAssetDir(AssetDatabase.GetAssetPath(inst.savePathObj));
                }

                BakeCube(inst);
            }
        }

        public void BakeCube(CameraBakeCubeSetting inst)
        {
            if (inst.targetTr == null || string.IsNullOrEmpty(inst.savePath))
                return;

            var cubemap = new Cubemap((int)inst.width, TextureFormat.RGB24, true);

            var go = new GameObject("render cube");
            var cam = go.AddComponent<Camera>();
            cam.transform.position = inst.targetTr.position;
            cam.transform.rotation = Quaternion.identity;
            cam.RenderToCubemap(cubemap);

            DestroyImmediate(go);

            var filePath = $"{inst.savePath}/{inst.targetTr.name}.asset";
            AssetDatabase.DeleteAsset(filePath);
            AssetDatabase.CreateAsset(cubemap, filePath);
        }
    }

    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS+"/Tools/BakeCube")]
    [SOAssetPath(nameof(CameraBakeCubeSetting))]
    public class CameraBakeCubeSetting : ScriptableObject
    {
        public Transform targetTr;

        public TextureResolution width = TextureResolution.x256;
        public string savePath;
        public Object savePathObj;
    }
#endif
}