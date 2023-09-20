#if UNITY_EDITOR
using JetBrains.Annotations;
using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraBakeCubeSetting))]
public class CameraBakeCubeSettingEditor : PowerEditor<CameraBakeCubeSetting>
{

    public override void DrawInspectorUI(CameraBakeCubeSetting inst)
    {
        inst.targetTr = (Transform)EditorGUILayout.ObjectField("target camera",inst.targetTr, typeof(Transform), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
        if (GUILayout.Button("BakeCube"))
        {
            
        }
    }
}

[ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS+"/Tools/BakeCube")]
[SOAssetPath(nameof(CameraBakeCubeSetting))]
public class CameraBakeCubeSetting : ScriptableObject
{
    public Transform targetTr;

    public TextureResolution width = TextureResolution.x256;
}
#endif