using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// cullingGroup normal use
    /// </summary>
    [ExecuteInEditMode]
    public class CommonCullingGroupControl : MonoBehaviour
    {
        public Camera cam;
        public List<CommomCullingInfo> cullingInfos = new List<CommomCullingInfo>();

        BoundingSphere[] cullingSpheres;

        CullingGroup group;

        private void OnEnable()
        {
            TryInitGroup();
        }
        private void OnDisable()
        {
            if(group != null)
            {
                group.onStateChanged = null;
                group.Dispose();
                group = null;
            }
        }

        private void SetBoundingSphere()
        {
            //group.SetBoundingDistances(new[] { 10f });
            cullingSpheres = cullingInfos.Select(item => new BoundingSphere(item.pos, item.size)).ToArray();
            group.SetBoundingSpheres(cullingSpheres);
        }



        void TryInitGroup()
        {
            if (group != null)
                return;
            group = new CullingGroup();

            group.onStateChanged = OnCullingChanged;
            group.targetCamera = cam ?? Camera.main;

            SetBoundingSphere();
            SetupCullingInfosVisible();
        }

        void SetupCullingInfosVisible()
        {
            for (int i = 0; i < cullingInfos.Count; i++)
            {
                cullingInfos[i].SetIsVisible(group.IsVisible(i));
            }
        }

        void OnCullingChanged(CullingGroupEvent e)
        {
            cullingInfos[e.index].SetIsVisible(e.isVisible);
        }

        private void OnDrawGizmosSelected()
        {
            var lastColor = Gizmos.color;
            Gizmos.color = Color.green;
            foreach (var item in cullingInfos)
            {
                Gizmos.DrawWireSphere(item.pos, item.size);
            }
            Gizmos.color = lastColor;
        }
    }
}
