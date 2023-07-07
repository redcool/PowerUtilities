#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System;

namespace PowerUtilities
{
    /// <summary>
    /// 管理材质上代码绘制的属性
    /// 这是属性需要定义在Layout.txt里,通过代码来控制材质的状态.
    /// </summary>
    public class MaterialCodeProps
    {
        public enum CodePropNames
        {
            _PresetBlendMode,
            _RenderQueue,
            _ToggleGroups,
            _BakedEmission,
            _Version,
        }

        Dictionary<string, bool> materialCodePropDict = new Dictionary<string, bool>();
        public void Clear()
        {
            var keys = materialCodePropDict.Keys.ToList();
            foreach (var item in keys)
            {
                materialCodePropDict[item] = false;
            }
        }

        private MaterialCodeProps()
        {
            var names = Enum.GetNames(typeof(CodePropNames));
            foreach (var item in names)
            {
                materialCodePropDict[item] = false;
            }
        }

        public static MaterialCodeProps Instance { get; } = new MaterialCodeProps();

        public void InitMaterialCodeVars(string propName)
        {
            if (materialCodePropDict.ContainsKey(propName))
            {
                materialCodePropDict[propName] = true;
            }
        }

        public bool IsPropExists(CodePropNames propName)
        {
            var key = propName.ToString();
            materialCodePropDict.TryGetValue(key, out var isExist);
            return isExist;
        }
    }
}


#endif