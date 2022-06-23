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
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Equip Part"))
        {
            var partControl = (EquipmentPartControl)target;
            partControl.ChangePart(partControl.overridePart);
        }
    }
}
#endif

public class EquipmentPartControl : MonoBehaviour
{
    Dictionary<CharacterPart, EquipmentPart> partDict = new Dictionary<CharacterPart, EquipmentPart>();

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
