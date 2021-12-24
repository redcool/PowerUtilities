using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class GraphicsEx
    {
        /// <summary>
        /// texture(src) blit to texture dest, ignore width,height matches.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public static void Blit(Texture src, Texture2D dest, int destX = 0, int destY = 0, int destBlockWidth = -1, int destBlockHeight = -1)
        {
            if (!src || !dest)
                return;

            var width = destBlockWidth < 1 ? dest.width : destBlockWidth;
            var height = destBlockHeight < 1 ? dest.height : destBlockHeight;

            var rt = RenderTexture.GetTemporary(width, height, 0);

            Graphics.Blit(src, rt);

            //Graphics.SetRenderTarget(rt);
            dest.ReadPixels(new Rect(0, 0, width, height), destX, destY);
            Graphics.SetRenderTarget(null);

            RenderTexture.ReleaseTemporary(rt);
        }
    }
}
