﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public class BRGBatchBlock : MonoBehaviour
    {
        public int instId;
        public BRGBatch brgBatch;

        public Color color;
        Color lastColor;

        public void Update()
        {
            if (transform.hasChanged)
            {
                var objectToWorld = transform.localToWorldMatrix.ToFloat3x4();
                var worldToObject = transform.worldToLocalMatrix.ToFloat3x4();
                brgBatch.FillData(objectToWorld.ToColumnArray(), instId, 0);
                brgBatch.FillData(worldToObject.ToColumnArray(), instId, 1);
            }
            if (CompareTools.CompareAndSet(ref lastColor, ref color))
            {
                brgBatch.FillData(color.ToArray(), instId, 3);
            }
        }
    }
}
