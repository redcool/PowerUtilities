using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameUtilsFramework;
using System;



#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(EquipmentPartControl))]
public class EquipmentPartChangeEditor : Editor
{
    public (string ,bool) showEquipPartFold=(nameof(showEquipPartFold),false);


    void DrawEquipParts(EquipmentPartControl inst)
    {

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var inst = (EquipmentPartControl)target;

        EditorGUITools.BeginVerticalBox(() =>
        {
            if (!inst.boneRoot)
            {
                EditorGUITools.DrawColorLabel("* Need assign Bone Root.", Color.red);
            }

            EditorGUITools.BeginDisableGroup(!inst.boneRoot, () =>
            {
                if (GUILayout.Button("Equip Part"))
                {
                    inst.ChangePart(inst.overridePart);
                }

            });

            EditorGUITools.DrawFoldContent(ref showEquipPartFold, () =>
            {
                foreach (var item in inst.partDict)
                {
                    EditorGUILayout.PrefixLabel(item.Key.ToString());
                    EditorGUILayout.ObjectField(item.Value.skinned, typeof(SkinnedMeshRenderer),true);
                }
            });

        });

    }
}
#endif

public class EquipmentPartControl : MonoBehaviour
{
    public Dictionary<CharacterPart, EquipmentPart> partDict = new Dictionary<CharacterPart, EquipmentPart>();

    public Transform boneRoot;

    //[Header("Male Part Mesh")]
    public EquipmentPart overridePart;

    private void Awake()
    {
        InitEquipmentParts();
    }

    private void InitEquipmentParts()
    {
        if(partDict.Count > 0)
            partDict.Clear();

        var partNames = Enum.GetValues(typeof(CharacterPart));
        foreach (CharacterPart partName in partNames)
        {
            partDict.Add(partName, new EquipmentPart { part = partName });
        }
    }

    private void OnDestroy()
    {
        partDict.Clear();
    }

    void Start()
    {
        ChangePart(overridePart);
    }

    public void ChangePart(EquipmentPart overridePart)
    {
        if (overridePart == null)
            return;

        if (partDict.Count == 0)
            InitEquipmentParts();

        if (partDict.TryGetValue(overridePart.part, out var equipPart))
        {
            equipPart.UnequipAndDestroy();
        }

        equipPart.Equip(transform, overridePart, boneRoot);

    }
}
