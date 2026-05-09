using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SetMaterialVisualElementMat : MonoBehaviour
{
    public UIDocument doc;
    [Header("Material per visualElement")]
    public string[] visualElementNames;
    public Material[] materials;

    [Header("Default material")]
    [Tooltip("all unnamed MaterialVisualElement use this")]
    public Material defaultMaterial;

    // Start is called before the first frame update
    void OnEnable()
    {
        doc = GetComponent<UIDocument>();

        if (defaultMaterial)
        {
            var list = doc.rootVisualElement.Query<MaterialVisualElement>().ToList();
            foreach (var v in list)
            {
                v.customMaterial = defaultMaterial;
            }
        }

        for (int i = 0; i < materials.Length; i++)
        {
            var mat = materials[i];
            if (!mat)
                continue;

            var name = "";
            if(visualElementNames.Length > i)
                name = visualElementNames[i];

            var visualElement = doc.rootVisualElement.Q<MaterialVisualElement>(name);
            visualElement.customMaterial = mat;
        }
    }

}
