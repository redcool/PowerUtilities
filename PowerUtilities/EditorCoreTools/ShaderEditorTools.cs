#if UNITY_EDITOR
using System.Linq;
using System.Text;
using UnityEngine;

namespace PowerUtilities
{
    public static class ShaderEditorTools
    {
        /// <summary>
        /// Get material use shader from selected folder or assets folder
        /// </summary>
        /// <param name="shader"></param>
        /// <returns></returns>
        public static Material[] GetMaterialsRefShader(this Shader shader)
        {
            var paths = SelectionTools.GetSelectedFolders(true);

            var mats = AssetDatabaseTools.FindAssetsInProject<Material>("t:Material", paths);
            mats = mats.Where(mat => mat.shader == shader).ToArray();
            return mats;
        }

        /// <summary>
        /// Clear material's keyword that use shader
        /// </summary>
        /// <param name="shader"></param>
        public static void ClearMaterialKeywords(this Shader shader)
        {
            Material[] mats = shader.GetMaterialsRefShader();
            foreach (var mat in mats)
            {
                mat.shaderKeywords = null;
            }
        }
    }
}
#endif