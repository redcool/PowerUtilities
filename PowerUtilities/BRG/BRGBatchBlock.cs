using PowerUtilities.RenderFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    [ExecuteInEditMode]
    /// <summary>
    /// Update instanceBuffer per renderer , Like MaterialPropertyBlock
    /// </summary>
    public class BRGBatchBlock : MonoBehaviour
    {
        public int instId;
        public BRGBatch brgBatch;

        [EditorSettingSO]
        public BRGMaterialInfo brgMaterialInfo;

        [Header("Update Block")]
        [Tooltip("material property name")]
        public string propName;
        [Tooltip("property type value")]
        public ShaderPropertyType propType = ShaderPropertyType.Float;

        [Tooltip("property float value")]
        public float floatValue;
        [Tooltip("property vector value")]
        public Vector4 vectorValue;
        [Tooltip("property color value")]
        public Color colorValue;

        [EditorButton(onClickCall = nameof(UpdateBlock))]
        public bool isUpdateBlock;

        public void Update_()
        {
            if (brgBatch == null || !brgBatch.instanceBuffer.IsValidSafe())
                return;

            if (transform.hasChanged)
            {
                var objectToWorld = transform.localToWorldMatrix.ToFloat3x4();
                var worldToObject = transform.worldToLocalMatrix.ToFloat3x4();

                brgBatch.instanceBuffer.SetData(objectToWorld, 0, BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools.unity_WorldToObject, 12, instId, 1));
                brgBatch.instanceBuffer.SetData(objectToWorld, 0, BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools.unity_ObjectToWorld, 12, instId, 1));
            }
        }

        public void UpdateBlock()
        {
            if (string.IsNullOrEmpty(propName) || brgBatch == null || !brgMaterialInfo)
                return;

            float[] floats = default;
            if (propType == ShaderPropertyType.Float)
                floats = new float[] { floatValue };
            else
            {
                floats = propType == ShaderPropertyType.Color ? colorValue.ToArray() : vectorValue.ToArray();
            }

            brgMaterialInfo.FillMaterialData(brgBatch, instId, propName, floats);
        }
    }
}
