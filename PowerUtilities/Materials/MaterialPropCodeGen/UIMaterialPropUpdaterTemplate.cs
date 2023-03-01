using UnityEngine;

namespace PowerUtilities
{
    [ExecuteInEditMode]
    public class UIMaterialPropUpdaterTemplate : BaseUIMaterialPropUpdater
    {
        public Color color = Color.white;

        public override void ReadFirstMaterial(Material mat)
        {
            if (!mat)
                return;

            color= mat.color;
        }

        public override void UpdateMaterial(Material mat)
        {
            if (!mat)
                return;

            mat.SetColor("_Color", color);
        }

        public override void UpdateBlock(MaterialPropertyBlock block)
        {
            if (block == null)
                return;

            block.SetColor("_Color",color);
        }
    }
}
