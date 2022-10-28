using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PowerUtilities
{
    [ExecuteInEditMode]
    public class UIMaterialPropCodeTemplate : MonoBehaviour
    {
        public Color color;

        Material mat;
        MaskableGraphic graph;

        private void Start()
        {
            graph = GetComponent<MaskableGraphic>();
            mat = graph.material;
        }

        private void Update()
        {
            if(!mat)
            {
                enabled=false;
                return;
            }

            UpdateMaterial();
            graph.SetMaterialDirty();
        }

        void UpdateMaterial()
        {
            mat.SetColor("_Color", color);
        }
    }
}
