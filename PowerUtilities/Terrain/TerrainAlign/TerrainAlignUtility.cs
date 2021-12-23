namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(TerrainAlignUtility))]
    public class TerrainAlignUtilityEditor : Editor
    {
        const string REFRESH_TERRAIN = "获取Terrains";
        const string REALIGN_TERRAIN = "对齐Terrains";

        TerrainAlignUtility inst;

        void RealignTerrains(Terrain[] terrains,int countInRow,Vector3 terrainSize)
        {
            for (int i = 0; i < inst.terrains.Length; i++)
            {
                var colId = i / inst.countInRow;
                var rowId = i % inst.countInRow;

                var pos = new Vector3(rowId, 0, colId);

                var item = inst.terrains[i];
                item.terrainData.size = inst.terrainSize;
                item.transform.position = Vector3.Scale(inst.terrainSize, pos);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            inst = target as TerrainAlignUtility;

            serializedObject.UpdateIfRequiredOrScript();


            if (GUILayout.Button(REFRESH_TERRAIN))
            {
                inst.terrains = inst.GetComponentsInChildren<Terrain>();
            }

            if (inst.terrains != null && GUILayout.Button(REALIGN_TERRAIN))
            {
                RealignTerrains(inst.terrains, inst.countInRow, inst.terrainSize);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    public class TerrainAlignUtility : MonoBehaviour
    {
        public Vector3 terrainSize = new Vector3(1000,600,1000);
        public int countInRow = 2;
        public Terrain[] terrains;

    }
}