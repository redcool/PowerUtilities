using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    public class CommonCullingResponse : MonoBehaviour
    {
        public void OnVisibleChanged(CommomCullingInfo info)
        {
            if(info != null)
                foreach (var item in info.contentGameObjects)
                {
                    item.SetActive(info.isVisible);
                }
        }
    }
}
