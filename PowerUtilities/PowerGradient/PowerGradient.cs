namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [System.Serializable]
    public class PowerGradient
    {
        public Vector2 range = new Vector2(0, 1);
        public enum BlendMode { Linear, Discrete };
        public BlendMode blendMode;
        public bool randomizeColour;

        [SerializeField]
        List<ColourKey> keys = new List<ColourKey>();

        public PowerGradient()
        {
            AddKey(Color.white, range.x);
            AddKey(Color.black, range.y);
        }

        #region Utils
        public static void ReplaceGradient(object target, string prefix = "_")
        {
            var gradientType = typeof(PowerGradient);
            var copyFromMethod = gradientType.GetMethod("CopyFrom", new[] { typeof(Gradient) });

            var skyType = target.GetType();
            var q = skyType.GetFields().Where(f => f.FieldType == typeof(Gradient));
            foreach (var f in q)
            {
                var targetGradient = skyType.GetField(prefix + f.Name);
                if (targetGradient == null)
                    continue;

                var targetInstance = targetGradient.GetValue(target);
                copyFromMethod.Invoke(targetInstance, new[] { f.GetValue(target) });
            }
        }

        public void CopyFrom(PowerGradient g)
        {
            if (g == this)
                return;

            range = g.range;
            blendMode = g.blendMode;
            randomizeColour = g.randomizeColour;

            keys.Clear();
            for (int i = 0; i < g.NumKeys; i++)
            {
                var k = g.GetKey(i);
                AddKey(k.Color, k.Time);
            }
        }

        public void CopyFrom(Gradient g)
        {
            if (g.colorKeys.Length == 0)
                return;

            keys.Clear();

            var len = g.colorKeys.Length;
            for (int i = 0; i < len; i++)
            {
                var ck = g.colorKeys[i];
                var color = ck.color;
                color.a = g.Evaluate(ck.time).a;

                AddKey(ck.time, color);
            }
        }
        #endregion
        public Color Evaluate(float time)
        {
            if (keys.Count < 2)
                return Color.white;

            ColourKey keyLeft = keys[0];
            ColourKey keyRight = keys[keys.Count - 1];

            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i].Time <= time)
                {
                    keyLeft = keys[i];
                }
                if (keys[i].Time > time)
                {
                    keyRight = keys[i];
                    break;
                }
            }

            if (blendMode == BlendMode.Linear)
            {
                float blendTime = Mathf.InverseLerp(keyLeft.Time, keyRight.Time, time);
                return Color.Lerp(keyLeft.Color, keyRight.Color, blendTime);
            }
            return keyRight.Color;
        }

        public int AddKey(Color color, float time)
        {
            ColourKey newKey = new ColourKey(color, time);
            for (int i = 0; i < keys.Count; i++)
            {
                if (newKey.Time < keys[i].Time)
                {
                    keys.Insert(i, newKey);
                    return i;
                }
            }

            keys.Add(newKey);
            return keys.Count - 1;
        }

        public int SortKeys(int index)
        {
            var key = keys[index];
            keys = keys.OrderBy(k => k.Time).ToList();
            return keys.IndexOf(key);
        }

        public int AddKey(float time, Color c)
        {
            return AddKey(c, time);
        }

        public void RemoveKey(int index)
        {
            if (keys.Count > 1)
            {
                keys.RemoveAt(index);
            }
        }

        //public int UpdateKeyTime(int index, float time)
        //{
        //    Color col = keys[index].Color;
        //    RemoveKey(index);
        //    return AddKey(col, time);
        //}

        //public void UpdateKeyColor(int index, Color col)
        //{
        //    keys[index] = new ColorKey(col, keys[index].Time);
        //}

        public int NumKeys
        {
            get
            {
                return keys.Count;
            }
        }

        public ColourKey GetKey(int i)
        {
            return keys[i];
        }

        public Texture2D GetTexture(int width)
        {
            Texture2D texture = new Texture2D(width, 1);
            Color[] colors = new Color[width];
            for (int i = 0; i < width; i++)
            {
                colors[i] = Evaluate((float)i / (width-1));
            }
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }

        [System.Serializable]
        public struct ColourKey
        {
            [SerializeField]
            [ColorUsage(true, false)]
            Color colour;
            [SerializeField]
            float time;

            public ColourKey(Color color, float time)
            {
                this.colour = color;
                this.time = time;
            }

            public Color Color
            {
                get
                {
                    return colour;
                }
            }

            public float Time
            {
                get
                {
                    return time;
                }
            }
        }

    }
}