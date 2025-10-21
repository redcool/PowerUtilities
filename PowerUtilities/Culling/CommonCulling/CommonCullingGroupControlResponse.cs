using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    public class CommonCullingGroupControlResponse : MonoBehaviour
    {
        public void OnVisibleChanged(CommomCullingInfo info)
        {
            if (info != null)
                foreach (var item in info.contentGameObjects)
                {
                    item.SetActive(info.isVisible);
                }
        }

        public void OnVisibleChanged(CullingGroupEvent e)
        {
            Debug.Log($"{e.index},visible:{e.isVisible},distanceBands:{e.currentDistance}");
        }
    }
}
