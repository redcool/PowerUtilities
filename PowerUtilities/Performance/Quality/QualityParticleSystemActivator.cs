using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public class QualityParticleSystemActivator : QualityGameObjectActivator
    {

        public override void UpdateWithQuality(QualitySettingEx.QualityInfo info)
        {
            var children = GetComponentsInChildren<ParticleSystem>()
                .Skip(info.componentCount);

            foreach (var child in children)
            {
                if (updateMode == UpdateMode.DestroyGameObject)
                {
                    child.gameObject.Destroy();
                }
                else
                {
                    var emission = child.emission;
                    emission.enabled = false;
                }
            }
        }

    }
}
