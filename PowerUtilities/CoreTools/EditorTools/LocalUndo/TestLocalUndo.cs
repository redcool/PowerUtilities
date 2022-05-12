namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;



#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(TestLocalUndo))]
    public class TestUndoE : Editor
    {

        private void OnEnable()
        {
            LocalUndo.Clear();
        }
        public override void OnInspectorGUI()
        {
            var inst = target as TestLocalUndo;

            GUILayout.Label("undo index: " + LocalUndo.Index);
            GUILayout.Label("undo max index: " + LocalUndo.MaxIndex);


            if (GUILayout.Button("Undo"))
            {
                LocalUndo.PerformUndo();
            }

            if (GUILayout.Button("record"))
            {
                LocalUndo.Record(inst.gameObject, "change go name");
                //Undo.RecordObject(inst.gameObject, inst.name);
                inst.gameObject.name = "abc";
            }

            if (GUILayout.Button("redo"))
            {
                LocalUndo.PerformRedo();
            }

            if (GUILayout.Button("Clear Undo"))
            {
                LocalUndo.Clear();
            }
        }
    }

    public class TestLocalUndo : MonoBehaviour
    {
    }
#endif

}