﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class TextureTools
    {
        /// <summary>
        /// Split Textures by resolution
        /// </summary>
        /// <param name="textures"></param>
        /// <param name="resolution"></param>
        /// <param name="countInRow"></param>
        /// <returns></returns>
        public static List<Texture2D> SplitTextures(Texture2D[] textures, TextureResolution resolution, ref int countInRow, Action<float> onProgress, bool isHeightmap, TextureFormat textureFormat = TextureFormat.R8, bool isMipChain = false, bool isLinear = true)
        {
            if (textures == null || textures.Length == 0)
                return null;

            var res = (int)resolution;
            var textureList = new List<Texture2D>();
            countInRow = 0;

            for (int i = 0; i < textures.Length; i++)
            {
                var tex = textures[i];
                if (tex == null)
                    continue;


                var countInRowTile = tex.width / res;
                countInRowTile = Mathf.Max(1, countInRowTile);

                countInRow += countInRowTile;

                if (tex.width > res)
                {
                    var texs = tex.SplitTexture(res, onProgress, isHeightmap, textureFormat, isMipChain, isLinear);
                    textureList.AddRange(texs);
                }
                else
                {
                    textureList.Add(tex);
                }
            }
            return textureList;
        }

        public static Texture2DArray Create2DArray(List<Texture2D> textures, int width, int height, TextureFormat tf,bool mipChain,bool linear)
        {
            var arr = new Texture2DArray(width, height, textures.Count, tf, mipChain, linear);
            textures.ForEach((tex, id) =>
            {
                Graphics.CopyTexture(tex, 0, arr, id);
            });
            return arr;
        }

        public static Texture2DArray Create2DArray(List<Texture2D> textures,bool linear)
        {
            var q = textures.Where(t => t);
            if (q.Count()==0)
                return null;

            var sample = textures[0];
            
            return Create2DArray(textures, sample.width, sample.height, sample.format,true, linear);
        }
    }
}
