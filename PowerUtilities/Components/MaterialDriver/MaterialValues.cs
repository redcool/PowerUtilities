using System;
using UnityEngine;

namespace PowerUtilities
{
    public abstract class BaseMaterialValue
    {
        public string name;
        public bool enabled = true;

        public bool CanUpdate()
        {
            return enabled && !string.IsNullOrEmpty(name);
        }

        public abstract void UpdateValue(float elapsedTime);
    }
    public abstract class BaseMaterialValue<ValueType> : BaseMaterialValue
    {
        [NonSerialized]public ValueType value;
    }

    [Serializable]
    public class MaterialFloatValue : BaseMaterialValue<float>
    {
        public AnimationCurve floatCurve = new AnimationCurve(new[] { new Keyframe(0,1),new Keyframe(1,1)});

        public override void UpdateValue(float elapsedTime)
        {
            value = floatCurve.Evaluate(elapsedTime);
        }
    }
    [Serializable]
    public class MaterialVectorValue : BaseMaterialValue<Vector4> {
        public AnimationCurve xCurve = new AnimationCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) });
        public AnimationCurve yCurve = new AnimationCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) });
        public AnimationCurve zCurve = new AnimationCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) });
        public AnimationCurve wCurve = new AnimationCurve(new[] { new Keyframe(0, 1), new Keyframe(1, 1) });

        public override void UpdateValue(float elapsedTime)
        {
            float x = xCurve.Evaluate(elapsedTime);
            float y = yCurve.Evaluate(elapsedTime);
            float z = zCurve.Evaluate(elapsedTime);
            float w = wCurve.Evaluate(elapsedTime);
            value.Set(x, y, z, w);
        }
    }
}