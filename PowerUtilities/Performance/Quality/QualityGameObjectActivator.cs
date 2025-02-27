using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Control children gameobject' count
    /// config (ProjectSettings/PowerUtils/Project/QualitySettingEx)
    /// </summary>
    public class QualityGameObjectActivator : MonoBehaviour
    {
        public enum UpdateMode
        {
            DestroyGameObject, Disable
        }
        [Header("Default Setting")]
        [LoadAsset("Assets/PowerUtilities/QualitySettingEx.asset")]
        public QualitySettingEx defaultSettingSO;

        [Header("Override Setting")]
        [Tooltip("override default")]
        public bool isUseOverrideSetting;
        [ListItemDraw("qLv:,qualityLevel,CompCount:,componentCount", "100,100,100,50")]
        public List<QualitySettingEx.QualityInfo> overrideInfos = new();

        [Header("Options")]
        public UpdateMode updateMode;

        private void OnEnable()
        {
            if (!defaultSettingSO)
                return;

            var qlevel = QualitySettings.GetQualityLevel();
            var info = defaultSettingSO.GetInfo(qlevel);

            if (isUseOverrideSetting)
            {
                info = overrideInfos.Find(item => item.qualityLevel == qlevel);
            }

            if (info == null)
                return;

            UpdateWithQuality(info);
        }


        public virtual void UpdateWithQuality(QualitySettingEx.QualityInfo info)
        {
            var children = GetComponentsInChildren<Transform>(true).Skip(info.componentCount+1);

            foreach (var child in children)
            {
                if (updateMode == UpdateMode.DestroyGameObject)
                {
                    child.gameObject.Destroy();
                }
                else
                    child.gameObject.SetActive(false);
            }
        }
    }
}
