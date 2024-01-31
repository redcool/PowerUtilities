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
        [Header("game object")]
        public List<GameObject> contentGameObjects = new List<GameObject>();

        [Header("Reactions")]
        [Tooltip("when isVisible change, update gameobjects's active")]
        public bool isSetContentGameObjectsActive = true;

        public UnityEvent triggerEvent;

        bool lastIsVisible;

        /// <summary>
        /// Set IsVisible and trigger reactive events
        /// </summary>
        /// <param name="isVisible"></param>
        public void SetIsVisible(bool isVisible)
        {
            this.isVisible = isVisible;
            if(lastIsVisible != isVisible)
            {
                SetGameObjectsActive();
                TriggerEvents();
                lastIsVisible = isVisible;
            }
        }

        private void TriggerEvents()
        {
            if(triggerEvent != null)
            {
                triggerEvent.Invoke();
            }
        }

        private void SetGameObjectsActive()
        {
            foreach (var go in contentGameObjects)
            {
                if (go)
                    go.SetActive(isVisible);
            }
        }

        public CommomCullingInfo(Vector3 pos, float size = 2) : base(pos, size)
        {
        }
    }
}
