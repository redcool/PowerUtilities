namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(MaterialValueDriver))]
    public class MaterialValueDriverEditor : PowerEditor<MaterialValueDriver>
    {
        public override bool NeedDrawDefaultUI()
        {
            return true;
        }
        public override void DrawInspectorUI(MaterialValueDriver inst)
        {
            if (GUILayout.Button("Reset"))
                inst.startTime = Time.time;
        }
    }
#endif

    [ExecuteInEditMode]
    public class MaterialValueDriver : MonoBehaviour
    {
        //[ListItemDraw("enabled:,enabled,name:,name,value:,value,speed:,speed,min:,minValue,max:,maxValue", "50,20,50,60,50,50,50,50,50,50,50,50")]
        public List<MaterialFloatValue> floatValues = new List<MaterialFloatValue>();
        public List<MaterialVectorValue> vectorValues = new List<MaterialVectorValue>();
        //3d
        Renderer render;
        static MaterialPropertyBlock block;

        // ui
        Graphic graphic;
        Material graphicMaterialInst;

        [HideInInspector]public float startTime;

        // Start is called before the first frame update
        void OnEnable()
        {
            render = GetComponent<Renderer>();
            if (render)
            {
                if (block == null)
                    block = new MaterialPropertyBlock();
            }

            graphic = GetComponent<Graphic>();
            if (graphic)
            {
                graphicMaterialInst = Instantiate( graphic.material);
            }

            startTime = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            if (!render && !graphicMaterialInst)
                return;

            UpdateFloats();

            UpdateVectors();
        }


        private void UpdateFloats()
        {
            foreach (var info in floatValues)
            {
                if (!info.CanUpdate())
                    continue;

                info.UpdateValue(Time.time - startTime);

                SetFloat(info);
            }
        }
        private void SetFloat(MaterialFloatValue info)
        {
            if (render)
            {
                block.SetFloat(info.name, info.value);
                render.SetPropertyBlock(block);
            }
            if (graphicMaterialInst)
            {
                graphicMaterialInst.SetFloat(info.name, info.value);
            }
        }

        private void UpdateVectors()
        {
            foreach (var info in vectorValues)
            {
                if (!info.CanUpdate())
                    continue;

                info.UpdateValue(Time.time - startTime);
                SetVector(info);
            }
        }
        private void SetVector(MaterialVectorValue info)
        {
            if (render)
            {
                block.SetVector(info.name, info.value);
                render.SetPropertyBlock(block);
            }
            if (graphicMaterialInst)
            {
                graphicMaterialInst.SetVector(info.name, info.value);
            }
        }
    }
}