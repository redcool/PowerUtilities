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

        [Header("Reactions")]
        [Tooltip("when isVisible change, update gameobjects's active")]
        public bool isSetContentGameObjectsActive = true;

        public UnityEvent<CommomCullingInfo> OnVisibleChanged;
        //[SerializeField]
        bool lastIsVisible;
        
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
                if(CompareTools.CompareAndSet(ref lastIsVisible, ref isVisible))
                {
                    if(isSetContentGameObjectsActive)
                        SetGameObjectsActive();

                    TriggerEvents();
                }
            }
        }

        private void TriggerEvents()
        {
            OnVisibleChanged?.Invoke(this);
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
