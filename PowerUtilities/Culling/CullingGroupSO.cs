using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PowerUtilities
{
    /// <summary>
    /// scene culling group profile
    /// one profile per scene
    /// </summary>
    [Serializable]
    public class CullingGroupSO : ScriptableObject
    {
        [ListItemDraw("p:,pos,s:,size,v:,isVisible", "20,200,20,30,20,30")]
        public List<InstancedGroupCullingInfo> cullingInfos = new List<InstancedGroupCullingInfo>();

        /// <summary>
        /// update group.originalTransformsGroupList instances visible
        /// </summary>
        /// <param name="groupList"></param>
        public void SetupCullingGroupSO(List<InstancedGroupInfo> groupList)
        {
            groupList.ForEach((group, groupId) =>
            {
                group.originalTransformsGroupList.ForEach((transformGroup, transformGroupId) =>
                {
                    for (int i = 0; i < transformGroup.transforms.Count; i++)
                    {
                        var transform = transformGroup.transforms[i];
                        var boundsSphereRadius = transformGroup.boundsSphereRadiusList[i];

                        cullingInfos.Add(new InstancedGroupCullingInfo(transform.GetColumn(3), boundsSphereRadius)
                        {
                            groupId = groupId,
                            transformGroupId = transformGroupId,
                            transformId = i
                        });

                    }
                });
            });
        }

    }
}
