#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public static class MaterialContextMenu
    {
        [MenuItem(ContextMenuConsts.CONTEXT+ "Material/Clear Unused Properties")]
        public static void ClearUnusedMaterialVariables(MenuCommand menuCmd)
        {
            var mat = menuCmd.context as Material;
            if (!mat || !mat.shader)
                return;

            MaterialEditorEx.RemoveUnusedMaterialProperties(mat);
        }

        [MenuItem(ContextMenuConsts.POWER_UTILS_MENU + "Material/Clear Unused Properties In Folder")]
        public static void ClearUnusedMaterialVariablesFolder(MenuCommand menuCmd)
        {
            var folders = SelectionTools.GetSelectedFolders();
            foreach (var folder in folders)
            {
                var mats = AssetDatabaseTools.FindAssetsInProject<Material>("", folder);
                foreach (var mat in mats)
                {
                    if (!mat || !mat.shader)
                        continue;
                    MaterialEditorEx.RemoveUnusedMaterialProperties(mat);
                }
            }

        }
    }
}
#endif