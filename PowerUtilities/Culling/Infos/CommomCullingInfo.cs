using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace PowerUtilities
{
    /// <summary>
    /// can ref gameObjects
    /// </summary>
    [Serializable]
    public class CommomCullingInfo : CullingInfo
    {
        [Header("Reactions game object")]
        public List<GameObject> contentGameObjects = new List<GameObject>();

        /// <summary>
        /// batch group id
        /// </summary>
        public int batchGroupId;
        /// <summary>
        /// visible item id in a batchGroup
        /// </summary>
        public int visibleId;
        /// <summary>
        /// Set IsVisible and trigger reactive events
        /// </summary>
        /// <param name="isVisible"></param>
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
            }
        }

        public void SetActive(CommonCullingGroupControl.ReactionType reactionType )
        {
            if (reactionType == CommonCullingGroupControl.ReactionType.None)
                return;

            foreach (var go in contentGameObjects)
            {
                if(reactionType == CommonCullingGroupControl.ReactionType.GameObject)
                    go?.SetActive(isVisible);
                else
                {
                    go.GetComponent<Renderer>()?.SetEnable(isVisible);
                }
            }
        }

        public CommomCullingInfo(Vector3 pos, float size = 2) : base(pos, size)
        {
        }
    }
}
