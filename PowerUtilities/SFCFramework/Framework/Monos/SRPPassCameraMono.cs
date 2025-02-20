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
        [SerializeField]
        string helpBox = "Add this Mono to Camera's gameobject, will call events when (SFC)SRPPass execute";
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

        [Header("Pass match")]
        public string passName = "";
        public StringEx.NameMatchMode nameMathMode = StringEx.NameMatchMode.Full;

        public void ShowSRPPassInfo(SRPPass p)
        {
            if (!IsPassNameMatch(p))
            {
                return;
            }

            Debug.Log($"name:{p.featureName},camera:{GetComponent<Camera>()}");
        }


        public virtual void OnEnable()
        {
            if(isAddDefaultEvents)
            {
                OnPassExecuteBefore.AddListener(DefaultExecuteBefore);
                OnPassExecuteEnd.AddListener(DefaultExecuteEnd);
            }
        }

        public virtual void OnDisable()
        {
            OnPassExecuteBefore.RemoveAllListeners();
            OnPassExecuteEnd.RemoveAllListeners();
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

        public bool IsPassNameMatch(SRPPass pass) =>
            string.IsNullOrEmpty(passName) ? 
            true : 
            pass.featureName.IsMatch(passName,nameMathMode);
    }
}
