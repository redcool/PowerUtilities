using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class SpriteTools
    {
        /// <summary>
        /// scale sprite transform's localScale to spriteTargetSize
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="spriteTr"></param>
        /// <param name="spriteTargetSize"></param>
        public static void SetSpriteScale(this Sprite sprite, Transform spriteTr, Vector2 spriteTargetSize)
        {
            // bounds{x,y} = sprite.rect.zw/sprite.pixelsPerUnit
            var rect = sprite.rect;

            var spriteScale = spriteTargetSize / new Vector2(rect.width, rect.height);
            spriteTr.localScale = new Vector3(spriteScale.x, spriteScale.y, spriteTr.localScale.z);
        }
    }
}
