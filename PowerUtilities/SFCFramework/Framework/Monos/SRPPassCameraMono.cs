namespace PowerUtilities.RenderFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Add this Mono to Camera's gameobject
    /// called events when (SFC)SRPPass execute
    /// </summary>
    public class SRPPassCameraMono : MonoBehaviour
    {
        [HelpBox]
        public string helpBox = "Add this Mono to Camera's gameobject, will call events when (SFC)SRPPass execute";
        /// <summary>
        /// called before SRPPass Execute
        /// </summary>
        public UnityEvent<SRPPass> OnPassExecuteBefore;

        /// <summary>
        /// called end of SRPPass Execute
        /// </summary>
        public UnityEvent<SRPPass> OnPassExecuteEnd;

        [Tooltip("Add default event for monos")]
        public bool isAddDefaultEvents = true;

        public void ShowSRPPassInfo(SRPPass p)
        {
            var genericPass = p as SRPPass<SRPFeature>;
            Debug.Log($"name:{p.featureName},camera:{GetComponent<Camera>()},feature:{genericPass?.Feature}");
        }


        public virtual void OnEnable()
        {
            if(isAddDefaultEvents)
            {
                AddPassExecuteBeforeEvent(DefaultExecuteBefore);
                AddPassExecuteEndEvent(DefaultExecuteEnd);
            }
        }

        public virtual void OnDisable()
        {
            if (OnPassExecuteBefore != null)
            {
                OnPassExecuteBefore.RemoveAllListeners();
                OnPassExecuteBefore = null;
            }

            if (OnPassExecuteEnd != null)
            {
                OnPassExecuteEnd.RemoveAllListeners();
                OnPassExecuteEnd = null;
            }
        }

        /// <summary>
        /// Register OnPassExecuteBefore, auto remove when Disable
        /// </summary>
        /// <param name="ev"></param>
        public void AddPassExecuteBeforeEvent(UnityAction<SRPPass> ev)
        {
            if (OnPassExecuteBefore == null)
                OnPassExecuteBefore = new();

            OnPassExecuteBefore.AddListener(ev);
        }

        /// <summary>
        ///Register OnPassExecuteEnd,
        /// </summary>
        /// <param name="ev"></param>
        public void AddPassExecuteEndEvent(UnityAction<SRPPass> ev)
        {
            if (OnPassExecuteEnd == null)
                OnPassExecuteEnd = new();

            OnPassExecuteEnd.AddListener(ev);
        }
        /// <summary>
        /// override for PassExecuteBeforeEvent
        /// </summary>
        /// <param name="pass"></param>
        public virtual void DefaultExecuteBefore(SRPPass pass)
        {

        }

        /// <summary>
        /// override for PassExecuteEndEvent
        /// </summary>
        /// <param name="pass"></param>
        public virtual void DefaultExecuteEnd(SRPPass pass)
        {

        }
    }
}
