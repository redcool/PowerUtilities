using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class SpriteEx
    {
        /// <summary>
        /// Get sprite's in atlas,(xy:scaling,zw:offset)
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public static Vector4 GetSpriteUVScaleOffset(this Sprite sprite, bool isTextureRect)
        {
            if (!sprite)
                return default;

            var rect = isTextureRect ? sprite.textureRect : sprite.rect;
            var texWidth = (float)sprite.texture.width;
            var texHeight = (float)sprite.texture.height;

            var uvStart = new Vector2(rect.x / texWidth, rect.y / texHeight);
            var uvRange = new Vector2(rect.width / texWidth, rect.height / texHeight);
            var spriteUV = new Vector4(uvRange.x, uvRange.y, uvStart.x, uvStart.y);
            return spriteUV;
        }
    }
}
