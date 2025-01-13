using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Project/QualitySettingEx")]
    [SOAssetPath("Assets/PowerUtilities/QualitySettingEx.asset")]
    public class QualitySettingEx : ScriptableObject
    {
        [Serializable]
        public class QualityInfo
        {
            [EnumFlags(isFlags = false, type = typeof(QualitySettings), memberName = "names")]
            [Tooltip("quality level")]
            public int qualityLevel = 3;

            [Tooltip("component count(like :particle System Count),exceed is disabled from top to down")]
            [Min(0)]
            public int componentCount=3;
        }

        [HelpBox]
        public string helpBox = "set project's quality setting ex";

        [ListItemDraw("qLv:,qualityLevel,CompCount:,componentCount", "100,100,100,50")]
        public List<QualityInfo> infos = new();

        private void Awake()
        {
            if (infos.Count > 0)
                return;

            for (var i = 0; i < QualitySettings.count; i++)
            {
                infos.Add(new QualityInfo { qualityLevel = i });
            }
        }

        public QualityInfo GetInfo(int qLevel)
        {
            return infos.Find((QualityInfo info )=> info.qualityLevel == qLevel);
        }

    }
}
