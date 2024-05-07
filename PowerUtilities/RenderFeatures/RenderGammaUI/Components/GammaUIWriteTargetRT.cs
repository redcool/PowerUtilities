using PowerUtilities.Features;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    public class GammaUIWriteTargetRT : MonoBehaviour
    {
        public GammaUISettingSO settingSO;
        public RenderTexture targetRT;
        public bool isNewRT;
        public int width=1024,height=1024;

        public Material mat;

        [EditorButton()]public bool isSetOnce;
        // Start is called before the first frame update
        void Start()
        {
            if (isNewRT && !targetRT)
            {
                targetRT = new RenderTexture(1024, 1024, 0);
            }

            if(mat)
                mat.SetTexture("_MainTex", targetRT);

        }

        // Update is called once per frame
        void Update()
        {
            if (isSetOnce && settingSO)
            {
                isSetOnce = false;

                settingSO.filterInfo.targetTexture = targetRT;
                settingSO.filterInfo.isWriteTargetTextureOnce = true;
            }
        }
    }
}
