using UnityEngine;

namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// Communication with TargetTrackTrace
    /// </summary>
    [ExecuteAlways]
    public class TargetTrackTraceMono : MonoBehaviour
    {
        public Material sceneFogMat;
        public Transform minPosTr, maxPosTr;

        public Renderer r;
        public bool isClear;

        public void OnEnable()
        {
            SRPPass<TargetTrackTrace>.OnBeforeExecute -= OnBeforeExecute;
            SRPPass<TargetTrackTrace>.OnBeforeExecute += OnBeforeExecute;

            SRPPass<TargetTrackTrace>.OnEndExecute -= OnEndExecute;
            SRPPass<TargetTrackTrace>.OnEndExecute+= OnEndExecute;

            r = GetComponent<Renderer>();
        }

        private void OnDisable()
        {
            SRPPass<TargetTrackTrace>.OnBeforeExecute -= OnBeforeExecute;
            SRPPass<TargetTrackTrace>.OnEndExecute -= OnEndExecute;
        }

        void OnEndExecute(SRPPass<TargetTrackTrace> pass)
        {
            if (!enabled)
                return;

            UpdateMaterial(sceneFogMat, pass.Feature.trackRT);

            if (r)
            {
                var mat = Application.isPlaying ? r.material : r.sharedMaterial;
                UpdateMaterial(mat, pass.Feature.trackRT);
            }
        }
        void OnBeforeExecute(SRPPass<TargetTrackTrace> pass)
        {
            if (!enabled)
                return;

            if (minPosTr)
                pass.Feature.minPos = minPosTr.position;
            if (maxPosTr)
                pass.Feature.maxPos = maxPosTr.position;

            if(CompareTools.IsTiggerOnce(ref isClear))
            {
                pass.Feature.isClear = false;
            }
        }

        private void UpdateMaterial(Material mat, Texture rt)
        {
            if (mat)
            {
                mat.SetVector("_MinWorldPos", minPosTr.position);
                mat.SetVector("_MaxWorldPos", maxPosTr.position);
                mat.SetTexture("_SceneFogMap", rt);
            }
        }
    }
}
